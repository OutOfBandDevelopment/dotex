namespace BotChat;

public record BotChatOptions
{
    public const string OptionName = "BotChat";

    public required string BaseUrl { get; init; }
}

