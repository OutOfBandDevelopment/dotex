namespace OoBDev.Microsoft.SqlServer.Server;

/// <summary>
/// Provider keys of the SQL Server adapter.
/// </summary>
public static class SqlServerGlobals
{
    /// <summary>
    /// The kebab-case key the SQL Server services are registered under.
    /// </summary>
    public const string ProviderKey = "mssql";

    /// <summary>
    /// The earlier key, still registered so existing configuration keeps working.
    /// </summary>
    public const string LegacyKey = "MSSQL";
}
