namespace OoBDev.System.Text.Templating.EntityFrameworkCore;

/// <summary>A new version of a template to publish.</summary>
/// <param name="Name">The template name.</param>
/// <param name="ContentType">The template content type, which selects the template provider.</param>
/// <param name="Content">The template text.</param>
/// <param name="CreatedBy">Who publishes it (the audit trail).</param>
/// <param name="Culture">The culture code, or empty for the neutral template.</param>
/// <param name="TenantId">The tenant, or empty for every tenant.</param>
/// <param name="TargetContentType">The content type the template produces.</param>
/// <param name="TargetFileExtension">The file extension of the produced content.</param>
/// <param name="Description">An optional note on the change.</param>
public sealed record TemplatePublish(
    string Name,
    string ContentType,
    string Content,
    string CreatedBy,
    string Culture = "",
    string TenantId = "",
    string TargetContentType = "text/plain",
    string TargetFileExtension = ".txt",
    string? Description = null);
