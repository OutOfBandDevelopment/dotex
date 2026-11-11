using System.ComponentModel.DataAnnotations;

namespace OoBDev.Neo4j;

/// <summary>
/// Settings of the Neo4j graph store.
/// </summary>
public class Neo4jOptions
{
    /// <summary>
    /// Gets or sets the Bolt endpoint, for example <c>bolt://localhost:7687</c>.
    /// </summary>
    [Required]
    public required string Uri { get; set; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    [Required]
    public required string User { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required]
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the database name; the server default when empty.
    /// </summary>
    public string? Database { get; set; }

    /// <summary>
    /// Gets or sets the longest path a shortest-path search explores.
    /// </summary>
    [Range(1, 100)]
    public int MaxPathLength { get; set; } = 15;
}
