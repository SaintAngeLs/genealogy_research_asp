using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Known account roles.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Newly registered user.
        /// </summary>
        [Description("New")]
        Unvalidated,

        /// <summary>
        /// Basic user.
        /// </summary>
        [Description("User")]
        User,

        /// <summary>
        /// Page editor.
        /// </summary>
        [Description("Editor")]
        Editor,

        /// <summary>
        /// Almighty administator
        /// </summary>
        [Description("Admin")]
        Admin
    }
}
