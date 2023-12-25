using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Kinds of entity relationships.
    /// </summary>
    public enum RelationType
    {
        [Description("Parent")]
        Parent,
        [Description("Child")]
        Child,
        [Description("Spouse")]
        Spouse,

        [Description("StepParent")]
        StepParent,
        [Description("StepChild")]
        StepChild,

        [Description("Friend")]
        Friend,
        [Description("Colleague")]
        Colleague,

        [Description("Owner")]
        Owner,
        [Description("Pet")]
        Pet,

        [Description("Location")]
        Location,
        [Description("Location Inhabitant")]
        LocationInhabitant,

        [Description("Event")]
        Event,
        [Description("Event Visitor")]
        EventVisitor,

        [Description("Other")]
        Other
    }
}
