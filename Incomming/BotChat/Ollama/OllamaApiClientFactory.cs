using Microsoft.Extensions.Options;
using OllamaSharp;

namespace BotChat.Ollama;

public class OllamaApiClientFactory : IOllamaApiClientFactory
{
    private readonly IOptions<OllamaOptions> _options;
    public OllamaApiClientFactory(
        IOptions<OllamaOptions> options
        )
    {
        _options = options;
    }
    public OllamaApiClient Create()
    {
        var ollamaClient = new OllamaApiClient(_options.Value.Endpoint, _options.Value.Model);
        if (!string.IsNullOrWhiteSpace(_options.Value.ApiKey))
            ollamaClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.Value.ApiKey}");
        return ollamaClient;
    }
}

