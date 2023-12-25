using System.Collections.Generic;
using System.Linq;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// The mapping between facts and their corresponding templates.
    /// </summary>
    public static class FactDefinitions
    {
        static FactDefinitions()
        {
            Groups = new Dictionary<PageType, FactDefinitionGroup[]>
            {
                [PageType.Person] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        "Main",
                        true,
                        new FactDefinition<HumanNameFactModel>("Name", "Name", "Name|Names")
                    ),
                    new FactDefinitionGroup(
                        "Birth",
                        "Birth",
                        true,
                        new FactDefinition<BirthDateFactModel>("Date", "Date of Birth", "Date"),
                        new FactDefinition<StringFactModel>("Place", "Place of Birth", "Place")
                    ),
                    new FactDefinitionGroup(
                        "Death",
                        "Death",
                        true,
                        new FactDefinition<DeathDateFactModel>("Date", "Date of Death", "Date"),
                        new FactDefinition<StringFactModel>("Place", "Place of Death", "Place"),
                        new FactDefinition<StringFactModel>("Cause", "Cause of Death", "Cause"),
                        new FactDefinition<StringFactModel>("Burial", "Burial Place")
                    ),
                    new FactDefinitionGroup(
                        "Bio",
                        "Biology",
                        false,
                        new FactDefinition<GenderFactModel>("Gender", "Gender"),
                        new FactDefinition<BloodTypeFactModel>("Blood", "Blood Group", "Blood Gr."),
                        new FactDefinition<StringFactModel>("Eyes", "Eye Color", "Eyes"),
                        new FactDefinition<StringFactModel>("Hair", "Hair Color", "Hair")
                    ),
                    new FactDefinitionGroup(
                        "Person",
                        "Personality",
                        false,
                        new FactDefinition<LanguageFactModel>("Language", "Language", "Language|Languages"),
                        new FactDefinition<SkillFactModel>("Skill", "Hobby"),
                        new FactDefinition<StringListFactModel>("Profession", "Profession", "Profession|Professions"),
                        new FactDefinition<StringListFactModel>("Religion", "Religion", "Religion|Religions")
                    ),
                    new FactDefinitionGroup(
                        "Meta",
                        "Other",
                        false,
                        new FactDefinition<SocialProfilesFactModel>("SocialProfiles", "Social Networks")
                    )
                },

                [PageType.Pet] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        "Main",
                        true,
                        new FactDefinition<NameFactModel>("Name", "Name")
                    ),
                    new FactDefinitionGroup(
                        "Birth",
                        "Birth",
                        true,
                        new FactDefinition<BirthDateFactModel>("Date", "Date of Birth", "Date"),
                        new FactDefinition<StringFactModel>("Place", "Place of Birth", "Place")
                    ),
                    new FactDefinitionGroup(
                        "Death",
                        "Death",
                        true,
                        new FactDefinition<DeathDateFactModel>("Date", "Date of Death", "Date"),
                        new FactDefinition<StringFactModel>("Place", "Place of Death", "Place"),
                        new FactDefinition<StringFactModel>("Cause", "Cause of Death", "Cause"),
                        new FactDefinition<StringFactModel>("Burial", "Burial Place")
                    ),
                    new FactDefinitionGroup(
                        "Bio",
                        "Biology",
                        true,
                        new FactDefinition<GenderFactModel>("Gender", "Gender"),
                        new FactDefinition<StringFactModel>("Species", "Species"),
                        new FactDefinition<StringFactModel>("Breed", "Breed"),
                        new FactDefinition<StringFactModel>("Color", "Color")
                    )
                },

                [PageType.Location] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        "Main",
                        true,
                        new FactDefinition<AddressFactModel>("Location", "Address"),
                        new FactDefinition<DateFactModel>("Opening", "Acquisition"),
                        new FactDefinition<DateFactModel>("Shutdown", "Sale")
                    )
                },

                [PageType.Event] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        "Main",
                        true,
                        new FactDefinition<DateFactModel>("Date", "Date")
                    )
                },

                [PageType.Other] = new FactDefinitionGroup[0]
            };

            Definitions = Groups.ToDictionary(
                x => x.Key,
                x => x.Value.SelectMany(y => y.Defs.Select(z => new { Key = y.Id + "." + z.Id, Fact = z }))
                      .ToDictionary(y => y.Key, y => y.Fact)
            );
        }

        /// <summary>
        /// Available groups of fact definitions.
        /// </summary>
        public static readonly Dictionary<PageType, FactDefinitionGroup[]> Groups;

        /// <summary>
        /// Lookup for fact definitions.
        /// </summary>
        public static readonly Dictionary<PageType, Dictionary<string, IFactDefinition>> Definitions;

        /// <summary>
        /// Finds a definition.
        /// </summary>
        public static IFactDefinition TryGetDefinition(PageType type, string key)
        {
            return Definitions.TryGetValue(type, out var pageLookup)
                   && pageLookup.TryGetValue(key, out var def)
                ? def
                : null;
        }
    }
}
