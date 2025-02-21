namespace OoBDev.OpenSearch;

/// <summary>
/// Configuration options for the OpenSearch client.
/// </summary>
public class OpenSearchOptions
{
    /// <summary>
    /// Gets or sets the hostname of the OpenSearch server.
    /// </summary>
    public required string HostName { get; set; }
    /// <summary>
    /// Gets or sets the port number of the OpenSearch server.
    /// </summary>
    public required int Port { get; set; } = 9200;
    /// <summary>
    /// Gets or sets the index name used for OpenSearch operations.
    /// </summary>
    public required string IndexName { get; set; }

    /// <summary>
    /// Gets or sets the username for authentication (if required).
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// Gets or sets the password for authentication (if required).
    /// </summary>
    public string? Password { get; set; }
}
