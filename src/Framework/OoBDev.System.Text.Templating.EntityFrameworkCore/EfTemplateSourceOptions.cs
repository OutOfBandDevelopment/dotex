namespace OoBDev.System.Text.Templating.EntityFrameworkCore;

/// <summary>Options of <see cref="EfTemplateSource"/>.</summary>
public sealed class EfTemplateSourceOptions
{
    /// <summary>The tenant whose templates are served (its own first, then the shared ones). Empty serves shared templates only.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>The priority of contexts from this source; lower wins against other sources.</summary>
    public int Priority { get; set; }
}
