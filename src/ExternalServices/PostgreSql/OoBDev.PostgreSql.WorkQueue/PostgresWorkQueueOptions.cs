using System.ComponentModel.DataAnnotations;

namespace OoBDev.PostgreSql.WorkQueue;

/// <summary>
/// Where the PostgreSQL work queue keeps its tables. Identifiers are lower-case letters, digits and underscores
/// so they can be quoted into the generated SQL safely.
/// </summary>
public sealed class PostgresWorkQueueOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "WorkQueue:Postgres";

    /// <summary>The identifier pattern accepted for schema and table names.</summary>
    public const string IdentifierPattern = "^[a-z_][a-z0-9_]{0,62}$";

    /// <summary>The connection string of the database holding the queue.</summary>
    [Required]
    public string ConnectionString { get; set; } = "";

    /// <summary>The schema holding the tables.</summary>
    [Required]
    [RegularExpression(IdentifierPattern)]
    public string Schema { get; set; } = "work_queue";

    /// <summary>The table holding one row per item.</summary>
    [Required]
    [RegularExpression(IdentifierPattern)]
    public string TasksTable { get; set; } = "tasks";

    /// <summary>The table holding one row per death.</summary>
    [Required]
    [RegularExpression(IdentifierPattern)]
    public string DeadLettersTable { get; set; } = "dead_letters";
}
