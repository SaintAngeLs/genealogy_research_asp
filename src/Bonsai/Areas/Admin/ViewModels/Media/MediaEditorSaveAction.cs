using System.ComponentModel;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    /// <summary>
    /// Possible actions to execute during media editor save.
    /// </summary>
    public enum MediaEditorSaveAction
    {
        /// <summary>
        /// Just save the current editor.
        /// </summary>
        [Description("Save")]
        Save = 0,

        /// <summary>
        /// Save the editor and edit the next media without tags.
        /// </summary>
        [Description("Save and open other")]
        SaveAndShowNext = 1,
    }
}
