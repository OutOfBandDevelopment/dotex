namespace OoBDev.WebApi.Models;

/// <summary>The read-only query runner settings, bound from <c>OoBDev:DataAccess</c>.</summary>
public class DataAccessOptions
{
    /// <summary>Gets or sets the connection string of a test or reporting instance; it should name a login that can only read. Empty turns the runner off.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Gets or sets the most rows one query returns.</summary>
    public int MaxRows { get; set; } = 200;

    /// <summary>Gets or sets the command timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}
