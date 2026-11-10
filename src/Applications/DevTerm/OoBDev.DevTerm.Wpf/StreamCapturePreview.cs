using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OoBDev.DevTerm.Configuration;
using OoBDev.Documents.Hpgl;
using OoBDev.Extensions.Drawing;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Builds what the stream monitor shows for a capture: a picture, or the reason there is none.</summary>
public static class StreamCapturePreview
{
    /// <summary>Draws <paramref name="capture"/> where its kind can be drawn.</summary>
    /// <param name="capture">The capture.</param>
    /// <param name="message">Empty when drawn; otherwise why not.</param>
    /// <returns>The image, or null.</returns>
    public static ImageSource? Render(StreamMonitorCapture capture, out string message)
    {
        ArgumentNullException.ThrowIfNull(capture);
        var kind = capture.Capture.Kind;
        if (kind.IsSvg)
        {
            var svg = SvgPreview.TryRender(global::System.Text.Encoding.UTF8.GetString(capture.Capture.Data), out var svgError);
            message = svg is null ? $"Could not draw this SVG: {svgError}" : string.Empty;
            return svg;
        }

        if (kind.Format == StreamContentFormat.Hpgl)
        {
            var scene = HpglToSvgConverter.ParseScene(capture.Capture.Data);
            return Decode(VectorSceneRasterizer.Rasterize(scene, HpglPreviewWidthPixels), kind.DisplayName, out message);
        }

        if (!kind.IsNativeImage)
        {
            message = $"Preview not available yet for {kind.DisplayName}; the captured bytes were saved as-is.";
            return null;
        }

        return Decode(capture.Capture.Data, kind.DisplayName, out message);
    }

    /// <summary>Width of the picture an HP-GL plot is drawn at.</summary>
    public const int HpglPreviewWidthPixels = 1000;

    /// <summary>Decodes image bytes Windows understands (PNG, JPEG, BMP, ...).</summary>
    /// <param name="data">The bytes.</param>
    /// <param name="kindName">What the bytes are, for the message.</param>
    /// <param name="message">Empty when decoded; otherwise why not.</param>
    public static ImageSource? Decode(byte[] data, string kindName, out string message)
    {
        try
        {
            using var stream = new global::System.IO.MemoryStream(data, writable: false);
            var frame = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
            frame.Freeze();
            message = string.Empty;
            return frame;
        }
        catch (Exception ex) when (ex is NotSupportedException or global::System.IO.FileFormatException or ArgumentException or global::System.IO.IOException or InvalidOperationException or OverflowException)
        {
            message = $"Could not preview this {kindName}: {ex.Message}";
            return null;
        }
    }
}
