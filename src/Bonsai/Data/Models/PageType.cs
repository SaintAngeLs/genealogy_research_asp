using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Type of an entity described by the page.
    /// </summary>
    public enum PageType
    {
        [Description("Person")]
        Person = 0,

        [Description("Pet")]
        Pet = 1,

        [Description("Event")]
        Event = 2,

        [Description("Place")]
        Location = 3,

        [Description("Others")]
        Other = 4
    }
}
