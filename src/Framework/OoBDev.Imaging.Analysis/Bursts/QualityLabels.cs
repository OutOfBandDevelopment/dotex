using System.Collections.Generic;

namespace OoBDev.Imaging.Analysis.Bursts;

/// <summary>Labels (shaky, defocused, soft, under, dark, over, noisy, flat) from the stored quality numbers.</summary>
public static class QualityLabels
{
    /// <summary>Best-tile focus below this is soft.</summary>
    public const double SoftTile = 500.0;

    /// <summary>High-frequency share below this is defocused.</summary>
    public const double DefocusHf = 0.006;

    /// <summary>Isotropy below this is shaky.</summary>
    public const double ShakeIso = 0.35;

    /// <summary>Mean luminance below this is under-exposed.</summary>
    public const double UnderMean = 45.0;

    /// <summary>Shadow clipping percent above this is under-exposed.</summary>
    public const double UnderClip = 35.0;

    /// <summary>Mean luminance above this is over-exposed.</summary>
    public const double OverMean = 205.0;

    /// <summary>Highlight clipping percent above this is over-exposed.</summary>
    public const double OverClip = 10.0;

    /// <summary>Noise above this is noisy.</summary>
    public const double Noisy = 6.0;

    /// <summary>Range below this (without a bad exposure) is flat.</summary>
    public const double LowRange = 60.0;

    /// <summary>Range above this makes an under-exposed frame a deliberate dark one.</summary>
    public const double DarkRange = 120.0;

    /// <summary>The labels of a frame, or <c>ok</c>.</summary>
    public static IReadOnlyList<string> Of(QualityMeasures r)
    {
        var labels = new List<string>();
        var under = false;
        var over = false;
        if (r.FocusTile < SoftTile)
            labels.Add(r.Isotropy < ShakeIso ? "shaky" : r.HfShare < DefocusHf ? "defocused" : "soft");
        if (r.LumaMean < UnderMean || r.ClipShadowPct > UnderClip)
        {
            under = r.DynRange <= DarkRange;
            labels.Add(under ? "under" : "dark");
        }

        if (r.LumaMean > OverMean || r.ClipHighlightPct > OverClip)
        {
            over = true;
            labels.Add("over");
        }

        if (r.Noise > Noisy)
            labels.Add("noisy");
        if (r.DynRange < LowRange && !under && !over)
            labels.Add("flat");
        return labels.Count == 0 ? ["ok"] : labels;
    }
}
