using Microsoft.SemanticKernel.TextGeneration;

namespace BotChat.Ollama;

public interface IOllamaServiceClientFactory
{
    ITextGenerationService GetTextGenerationService();
}

