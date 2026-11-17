using System.ComponentModel.DataAnnotations;

namespace OoBDev.PostgreSql.Query;

/// <summary>
/// Where the metadata query executor connects.
/// </summary>
public sealed class PostgresQueryOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "Query:Postgres";

    /// <summary>The connection string of the database holding the kinds' tables.</summary>
    [Required]
    public string ConnectionString { get; set; } = "";

    /// <summary>The command timeout in seconds.</summary>
    [Range(1, 600)]
    public int CommandTimeoutSeconds { get; set; } = 30;
}
