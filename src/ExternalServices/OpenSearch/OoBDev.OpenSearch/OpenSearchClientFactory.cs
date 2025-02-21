using Microsoft.Extensions.Options;
using OpenSearch.Net;
using System;

namespace OoBDev.OpenSearch;

/// <summary>
/// Implementation of the <see cref="IOpenSearchClientFactory"/> interface for creating instances of the <see cref="IOpenSearchLowLevelClient"/>.
/// </summary>
public class OpenSearchClientFactory : IOpenSearchClientFactory
{
    private readonly IOptions<OpenSearchOptions> _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenSearchClientFactory"/> class.
    /// </summary>
    /// <param name="config">The configuration options for the OpenSearch client.</param>
    public OpenSearchClientFactory(
        IOptions<OpenSearchOptions> config
        ) => _config = config;

    /// <summary>
    /// Creates a new instance of the <see cref="IOpenSearchLowLevelClient"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="IOpenSearchLowLevelClient"/>.</returns>
    public virtual IOpenSearchLowLevelClient Create()
    {
        var connection = new ConnectionConfiguration(
                new Uri($"http://{_config.Value.HostName}:{_config.Value.Port}")
            )
            .EnableHttpCompression(true)
            .ThrowExceptions(true)
            ;

        if (!string.IsNullOrWhiteSpace(_config.Value.UserName) && !string.IsNullOrWhiteSpace(_config.Value.Password))
        {
            connection.BasicAuthentication(_config.Value.UserName, _config.Value.Password);
        }

        var client = new OpenSearchLowLevelClient(connection);
        return client;
    }
}
