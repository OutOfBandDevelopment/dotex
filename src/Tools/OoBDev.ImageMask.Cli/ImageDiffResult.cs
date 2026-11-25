namespace OoBDev.ImageMask.Cli;

/// <summary>Outcome of comparing two images after masking.</summary>
/// <param name="SizeMatches">False when the images have different dimensions; no pixel comparison is made then.</param>
/// <param name="DifferentPixels">Pixels whose channels differ by more than the tolerance.</param>
/// <param name="TotalPixels">Pixels compared.</param>
public sealed record ImageDiffResult(bool SizeMatches, long DifferentPixels, long TotalPixels)
{
    /// <summary>True when the sizes match and no more than <paramref name="maxDifferentPixels"/> pixels differ.</summary>
    public bool IsMatch(long maxDifferentPixels = 0) => SizeMatches && DifferentPixels <= maxDifferentPixels;
}
