namespace OoBDev.WebApi.Models;

/// <summary>A template file found in the template folder.</summary>
public class TemplateInfoModel
{
    /// <summary>Gets or sets the template name (the file name without the target and template extensions).</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the file name.</summary>
    public required string File { get; set; }

    /// <summary>Gets or sets the template engine content type.</summary>
    public required string TemplateContentType { get; set; }

    /// <summary>Gets or sets the extension of the produced document (for example <c>.html</c>); empty when the file name has none.</summary>
    public required string TargetExtension { get; set; }
}
