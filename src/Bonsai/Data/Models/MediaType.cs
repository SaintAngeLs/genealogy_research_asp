using System;
using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Type of the uploaded media file.
    /// </summary>
    public enum MediaType
    {
        [Description("Photo")]
        Photo,

        [Description("PhtoSphere")]
        [Obsolete("Not yet implemented")]
        Photo360,

        [Description("Video")]
        Video,

        [Description("Document")]
        Document
    }
}
