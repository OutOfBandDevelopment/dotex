namespace BotChat.Ollama;

public record OllamaOptions
{
    public const string OptionName = "Ollama";

    public required string Model { get; init; }
    public required string Endpoint { get; init; }
    public required string ApiKey { get; init; }
}
