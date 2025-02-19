namespace OoBDev.System.Net.Mime;

/// <summary>
/// Provides constants representing various content types.
/// </summary>
public static class ContentTypesExtensions
{
    // https://www.iana.org/assignments/media-types/media-types.xhtml#text

    /// <summary>
    /// Represents text-based content types.
    /// </summary>
    public static class Text
    {
        /// <summary>
        /// Represents the content type for Handlebars templates.
        /// </summary>
        public const string HandlebarsTemplate = "text/x-handlebars-template";

        /// <summary>
        /// Represents the content type for calendar data.
        /// </summary>
        public const string Calendar = "text/calendar";

        /// <summary>
        /// Represents the content type for HTML.
        /// </summary>
        public const string Html = "text/html";

        /// <summary>
        /// Represents the content type for Markdown.
        /// </summary>
        public const string Markdown = "text/markdown";

        /// <summary>
        /// Represents the content type for Yaml.
        /// </summary>
        public const string Yaml = "text/yaml";

        /// <summary>
        /// Represents the content type for Plain text.
        /// </summary>
        public const string Plain = "text/plain";
    }

    /// <summary>
    /// Represents application-based content types.
    /// </summary>
    public static class Application
    {
        /// <summary>
        /// Represents the content type for XSLT (XML Stylesheet Language Transformations).
        /// </summary>
        public const string XSLT = "application/xslt+xml";
    }
}
