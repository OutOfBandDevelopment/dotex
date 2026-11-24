namespace OoBDev.Data.DesiredState;

/// <summary>Where to connect. The password is passed to the tool through its environment, never on the command line.</summary>
public sealed record DatabaseTarget(string Host, int Port, string Database, string? User = null, string? Password = null)
{
    /// <summary>For SQLite: the database file is <see cref="Database"/>.</summary>
    public static DatabaseTarget File(string path) => new(string.Empty, 0, path);
}
