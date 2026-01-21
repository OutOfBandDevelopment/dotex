using System.ComponentModel;

namespace OoBDev.DocumentCenter.Abstractions
{
    public enum DocumentTypes
    {
        [MimeType("application/octet-stream")]
        Unknown = 0,

        [MimeType("text/plain"), FileExtension(".txt"), FileExtension(".text"), Description("Text file")]
        Text = 1,

        [MimeType("text/html"), FileExtension(".html"), FileExtension(".htm"), Description("Hypertext Markup Language")]
        Html = 2,

        [MimeType("application/pdf"), FileExtension(".pdf"), Description("Portable Document Format")]
        Pdf = 3,

        [MimeType("text/markdown"), FileExtension(".md"), FileExtension(".markdown"), Description("Markdown")]
        Markdown = 4,

        [MimeType("image/jpeg"), FileExtension(".jpg"), FileExtension(".jpeg"), Description("Joint Photographic Experts Group")]
        Jpeg = 5,

        [MimeType("image/png"), FileExtension(".png"), Description("Portable Network Graphics")]
        Png = 6,

        [MimeType("application/json"), MimeType("text/json"), FileExtension(".json"), Description("JavaScript Object Notation")]
        Json = 7,

        [MimeType("application/xml"), MimeType("text/xml"), FileExtension(".xml"), Description("Extensible Markup Language")]
        Xml = 8,

        [MimeType("application/zip"), FileExtension(".zip"), Description("ZIP Compressed Archive")]
        Zip = 9,
    }
}
