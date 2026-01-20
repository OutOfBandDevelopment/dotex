namespace OoBDev.WebApi.Models;

/// <summary>
/// Represents the model used for generating AI responses.
/// </summary>
public class GenerativeAiRequestModel
{
    /// <summary>
    /// Gets or sets the prompt details.
    /// </summary>
    public required string PromptDetails { get; set; }

    /// <summary>
    /// Gets or sets the prompt details.
    /// </summary>
    public required string Data { get; set; }

    /// <summary>
    /// Gets or sets the user input.
    /// </summary>
    public required string UserInput { get; set; }
}
