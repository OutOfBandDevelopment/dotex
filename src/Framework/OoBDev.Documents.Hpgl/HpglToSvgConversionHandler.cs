using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OoBDev.Documents.Conversion;

namespace OoBDev.Documents.Hpgl;

/// <summary>Converts HP-GL plotter output (<c>image/vnd.hp-hpgl</c>) to SVG (<c>image/svg+xml</c>) with <see cref="HpglToSvgConverter"/>.</summary>
public sealed class HpglToSvgConversionHandler : IDocumentConversionHandler
{
    /// <summary>The HP-GL content type.</summary>
    public const string HpglContentType = "image/vnd.hp-hpgl";

    /// <summary>The SVG content type.</summary>
    public const string SvgContentType = "image/svg+xml";

    /// <inheritdoc />
    public string[] Sources => [HpglContentType];

    /// <inheritdoc />
    public string[] Destinations => [SvgContentType];

    /// <inheritdoc />
    public bool SupportedSource(string contentType) => Sources.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public bool SupportedDestination(string contentType) => Destinations.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">The source or destination content type is not supported.</exception>
    public async Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
    {
        if (!SupportedSource(sourceContentType)) throw new NotSupportedException($"Source content type \"{sourceContentType}\" is not supported");
        if (!SupportedDestination(destinationContentType)) throw new NotSupportedException($"Destination content type \"{destinationContentType}\" is not supported");

        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer).ConfigureAwait(false);
        var svg = HpglToSvgConverter.ConvertToSvg(buffer.ToArray());
        var bytes = new UTF8Encoding(false).GetBytes(svg);
        await destination.WriteAsync(bytes).ConfigureAwait(false);
        await destination.FlushAsync().ConfigureAwait(false);
    }
}
