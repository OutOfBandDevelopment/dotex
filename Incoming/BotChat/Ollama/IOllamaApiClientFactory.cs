using OllamaSharp;

namespace BotChat.Ollama;

public interface IOllamaApiClientFactory
{
    OllamaApiClient Create();
}

