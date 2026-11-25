namespace OoBDev.Data.DesiredState;

/// <summary>
/// Where to connect. The password is passed to the tool through its environment, never on the command line.
/// <paramref name="Schema"/> (PostgreSQL, SQL Server) and <paramref name="TablePrefix"/> (SQLite) limit the plan to one store instance,
/// so another instance's tables in the same database never show up as drops.
/// </summary>
public sealed record DatabaseTarget(string Host, int Port, string Database, string? User = null, string? Password = null, string? Schema = null, string? TablePrefix = null)
{
    /// <summary>For SQLite: the database file is <see cref="Database"/>.</summary>
    public static DatabaseTarget File(string path) => new(string.Empty, 0, path);

    /// <summary>The same target limited to one instance.</summary>
    public DatabaseTarget ForInstance(string? schema = null, string? tablePrefix = null) => this with { Schema = schema, TablePrefix = tablePrefix };
}
