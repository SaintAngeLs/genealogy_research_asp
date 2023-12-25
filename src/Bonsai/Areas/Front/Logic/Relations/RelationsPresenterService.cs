using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Strings;

namespace Bonsai.Areas.Front.Logic.Relations
{
    /// <summary>
    /// The service for calculating relations between pages.
    /// </summary>
    public partial class RelationsPresenterService
    {
        public RelationsPresenterService(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

         #region Relation definitions

        /// <summary>
        /// Relations for parents/siblings group.
        /// </summary>
        public static RelationDefinition[] ParentRelations =
        {
            new RelationDefinition("Parent:m", "Father"),
            new RelationDefinition("Parent:f", "Mother"),
            new RelationDefinition("Parent Child:m", "Brother", "Brothers"),
            new RelationDefinition("Parent Child:f", "Sister", "Sisters"),
            new RelationDefinition("Parent Parent:m", "Grandfather", "Grandfathers"),
            new RelationDefinition("Parent Parent:f", "Grandmother", "Grandmothers")
        };

        /// <summary>
        /// Relations for a spouse-based group.
        /// </summary>
        public static RelationDefinition[] SpouseDefinitions =
        {
            new RelationDefinition("!Spouse:m", "Husband", null, RelationDurationDisplayMode.RelationRange), 
            new RelationDefinition("!Spouse:f", "Wife", null, RelationDurationDisplayMode.RelationRange),
            new RelationDefinition("!Spouse Child+Child", "Son|Daughter|Child", "Children", RelationDurationDisplayMode.Birth),
            new RelationDefinition("!Spouse:m Parent:m", "Father-in-law"),
            new RelationDefinition("!Spouse:m Parent:f", "Mother-in-law"),
            new RelationDefinition("!Spouse:f Parent:m", "Father-in-law"),
            new RelationDefinition("!Spouse:f Parent:f", "Mother-in-law"),
            new RelationDefinition("!Spouse:m Parent Child:m", "Brother-in-law", "Brothers-in-law"),
            new RelationDefinition("!Spouse:m Parent Child:f", "Sister-in-law", "Sisters-in-law"),
            new RelationDefinition("!Spouse:f Parent Child:m", "Brother-in-law", "Brothers-in-law"),
            new RelationDefinition("!Spouse:f Parent Child:f", "Sister-in-law", "Sisters-in-law")
        };

        /// <summary>
        /// Other relations for family members.
        /// </summary>
        public static RelationDefinition[] OtherRelativeRelations =
        {
            new RelationDefinition("Child-Spouse Child", "Son|Daughter|Child", "Children", RelationDurationDisplayMode.Birth),
            new RelationDefinition("Child Child", "Grandson|Granddaughter|Grandchild", "Grandchildren", RelationDurationDisplayMode.Birth),
            new RelationDefinition("Child:f Spouse:m", "Son-in-law", "Sons-in-law"),
            new RelationDefinition("Child:m Spouse:f", "Daughter-in-law", "Daughters-in-law"),
            new RelationDefinition("Pet", "Pet", "Pets"),
        };

        /// <summary>
        /// Relations for other people.
        /// </summary>
        private static RelationDefinition[] NonRelativeRelations =
        {
            new RelationDefinition("Friend", "Friend", "Friends"),
            new RelationDefinition("Colleague", "Colleague", "Colleagues"),
            new RelationDefinition("Owner", "Owner", "Owners", RelationDurationDisplayMode.RelationRange),
            new RelationDefinition("EventVisitor", "Participant", "Participants"),
            new RelationDefinition("LocationInhabitant", "Resident", "Residents"),
        };

        /// <summary>
        /// Relations for non-human pages.
        /// </summary>
        private static RelationDefinition[] NonHumanRelations =
        {
            new RelationDefinition("Location", "Location", "Locations"),
            new RelationDefinition("Event", "Event", "Events"),
        };

        #endregion

        #region Public methods

        /// <summary>
        /// Returns the list of all inferred relation groups for the page.
        /// </summary>
        public async Task<IReadOnlyList<RelationCategoryVM>> GetRelationsForPage(Guid pageId)
        {
            var ctx = await RelationContext.LoadContextAsync(_db);

            var cats = new []
            {
                new RelationCategoryVM
                {
                    Title = "Relatives",
                    IsMain = true,
                    Groups = GetGroups(ctx, pageId, ParentRelations)
                        .Concat(GetSpouseGroups(ctx, pageId))
                        .Concat(GetGroups(ctx, pageId, OtherRelativeRelations))
                        .ToList()
                },
                new RelationCategoryVM
                {
                    Title = "People",
                    Groups = GetGroups(ctx, pageId, NonRelativeRelations).ToList(),
                },
                new RelationCategoryVM
                {
                    Title = "Pages",
                    Groups = GetGroups(ctx, pageId, NonHumanRelations).ToList(),
                }
            };

            return cats.Where(x => x.Groups.Any()).ToList();
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Returns the relation groups with parents and siblings.
        /// </summary>
        private IEnumerable<RelationGroupVM> GetGroups(RelationContext ctx, Guid pageId, RelationDefinition[] defs)
        {
            var ids = new[] {pageId};
            var relations = defs.Select(x => GetRelationVM(ctx, x, ids))
                                           .Where(x => x != null)
                                           .ToList();

            if (relations.Any())
                yield return new RelationGroupVM {Relations = relations};
        }

        /// <summary>
        /// Returns the groups for each spouse-based family.
        /// </summary>
        private IEnumerable<RelationGroupVM> GetSpouseGroups(RelationContext ctx, Guid pageId)
        {
            if (!ctx.Pages.TryGetValue(pageId, out var page))
                yield break;

            if (page.Type != PageType.Person && page.Type != PageType.Pet)
                yield break;

            if (!ctx.Relations.ContainsKey(pageId))
                yield break;

            var spouses = ctx.Relations[pageId]
                             .Where(x => x.Type == RelationType.Spouse)
                             .OrderBy(x => x.Duration);

            foreach (var spouse in spouses)
            {
                var ids = new[] {pageId, spouse.DestinationId};
                var relations = SpouseDefinitions.Select(x => GetRelationVM(ctx, x, ids))
                                                 .Where(x => x != null)
                                                 .ToList();

                if (relations.Any())
                    yield return new RelationGroupVM {Relations = relations};
            }
        }

        /// <summary>
        /// Returns a relation for all pages matching the definition.
        /// </summary>
        private RelationVM GetRelationVM(RelationContext ctx, RelationDefinition def, params Guid[] guids)
        {
            // Performs one step from the current page along the relation path and returns matching pages
            IEnumerable<RelationTarget> Step(RelationTarget elem, RelationPathSegment segment, Guid? guidFilter)
            {
                if(!ctx.Relations.TryGetValue(elem.Page.Id, out var rels))
                    return Enumerable.Empty<RelationTarget>();

                return from rel in rels
                       where rel.Type == segment.Type
                       where guidFilter == null || rel.DestinationId == guidFilter
                       let page = ctx.Pages[rel.DestinationId]
                       where segment.Gender == null || segment.Gender == page.Gender
                       where !elem.VisitedPages.Contains(page)
                       select new RelationTarget(page, rel, elem.VisitedPages.Append(page));
            }
            
            // Finds pages matching the entire path from current page
            IEnumerable<RelationTarget> GetMatchingPages(RelationPath path)
            {
                if (!ctx.Pages.TryGetValue(guids[0], out var root))
                    return Array.Empty<RelationTarget>();

                var currents = new List<RelationTarget> {new RelationTarget(root, null, new SinglyLinkedList<RelationContext.PageExcerpt>(root))};

                for (var depth = 0; depth < path.Segments.Count; depth++)
                {
                    if (currents.Count == 0)
                        break;

                    var segment = path.Segments[depth];
                    var guidFilter = path.IsBound && (depth + 1) < guids.Length
                        ? guids[depth + 1]
                        : (Guid?) null;

                    currents = currents.Select(x => Step(x, segment, guidFilter))
                                       .SelectMany(x => x)
                                       .ToList();
                }

                return currents;
            }
            
            // Gets the range to display alongside the relation
            FuzzyRange? GetRange(RelationTarget elem)
            {
                if (def.DurationDisplayMode == RelationDurationDisplayMode.RelationRange)
                    return elem.Relation.Duration;

                if (def.DurationDisplayMode == RelationDurationDisplayMode.Birth)
                    if (elem.Page.BirthDate != null)
                        return new FuzzyRange(elem.Page.BirthDate, null);

                if (def.DurationDisplayMode == RelationDurationDisplayMode.Life)
                    if(elem.Page.BirthDate != null || elem.Page.DeathDate != null)
                        return new FuzzyRange(elem.Page.BirthDate, elem.Page.DeathDate);

                return null;
            }

            PageTitleVM GetEventPageTitle(Guid? eventId)
            {
                if (eventId == null)
                    return null;

                var page = ctx.Pages[eventId.Value];
                return new PageTitleVM
                {
                    Title = page.Title,
                    Key = page.Key
                };
            }

            var posPaths = def.Paths.Where(x => !x.IsExcluded);
            var negPaths = def.Paths.Where(x => x.IsExcluded);

            // A+B-C means: all pages matching both paths A & B, but not matching path C
            var results = posPaths.Select(GetMatchingPages)
                                  .Aggregate((a, b) => a.Intersect(b))
                                  .Except(negPaths.Select(GetMatchingPages)
                                                  .SelectMany(x => x))
                                  .ToList();
             
            if(!results.Any())
                return null;

            return new RelationVM
            {
                Title = def.GetName(results.Count, results[0].Page.Gender),
                Pages = results.OrderBy(x => x.Page.BirthDate)
                               .Select(elem => new RelatedPageVM
                               {
                                   Title = StringHelper.Coalesce(elem.Page.ShortName, elem.Page.Title),
                                   Key = elem.Page.Key,
                                   Duration = GetRange(elem),
                                   RelationEvent = GetEventPageTitle(elem.Relation.EventId)
                               })
                               .ToList()
            };
        }

        #endregion
    }
}

