using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Kind of the change.
    /// </summary>
    public enum ChangesetType
    {
        /// <summary>
        /// The entity has been created.
        /// </summary>
        [Description("Created")]
        Created,

        /// <summary>
        /// The entity has been updated.
        /// </summary>
        [Description("Changed")]
        Updated,

        /// <summary>
        /// The entity has been removed.
        /// </summary>
        [Description("Deleted")]
        Removed,

        /// <summary>
        /// The previous changeset has been reverted.
        /// </summary>
        [Description("Restored")]
        Restored
    }
}
