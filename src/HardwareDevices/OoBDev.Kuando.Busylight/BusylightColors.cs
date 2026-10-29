using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace OoBDev.Kuando.Busylight;

/// <summary>
/// The colour presets and the text form a front end passes in.
/// </summary>
public static class BusylightColors
{
    /// <summary>All channels off.</summary>
    public static BusylightColor Off { get; } = new(0x00, 0x00, 0x00);

    /// <summary>Red.</summary>
    public static BusylightColor Red { get; } = new(0xFF, 0x00, 0x00);

    /// <summary>Green.</summary>
    public static BusylightColor Green { get; } = new(0x00, 0xFF, 0x00);

    /// <summary>Blue.</summary>
    public static BusylightColor Blue { get; } = new(0x00, 0x00, 0xFF);

    /// <summary>Yellow.</summary>
    public static BusylightColor Yellow { get; } = new(0xFF, 0xFF, 0x00);

    /// <summary>
    /// Reads a preset name (Red, Green, Blue, Yellow, Off; any case) or an <c>r,g,b</c> triple of 0 to 255.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="color">The colour.</param>
    /// <returns><see langword="false"/> when the text is neither.</returns>
    public static bool TryParse(string? text, [NotNullWhen(true)] out BusylightColor? color)
    {
        color = null;
        if (text is null) return false;

        switch (text.ToUpperInvariant())
        {
            case "RED": color = Red; return true;
            case "GREEN": color = Green; return true;
            case "BLUE": color = Blue; return true;
            case "YELLOW": color = Yellow; return true;
            case "OFF": color = Off; return true;
        }

        var parts = text.Split(',');
        if (parts.Length == 3
            && byte.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var r)
            && byte.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var g)
            && byte.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var b))
        {
            color = new BusylightColor(r, g, b);
            return true;
        }

        return false;
    }
}
