namespace OoBDev.EventSourcing;

/// <summary>
/// Configuration paths and provider keys of the event sourcing library.
/// </summary>
public static class EventSourcingGlobals
{
    /// <summary>Configuration path that selects the event store.</summary>
    public const string EventStorePath = "OoBDev:EventSourcing:EventStore:Type";

    /// <summary>Configuration path that selects the entity store.</summary>
    public const string EntityStorePath = "OoBDev:EventSourcing:EntityStore:Type";

    /// <summary>Configuration section of the relay options.</summary>
    public const string RelaySection = "OoBDev:EventSourcing:Relay";

    /// <summary>The configuration section of the snapshot options.</summary>
    public const string SnapshotSection = "OoBDev:EventSourcing:Snapshots";

    /// <summary>Key under which the configured event store is resolved.</summary>
    public const string EventStoreSelectedKey = "event-store-selected";

    /// <summary>Key under which the configured entity store is resolved.</summary>
    public const string EntityStoreSelectedKey = "entity-store-selected";

    /// <summary>Provider key of the in-memory stores.</summary>
    public const string InMemoryKey = "in-memory";
}
