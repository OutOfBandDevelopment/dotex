namespace OoBDev.EventSourcing.EntityFrameworkCore;

/// <summary>
/// Provider keys of the Entity Framework Core stores.
/// </summary>
public static class EntityFrameworkEventSourcingGlobals
{
    /// <summary>The key to put in <c>OoBDev:EventSourcing:EventStore:Type</c> and <c>EntityStore:Type</c>.</summary>
    public const string EntityFrameworkKey = "entity-framework";
}
