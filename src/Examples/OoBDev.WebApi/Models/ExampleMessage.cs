using System.Text.Json.Nodes;

namespace OoBDev.WebApi.Models;

/// <summary>
/// Represents an example message model.
/// </summary>
public class ExampleMessageModel
{
    /// <summary>
    /// Gets or sets the input string. Default value is "Default Value".
    /// </summary>
    public string Input { get; set; } = "Default Value";

    /// <summary>
    /// Gets or sets the JSON data associated with the message.
    /// </summary>
    public JsonNode? Data { get; set; }
}
