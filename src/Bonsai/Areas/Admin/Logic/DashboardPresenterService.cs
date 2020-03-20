﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Admin.ViewModels.Users;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for displaying dashboard info.
    /// </summary>
    public class DashboardPresenterService
    {
        public DashboardPresenterService(AppDbContext db, IWebHostEnvironment env, IUrlHelper url, IMapper mapper)
        {
            _db = db;
            _env = env;
            _url = url;
            _mapper = mapper;
        }

        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IUrlHelper _url;
        private readonly IMapper _mapper;

        /// <summary>
        /// Returns the dashboard data.
        /// </summary>
        public async Task<DashboardVM> GetDashboardAsync()
        {
            return new DashboardVM
            {
                Events = await GetEventsAsync().ToListAsync(),
                PagesCount = await _db.Pages.CountAsync(x => x.IsDeleted == false),
                PagesToImproveCount = await _db.PagesScored.CountAsync(x => x.IsDeleted == false && x.CompletenessScore <= 50),
                MediaCount = await _db.Media.CountAsync(x => x.IsDeleted == false),
                MediaToTagCount = await _db.Media.CountAsync(x => x.IsDeleted == false && !x.Tags.Any()),
                RelationsCount = await _db.Relations.CountAsync(x => x.IsComplementary == false && x.IsDeleted == false)
            };
        }

        /// <summary>
        /// Returns the list of changeset groups at given offset.
        /// </summary>
        public async IAsyncEnumerable<ChangesetEventVM> GetEventsAsync(int page = 0)
        {
            const int PAGE_SIZE = 20;

            var groups = await _db.ChangeEvents
                                  .OrderByDescending(x => x.Date)
                                  .Skip(PAGE_SIZE * page)
                                  .Take(PAGE_SIZE)
                                  .ToListAsync();

            var parsedGroups = groups.Select(x => new {x.GroupKey, Ids = x.Ids.Split(',').Select(y => y.Parse<Guid>())})
                                     .ToList();

            var changeIds = parsedGroups.SelectMany(x => x.Ids).ToList();
            var changes = await _db.Changes
                                   .AsNoTracking()
                                   .Include(x => x.EditedMedia)
                                   .Include(x => x.EditedPage)
                                   .Include(x => x.EditedRelation.Destination)
                                   .Include(x => x.EditedRelation.Source)
                                   .Include(x => x.Author)
                                   .Where(x => changeIds.Contains(x.Id))
                                   .ToDictionaryAsync(x => x.Id, x => x);

            foreach (var group in parsedGroups)
            {
                var chg = changes[group.Ids.First()];
                var vm = _mapper.Map<ChangesetEventVM>(chg);
                if (chg.Type == ChangesetEntityType.Page)
                {
                    vm.MainLink = GetLinkToPage(chg.EditedPage);
                }
                else if (chg.Type == ChangesetEntityType.Relation)
                {
                    var rel = chg.EditedRelation;
                    vm.MainLink = new LinkVM
                    {
                        Title = chg.EditedRelation.Type.GetEnumDescription(),
                        Url = _url.Action("Update", "Relations", new { area = "Admin", id = rel.Id })
                    };
                    vm.ExtraLinks = new[] {GetLinkToPage(rel.Destination), GetLinkToPage(rel.Source)};
                }
                else if (chg.Type == ChangesetEntityType.Media)
                {
                    vm.MediaThumbnails = group.Ids
                                              .Select(x => changes[x].EditedMedia)
                                              .Where(x => File.Exists(_env.GetMediaPath(x)))
                                              .Select(x => new MediaThumbnailVM
                                              {
                                                  Key = x.Key,
                                                  Type = x.Type,
                                                  ThumbnailUrl = MediaPresenterService.GetSizedMediaPath(x.FilePath, MediaSize.Small),
                                              })
                                              .ToList();
                }

                yield return vm;
            }
        }

        /// <summary>
        /// Builds a link to the page.
        /// </summary>
        private LinkVM GetLinkToPage(Page page)
        {
            return new LinkVM
            {
                Title = page.Title,
                Url = _url.Action("Description", "Page", new {area = "Front", key = page.Key})
            };
        }
    }
}
