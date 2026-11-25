namespace OoBDev.EventSourcing.Postgres;

/// <summary>
/// Provider key of the PostgreSQL stores.
/// </summary>
public static class PostgresGlobals
{
    /// <summary>The key to put in <c>OoBDev:EventSourcing:EventStore:Type</c> and <c>EntityStore:Type</c>.</summary>
    public const string ProviderKey = "postgres";
}
