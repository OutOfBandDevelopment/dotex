using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>
/// Represents the model used for generating AI responses.
/// </summary>
public class GenAiContextRequestModel
{
    /// <summary>
    /// Gets or sets the prompt details.
    /// </summary>
    public required string AssistantConfinment { get; set; }//TODO: is this spelled right?

    /// <summary>
    /// Gets or sets the prompt details.
    /// </summary>
    public required IList<string> PromptDetails { get; set; }

    /// <summary>
    /// Gets or sets the user input.
    /// </summary>
    public required IList<string> UserInput { get; set; }
}
