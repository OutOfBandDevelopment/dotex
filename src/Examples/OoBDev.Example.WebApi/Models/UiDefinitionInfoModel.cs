namespace OoBDev.WebApi.Models;

/// <summary>A sample UI definition the host can serve.</summary>
public sealed class UiDefinitionInfoModel
{
    /// <summary>The key used in the routes.</summary>
    public required string Key { get; init; }

    /// <summary>The definition's display name.</summary>
    public required string Name { get; init; }

    /// <summary>What the definition shows.</summary>
    public string? Description { get; init; }

    /// <summary>How the definition was produced: <c>hand-built</c> or <c>generated</c> from a model type.</summary>
    public required string Source { get; init; }
}
