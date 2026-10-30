namespace OoBDev.ImageMask.Cli;

/// <summary>A rectangle in pixels that is painted over before an image is compared or saved.</summary>
public sealed class MaskZone
{
    /// <summary>Label that says what the zone hides (clock, temp path, port).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Simple wildcard (<c>*</c>, <c>?</c>) matched against the image file name; the zone applies to every image by default.</summary>
    public string Match { get; set; } = "*";

    /// <summary>Left edge in pixels.</summary>
    public int X { get; set; }

    /// <summary>Top edge in pixels.</summary>
    public int Y { get; set; }

    /// <summary>Width in pixels.</summary>
    public int Width { get; set; }

    /// <summary>Height in pixels.</summary>
    public int Height { get; set; }

    /// <summary>Fill color as <c>#RRGGBB</c> or <c>#AARRGGBB</c>; solid red by default so a redaction is visible in the saved image.</summary>
    public string Color { get; set; } = "#FF0000";
}
