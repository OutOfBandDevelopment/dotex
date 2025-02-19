namespace OoBDev.AI.Models;

/// <summary>
/// Represents a completion request.
/// </summary>
public record CompletionRequest
{
    /// <summary>
    /// Gets or initializes the model for the completion request.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Gets or initializes the context for the completion request.
    /// </summary>
    public long[]? Context { get; init; }

    /// <summary>
    /// Gets or initializes the images for the completion request.
    /// </summary>
    public string[]? Images { get; init; }

    /// <summary>
    /// Gets or initializes the prompt for the completion request.
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// Gets or initializes the system for the completion request.
    /// </summary>
    public string? System { get; init; }

    /// <summary>
    /// Gets or initializes the template for the completion request.
    /// </summary>
    public string? Template { get; init; }
}
