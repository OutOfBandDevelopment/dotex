namespace OoBDev.EventSourcing.SqlServer;

/// <summary>
/// Provider key of the SQL Server stores.
/// </summary>
public static class SqlServerGlobals
{
    /// <summary>The key to put in <c>OoBDev:EventSourcing:EventStore:Type</c> and <c>EntityStore:Type</c>.</summary>
    public const string ProviderKey = "sql-server";
}
