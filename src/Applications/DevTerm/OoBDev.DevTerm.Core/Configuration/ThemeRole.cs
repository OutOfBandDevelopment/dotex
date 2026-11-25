namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// A semantic color role - what a color is <em>for</em>, not where it's drawn. Both front ends look
/// colors up by role (WPF as <c>DynamicResource</c> brushes, the TUI as Terminal.Gui schemes and
/// attributes), so a theme is one table of these. A theme file names them in camelCase
/// (<c>"outputError": "#FF6B6B"</c>). See docs/design/Theming/theming.md.
/// </summary>
public enum ThemeRole
{
    /// <summary>Window/surface background.</summary>
    Background,

    /// <summary>Ordinary text on <see cref="Background"/>.</summary>
    Foreground,

    /// <summary>Secondary text (notes, subtitles, hints) on <see cref="Background"/>.</summary>
    MutedForeground,

    /// <summary>Background of input controls: text boxes, lists, combo boxes, buttons.</summary>
    ControlBackground,

    /// <summary>Text inside input controls.</summary>
    ControlForeground,

    /// <summary>Border of input controls.</summary>
    ControlBorder,

    /// <summary>
    /// Background of a text-entry field in the TUI, which has no border to mark where a field is - so
    /// it has to differ from <see cref="Background"/> (WPF's fields have borders and use <see cref="ControlBackground"/>).
    /// </summary>
    FieldBackground,

    /// <summary>Background of a control under the mouse.</summary>
    ControlHoverBackground,

    /// <summary>Background of the selected item (list, menu, combo box).</summary>
    SelectionBackground,

    /// <summary>Text of the selected item.</summary>
    SelectionForeground,

    /// <summary>Menu bar, drop-down menus and the status bar.</summary>
    MenuBackground,

    /// <summary>Text in menus and the status bar.</summary>
    MenuForeground,

    /// <summary>Focus and informational accents (the control panel's "ⓘ" icons).</summary>
    Accent,

    /// <summary>Error text outside the output pane: form validation, not-found hints, a panel's failed-send status.</summary>
    Error,

    /// <summary>Warning text (playback's warning lines).</summary>
    Warning,

    /// <summary>dev-term's own status lines in the output pane (<c>[dev-term] …</c>).</summary>
    OutputStatus,

    /// <summary>Error lines in the output pane (<c>[error] …</c>).</summary>
    OutputError,

    /// <summary>Connected: the WPF status dot, the TUI status line's background.</summary>
    StatusConnected,

    /// <summary>Text drawn on <see cref="StatusConnected"/> (the TUI status line).</summary>
    StatusConnectedText,

    /// <summary>Connecting: the WPF status dot, the TUI status line's background.</summary>
    StatusConnecting,

    /// <summary>Text drawn on <see cref="StatusConnecting"/>.</summary>
    StatusConnectingText,

    /// <summary>Disconnected: the WPF status dot, the TUI status line's background.</summary>
    StatusDisconnected,

    /// <summary>Text drawn on <see cref="StatusDisconnected"/>.</summary>
    StatusDisconnectedText,

    /// <summary>The logger's "● REC" indicator.</summary>
    Recording,

    /// <summary>The border around a color swatch (picked-color previews), so a swatch close to the background stays visible.</summary>
    SwatchBorder,

    /// <summary>The plot area of a chart (bar graph, strip chart, vector plot).</summary>
    ChartSurface,

    /// <summary>Chart gridlines, tracks and the plot border.</summary>
    ChartGrid,

    /// <summary>Chart labels, legends and readouts.</summary>
    ChartText,

    /// <summary>Chart axes, rings, trails and scale labels.</summary>
    ChartMuted,
}
