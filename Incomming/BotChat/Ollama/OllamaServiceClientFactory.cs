using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;
using OllamaSharp;

namespace BotChat.Ollama;

public class OllamaServiceClientFactory : IOllamaServiceClientFactory
{
    private readonly IOptions<OllamaOptions> _options;
    private readonly OllamaApiClient _client;

    public OllamaServiceClientFactory(
        IOptions<OllamaOptions> options,
        OllamaApiClient client
        )
    {
        _options = options;
        _client = client;
    }

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public ITextGenerationService GetTextGenerationService() => new OllamaTextGenerationService(_options.Value.Model, _client);
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}

