namespace OoBDev.AspNetCore.EntityFrameworkCore;

/// <summary>A stored right check.</summary>
public sealed class RightAuditRow
{
    /// <summary>Gets or sets the identity.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the UTC ticks of the check (ticks, because SQLite cannot order a DateTimeOffset).</summary>
    public long AtTicks { get; set; }

    /// <summary>Gets or sets the caller.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Gets or sets the required rights.</summary>
    public string Rights { get; set; } = string.Empty;

    /// <summary>Gets or sets the outcome.</summary>
    public string Result { get; set; } = string.Empty;
}
