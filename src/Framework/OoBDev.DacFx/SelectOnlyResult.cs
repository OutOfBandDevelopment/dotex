namespace OoBDev.DacFx;

/// <summary>The outcome of <see cref="SelectOnlyGuard.Check"/>.</summary>
/// <param name="IsAllowed">True when the text is exactly one plain SELECT.</param>
/// <param name="Reason">Why the text was refused; null when allowed.</param>
public sealed record SelectOnlyResult(bool IsAllowed, string? Reason)
{
    /// <summary>Creates an allowed result.</summary>
    /// <returns>The result.</returns>
    public static SelectOnlyResult Allow() => new(true, null);

    /// <summary>Creates a refused result.</summary>
    /// <param name="reason">Why the text was refused.</param>
    /// <returns>The result.</returns>
    public static SelectOnlyResult Refuse(string reason) => new(false, reason);
}
