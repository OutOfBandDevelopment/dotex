using System.ComponentModel.DataAnnotations;

namespace OoBDev.OpenSearch;

/// <summary>
/// Configuration options for the OpenSearch client.
/// </summary>
public class OpenSearchOptions
{
    /// <summary>
    /// Gets or sets the hostname of the OpenSearch server.
    /// </summary>
    [Required]
    public required string HostName { get; set; }
    /// <summary>
    /// Gets or sets the port number of the OpenSearch server.
    /// </summary>
    public required int Port { get; set; } = 9200;
    /// <summary>
    /// Gets or sets the index name used for OpenSearch operations.
    /// </summary>
    [Required]
    public required string IndexName { get; set; }

    /// <summary>
    /// Gets or sets the username for authentication (if required).
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// Gets or sets the password for authentication (if required).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to connect over <c>https</c> (default <c>false</c>, plain <c>http</c>).
    /// </summary>
    public bool UseTls { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether any server certificate is accepted (default <c>false</c>). Only for local test instances with self-signed certificates.
    /// </summary>
    public bool AllowInvalidCertificate { get; set; }
}
