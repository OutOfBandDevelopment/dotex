namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// Which step of <c>ChartPalette</c>'s fixed, colorblind-safe categorical order a theme uses. A theme
/// never reorders or replaces the eight hues - it only picks the variant validated for its surface.
/// </summary>
public enum ChartPaletteVariant
{
    Light,
    Dark,
}
