using Microsoft.Extensions.Options;
using OllamaSharp;

namespace OoBDev.Ollama;

/// <summary>
/// Factory class for creating instances of the <see cref="OllamaApiClient"/>.
/// </summary>
public class OllamaApiClientFactory : IOllamaApiClientFactory
{
    private readonly IOptions<OllamaApiClientOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaApiClientFactory"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options for configuring the Ollama API client.</param>
    public OllamaApiClientFactory(
        IOptions<OllamaApiClientOptions> options
      ) => _options = options;

    /// <summary>
    /// Builds a new instance of the <see cref="OllamaApiClient"/> with the specified host.
    /// </summary>
    /// <returns>A new instance of the <see cref="OllamaApiClient"/>.</returns>
    public IOllamaApiClient Build()
    {
        var options = _options.Value;
        var http = new global::System.Net.Http.HttpClient { BaseAddress = new global::System.Uri(options.Url), Timeout = options.Timeout };
        var client = new OllamaApiClient(http, options.DefaultModel);
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
        }

        return client;
    }
}
