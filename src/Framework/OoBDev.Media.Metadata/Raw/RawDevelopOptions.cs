namespace OoBDev.Media.Metadata.Raw;

/// <summary>How a RAW file is developed.</summary>
/// <param name="Format">The output format.</param>
/// <param name="JpegQuality">JPEG quality, 1 to 100.</param>
/// <param name="BitDepth">8 or 16 bits per channel.</param>
/// <param name="RedScale">White-balance multiplier of the red channel; 1 leaves it unchanged.</param>
/// <param name="GreenScale">White-balance multiplier of the green channel.</param>
/// <param name="BlueScale">White-balance multiplier of the blue channel.</param>
public sealed record RawDevelopOptions(
    RawOutputFormat Format = RawOutputFormat.Jpeg,
    int JpegQuality = 92,
    int BitDepth = 8,
    double RedScale = 1,
    double GreenScale = 1,
    double BlueScale = 1)
{
    /// <summary>True when the options ask for nothing beyond the camera's own JPEG preview.</summary>
    public bool IsPreviewCompatible =>
        Format == RawOutputFormat.Jpeg && BitDepth == 8 && RedScale == 1 && GreenScale == 1 && BlueScale == 1;
}
