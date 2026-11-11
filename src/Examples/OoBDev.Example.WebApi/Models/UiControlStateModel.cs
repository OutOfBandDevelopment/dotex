namespace OoBDev.WebApi.Models;

/// <summary>The evaluated state of one control for a set of values.</summary>
public sealed class UiControlStateModel
{
    /// <summary>The control id.</summary>
    public required string Id { get; init; }

    /// <summary>False when the control or its section is hidden by a condition.</summary>
    public bool Visible { get; init; }

    /// <summary>False when an enabled-when condition is not met.</summary>
    public bool Enabled { get; init; }

    /// <summary>Why the control is disabled, when it is.</summary>
    public string? DisabledReason { get; init; }

    /// <summary>False when the value fails the control's constraint.</summary>
    public bool Valid { get; init; }

    /// <summary>The value after normalization, or the input when it is invalid.</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>The validation message when the value is invalid.</summary>
    public string? Error { get; init; }
}
