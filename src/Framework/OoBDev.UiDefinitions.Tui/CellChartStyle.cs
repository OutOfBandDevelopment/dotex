using System;

namespace OoBDev.UiDefinitions.Tui;

/// <summary>The two theme-dependent inputs of <see cref="CellCharts"/>. A host that themes its terminal points <see cref="Source"/> at its theme; without one the charts use a neutral grey on the light palette.</summary>
public static class CellChartStyle
{
    private static readonly (string Muted, bool Dark) _default = ("#808080", false);

    /// <summary>Reads the muted ink (<c>#RRGGBB</c>) and whether the dark palette slots apply, each time a chart renders so a live theme change shows at once.</summary>
    public static Func<(string Muted, bool Dark)>? Source { get; set; }

    /// <summary>Muted ink for axes, rings and trails.</summary>
    public static string Muted => (Source?.Invoke() ?? _default).Muted;

    /// <summary>Whether the dark palette slots apply.</summary>
    public static bool DarkPalette => (Source?.Invoke() ?? _default).Dark;
}
