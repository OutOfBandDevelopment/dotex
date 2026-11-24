namespace OoBDev.EventSourcing;

/// <summary>
/// One property-level difference between two schemas.
/// </summary>
/// <param name="Property">The property name.</param>
/// <param name="Kind">The kind of change.</param>
public sealed record EventSchemaChange(string Property, ChangeKind Kind)
{
    /// <summary>
    /// Gets a value indicating whether payloads written to the old schema may fail to read.
    /// </summary>
    public bool IsBreaking => Kind != ChangeKind.AddedOptional;
}
