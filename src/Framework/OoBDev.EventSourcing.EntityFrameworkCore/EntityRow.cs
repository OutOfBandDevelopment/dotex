namespace OoBDev.EventSourcing.EntityFrameworkCore;

/// <summary>
/// The row of the <c>Entities</c> table, keyed by <see cref="Type"/> and <see cref="Key"/>.
/// </summary>
public sealed class EntityRow
{
    /// <summary>Gets or sets the entity type name.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the version, starting at 1.</summary>
    public long Version { get; set; }

    /// <summary>Gets or sets the JSON value.</summary>
    public string Value { get; set; } = "{}";
}
