namespace OoBDev.EventSourcing;

/// <summary>
/// How a payload property differs between two versions of a schema.
/// </summary>
public enum ChangeKind
{
    /// <summary>A new optional property; readers of the old shape keep working.</summary>
    AddedOptional,

    /// <summary>A new required property; old payloads no longer fit.</summary>
    AddedRequired,

    /// <summary>A property was removed.</summary>
    Removed,

    /// <summary>The property type changed.</summary>
    TypeChanged,

    /// <summary>An optional property became required.</summary>
    BecameRequired,
}
