using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.Tui;

namespace OoBDev.DevTerm.Tui;

/// <summary>Points the shared Tui chart renderers at DevTerm's active theme.</summary>
internal static class DevTermChartStyle
{
    internal static void Install() =>
        CellChartStyle.Source = static () => (ActiveTheme.Current[ThemeRole.ChartMuted].ToHex(), ActiveTheme.Current.ChartPalette == ChartPaletteVariant.Dark);
}
