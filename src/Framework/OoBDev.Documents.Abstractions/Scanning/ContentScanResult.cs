namespace OoBDev.Documents.Scanning;

/// <summary>The outcome of a content scan.</summary>
/// <param name="IsClean">True when nothing was found.</param>
/// <param name="Detail">The signature or reason when something was found; null when clean.</param>
public record ContentScanResult(bool IsClean, string? Detail = null)
{
    /// <summary>A clean result.</summary>
    public static ContentScanResult Clean { get; } = new(true);

    /// <summary>An infected result.</summary>
    /// <param name="detail">The signature or reason.</param>
    /// <returns>The result.</returns>
    public static ContentScanResult Infected(string detail) => new(false, detail);
}
