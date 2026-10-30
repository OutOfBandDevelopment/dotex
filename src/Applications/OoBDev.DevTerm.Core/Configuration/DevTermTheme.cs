using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// One complete, immutable theme: a name plus a color for every <see cref="ThemeRole"/> and the chart
/// palette variant. Built-ins come from <see cref="BuiltInThemes"/>; user themes are loaded from JSON
/// by <see cref="ThemeFile"/>, starting from a built-in and overriding some roles. See
/// docs/design/theming.md.
/// </summary>
public sealed class DevTermTheme
{
    private readonly IReadOnlyDictionary<ThemeRole, ThemeColor> _colors;

    public DevTermTheme(string name, IReadOnlyDictionary<ThemeRole, ThemeColor> colors, ChartPaletteVariant chartPalette)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(colors);
        var missing = Enum.GetValues<ThemeRole>().Where(role => !colors.ContainsKey(role)).ToList();
        if (missing.Count > 0)
        {
            throw new ArgumentException($"Theme '{name}' is missing colors for: {string.Join(", ", missing)}.", nameof(colors));
        }

        Name = name;
        _colors = new Dictionary<ThemeRole, ThemeColor>(colors);
        ChartPalette = chartPalette;
    }

    /// <summary>The name shown in View &gt; Theme and accepted by <c>--theme</c>.</summary>
    public string Name { get; }

    public ChartPaletteVariant ChartPalette { get; }

    public ThemeColor this[ThemeRole role] => _colors[role];

    /// <summary>
    /// Whether this is a dark theme (its <see cref="ThemeRole.Background"/> is closer to black than
    /// white). WPF uses it to decide whether to swap in dev-term's own dark-capable control templates
    /// instead of the stock ones, whose chrome is hard-coded light (see docs/design/theming.md).
    /// </summary>
    public bool IsDark => this[ThemeRole.Background].RelativeLuminance() < 0.25;

    /// <summary>A copy with <paramref name="overrides"/> applied on top and a new name.</summary>
    /// <summary>
    /// A variant for consoles limited to the 16 ANSI colors (Terminal.Gui's legacy-conhost downgrade snaps every color to
    /// the nearest of 16): a light theme's field, hover and selection backgrounds are all near white and would collapse
    /// onto "White", so this spreads them over Gray, DarkGray and BrightCyan instead (dark text stays readable on all
    /// three). A dark theme is returned unchanged (its roles were separated in the theme itself). TUI-only; the WPF
    /// palette is untouched. See docs/design/theming.md.
    /// </summary>
    public DevTermTheme ForSixteenColors() => IsDark
        ? this
        : With(Name, new Dictionary<ThemeRole, ThemeColor>
        {
            [ThemeRole.FieldBackground] = ThemeColor.Parse("#C0C0C0"),
            [ThemeRole.ControlHoverBackground] = ThemeColor.Parse("#808080"),
            [ThemeRole.SelectionBackground] = ThemeColor.Parse("#00FFFF"),
        });

    public DevTermTheme With(string name, IReadOnlyDictionary<ThemeRole, ThemeColor> overrides, ChartPaletteVariant? chartPalette = null)
    {
        ArgumentNullException.ThrowIfNull(overrides);
        var colors = new Dictionary<ThemeRole, ThemeColor>(_colors);
        foreach (var (role, color) in overrides)
        {
            colors[role] = color;
        }

        return new DevTermTheme(name, colors, chartPalette ?? ChartPalette);
    }

    /// <summary>
    /// The text/background pairs a reader has to be able to read, with the WCAG contrast each needs:
    /// 4.5:1 for ordinary text, 3:1 for secondary/large text and indicator fills. Checked for every
    /// built-in by the tests, and reported (not rejected) for a user theme - see <see cref="ContrastWarnings"/>.
    /// </summary>
    public static IReadOnlyList<(ThemeRole Text, ThemeRole Background, double Minimum)> ReadablePairs { get; } =
    [
        (ThemeRole.Foreground, ThemeRole.Background, 4.5),
        (ThemeRole.ControlForeground, ThemeRole.ControlBackground, 4.5),
        (ThemeRole.ControlForeground, ThemeRole.ControlHoverBackground, 4.5),
        (ThemeRole.ControlForeground, ThemeRole.FieldBackground, 4.5),
        (ThemeRole.SelectionForeground, ThemeRole.SelectionBackground, 4.5),
        (ThemeRole.MenuForeground, ThemeRole.MenuBackground, 4.5),
        (ThemeRole.MutedForeground, ThemeRole.Background, 3.0),
        (ThemeRole.Error, ThemeRole.Background, 4.5),
        // Text, not just an indicator fill: Playback's [note] lines render Warning as full-weight text.
        (ThemeRole.Warning, ThemeRole.Background, 4.5),
        (ThemeRole.OutputStatus, ThemeRole.ControlBackground, 3.0),
        (ThemeRole.OutputError, ThemeRole.ControlBackground, 4.5),
        (ThemeRole.StatusConnectedText, ThemeRole.StatusConnected, 4.5),
        (ThemeRole.StatusConnectingText, ThemeRole.StatusConnecting, 4.5),
        (ThemeRole.StatusDisconnectedText, ThemeRole.StatusDisconnected, 4.5),
        (ThemeRole.Recording, ThemeRole.MenuBackground, 3.0),
        // Text, not just an indicator fill: Playback's [tx] lines render Accent as full-weight text.
        (ThemeRole.Accent, ThemeRole.Background, 4.5),
        (ThemeRole.ChartText, ThemeRole.ChartSurface, 4.5),
        (ThemeRole.ChartMuted, ThemeRole.ChartSurface, 3.0),
    ];

    /// <summary>One message per <see cref="ReadablePairs"/> entry this theme falls short on - empty for a readable theme.</summary>
    public IReadOnlyList<string> ContrastWarnings() =>
    [
        .. ReadablePairs
            .Select(pair => (pair.Text, pair.Background, pair.Minimum, Ratio: ThemeColor.ContrastRatio(this[pair.Text], this[pair.Background])))
            .Where(pair => pair.Ratio < pair.Minimum)
            .Select(pair => FormattableString.Invariant(
                $"Theme '{Name}': {ThemeFile.RoleName(pair.Text)} on {ThemeFile.RoleName(pair.Background)} has contrast {pair.Ratio:0.0}:1 (needs {pair.Minimum:0.0}:1) and may be hard to read.")),
    ];
}
