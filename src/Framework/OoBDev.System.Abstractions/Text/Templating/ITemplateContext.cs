using System;
using System.IO;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Represents the context for a template, including information about the template and target content types, source, and priority.
/// </summary>
public interface ITemplateContext
{
    /// <summary>
    /// Gets the name of the template.
    /// </summary>
    string TemplateName { get; }

    /// <summary>
    /// Gets the content type of the template.
    /// </summary>
    string TemplateContentType { get; }

    /// <summary>
    /// Gets the file extension of the template.
    /// </summary>
    string TemplateFileExtension { get; }

    /// <summary>
    /// Gets the source of the template.
    /// </summary>
    ITemplateSource TemplateSource { get; }

    /// <summary>
    /// Gets the reference identifier for the template.
    /// </summary>
    string TemplateReference { get; }

    /// <summary>
    /// Gets the function that opens the template as a stream.
    /// </summary>
    Func<ITemplateContext, Stream> OpenTemplate { get; }

    /// <summary>
    /// Gets the content type of the target.
    /// </summary>
    string TargetContentType { get; }

    /// <summary>
    /// Gets the file extension of the target.
    /// </summary>
    string TargetFileExtension { get; }

    /// <summary>
    /// Gets the priority of the template, used to determine the order of template application.
    /// </summary>
    int Priority { get; }
}
