namespace OoBDev.System.Text.Templating.EntityFrameworkCore;

/// <summary>
/// One version of a template. A published version is never changed; a new version is a new row. <see cref="Culture"/> and
/// <see cref="TenantId"/> are empty strings (not null) when absent so the unique index behaves the same on every database.
/// </summary>
public sealed class TemplateRow
{
    /// <summary>Row id.</summary>
    public long Id { get; set; }

    /// <summary>The template name, without culture or version.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The version number, starting at 1 and rising by one per publish.</summary>
    public int Version { get; set; }

    /// <summary>The culture code such as <c>es-ES</c>, or empty for the neutral template.</summary>
    public string Culture { get; set; } = string.Empty;

    /// <summary>The tenant, or empty for a template shared by every tenant.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>The template content type, which selects the template provider.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>The content type the template produces.</summary>
    public string TargetContentType { get; set; } = "text/plain";

    /// <summary>The file extension of the produced content.</summary>
    public string TargetFileExtension { get; set; } = ".txt";

    /// <summary>The template text.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>False once the template is withdrawn; withdrawn rows stay for the audit trail.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Optional note on the change.</summary>
    public string? Description { get; set; }

    /// <summary>Who published this version.</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>UTC ticks when this version was published.</summary>
    public long CreatedAtTicks { get; set; }
}
