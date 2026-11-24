using System.Collections.Generic;

namespace OoBDev.DataLoader;

/// <summary>What a handler decided about a row.</summary>
/// <param name="Accepted">False ends the pipeline for the row.</param>
/// <param name="Error">Why the row was rejected.</param>
public sealed record RowOutcome(bool Accepted, string? Error = null)
{
    /// <summary>The row may continue.</summary>
    public static RowOutcome Accept { get; } = new(true);

    /// <summary>Rejects the row with a reason.</summary>
    /// <param name="error">The reason.</param>
    public static RowOutcome Reject(string error) => new(false, error);
}
