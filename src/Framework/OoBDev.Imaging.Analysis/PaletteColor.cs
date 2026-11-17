namespace OoBDev.Imaging.Analysis;

/// <summary>A palette entry: a colour and the share of pixels it stands for.</summary>
/// <param name="R">Red.</param>
/// <param name="G">Green.</param>
/// <param name="B">Blue.</param>
/// <param name="Share">Fraction of the image, 0 to 1.</param>
public sealed record PaletteColor(byte R, byte G, byte B, double Share);
