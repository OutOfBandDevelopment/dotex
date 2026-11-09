using System.Text.Json.Nodes;

namespace OoBDev.WebApi.Models;

/// <summary>A draft template text to render with sample data; nothing is saved.</summary>
public class TemplatePreviewRequestModel
{
    /// <summary>Gets or sets the draft template text.</summary>
    public required string Text { get; set; }

    /// <summary>Gets or sets the template engine content type; Handlebars when empty.</summary>
    public string? TemplateContentType { get; set; }

    /// <summary>Gets or sets the extension of the document the template produces (for example <c>.html</c>).</summary>
    public string TargetExtension { get; set; } = ".txt";

    /// <summary>Gets or sets the data given to the template.</summary>
    public JsonNode? Data { get; set; }
}
