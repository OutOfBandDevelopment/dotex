namespace OoBDev.Imaging.Analysis.Bursts;

/// <summary>The eight stored quality numbers of one frame (computed by the caller).</summary>
/// <param name="FocusTile">Sharpness of the best tile; higher is sharper.</param>
/// <param name="Isotropy">Share of gradient energy that is not along one direction; low means motion blur.</param>
/// <param name="HfShare">Share of high-frequency energy.</param>
/// <param name="LumaMean">Mean luminance, 0 to 255.</param>
/// <param name="ClipShadowPct">Percent of pixels clipped to black.</param>
/// <param name="ClipHighlightPct">Percent of pixels clipped to white.</param>
/// <param name="DynRange">Luminance range, 0 to 255.</param>
/// <param name="Noise">Noise estimate.</param>
public sealed record QualityMeasures(
    double FocusTile,
    double Isotropy,
    double HfShare,
    double LumaMean,
    double ClipShadowPct,
    double ClipHighlightPct,
    double DynRange,
    double Noise);
