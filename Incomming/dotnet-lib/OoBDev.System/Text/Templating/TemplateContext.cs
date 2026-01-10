using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Represents the context of a template, providing information about the template and its processing.
/// </summary>
[ExcludeFromCodeCoverage]
public record TemplateContext : ITemplateContext
{
    /// <summary>
    /// Gets or sets the name of the template.
    /// </summary>
    public required string TemplateName { get; set; }

    /// <summary>
    /// Gets or sets the content type of the template.
    /// </summary>
    public required string TemplateContentType { get; set; }

    /// <summary>
    /// Gets or sets the file extension of the template.
    /// </summary>
    public required string TemplateFileExtension { get; set; }

    /// <summary>
    /// Gets or sets the source of the template.
    /// </summary>
    public required ITemplateSource TemplateSource { get; set; }

    /// <summary>
    /// Gets or sets the reference identifier of the template.
    /// </summary>
    public required string TemplateReference { get; set; }

    /// <summary>
    /// Gets or sets the function to open the template as a stream.
    /// </summary>
    public required Func<ITemplateContext, Stream> OpenTemplate { get; set; }

    /// <summary>
    /// Gets or sets the content type of the target.
    /// </summary>
    public required string TargetContentType { get; set; }

    /// <summary>
    /// Gets or sets the file extension of the target.
    /// </summary>
    public required string TargetFileExtension { get; set; }

    /// <summary>
    /// Gets or sets the priority of the template.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Returns a string representation of the template context.
    /// </summary>
    /// <returns>A string representation of the template context.</returns>
    public override string ToString() =>
        string.Join("",
            TemplateName,
            TargetFileExtension == TemplateFileExtension ? null : TargetFileExtension,
            TemplateFileExtension,
            $" ({TemplateContentType} -> {TargetContentType})"
        );
}
