Create a readme file describing the general functionality over the follow files.

Generate a summary that includes a description of the related functionality.
In a technical summary define any design patterns or achitectural patterns described by the files.
Generate a component diagrams using plantuml.
PlantUML blocks must all be identified with "```plantuml" and closed with "```"

## Source Files

```ResultMessage.cs
namespace OoBDev.System.ResponseModel;

/// <summary>
/// additional details about response
/// </summary>
public record ResultMessage
{
    /// <summary>
    /// importance level related to response
    /// </summary>
    public MessageLevels Level { get; init; } = MessageLevels.Information;

    /// <summary>
    /// Simple English message about issue. 
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// unique code that may be used to assist in translating issue
    /// </summary>
    public string? MessageCode { get; init; }

    /// <summary>
    /// Property or path related to this message 
    /// </summary>
    public string? Context { get; init; }

    /// <summary>
    /// additional properties related to response
    /// </summary>
    public object? MetaData { get; init; }
}

```

