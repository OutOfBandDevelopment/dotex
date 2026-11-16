using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using OoBDev.Media.Metadata.Raw;

namespace OoBDev.ImageMagick;

/// <summary>Develops camera RAW files with ImageMagick (Magick.NET): white balance, 8 or 16 bit, JPEG or PNG.</summary>
public sealed class MagickRawDeveloper : IRawDeveloper
{
    private readonly EmbeddedPreviewDeveloper detector = new();

    /// <inheritdoc />
    public bool IsRaw(Stream input) => detector.IsRaw(input);

    /// <inheritdoc />
    public async Task DevelopAsync(Stream input, Stream output, RawDevelopOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(options);
        if (options.BitDepth is not (8 or 16))
        {
            throw new NotSupportedException("Bit depth must be 8 or 16.");
        }

        using var copy = new MemoryStream();
        await input.CopyToAsync(copy, cancellationToken).ConfigureAwait(false);
        copy.Position = 0;
        if (!detector.IsRaw(copy))
        {
            throw new InvalidDataException("The input is not a camera RAW file.");
        }

        using var image = new MagickImage(copy.ToArray());
        Render(image, options);
        var bytes = image.ToByteArray();
        await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
    }

    internal static void Render(MagickImage image, RawDevelopOptions options)
    {
        if (options.RedScale != 1)
        {
            image.Evaluate(Channels.Red, EvaluateOperator.Multiply, options.RedScale);
        }

        if (options.GreenScale != 1)
        {
            image.Evaluate(Channels.Green, EvaluateOperator.Multiply, options.GreenScale);
        }

        if (options.BlueScale != 1)
        {
            image.Evaluate(Channels.Blue, EvaluateOperator.Multiply, options.BlueScale);
        }

        image.Depth = (uint)options.BitDepth;
        if (options.Format == RawOutputFormat.Png)
        {
            image.Format = MagickFormat.Png;
        }
        else
        {
            image.Format = MagickFormat.Jpeg;
            image.Quality = (uint)Math.Clamp(options.JpegQuality, 1, 100);
        }
    }
}
