using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OoBDev.DevTerm.Configuration;

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

        if (!kind.IsNativeImage)
        {
            message = $"Preview not available yet for {kind.DisplayName}; the captured bytes were saved as-is.";
            return null;
        }

        try
        {
            using var stream = new global::System.IO.MemoryStream(capture.Capture.Data, writable: false);
            var frame = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
            frame.Freeze();
            message = string.Empty;
            return frame;
        }
        catch (Exception ex) when (ex is NotSupportedException or global::System.IO.FileFormatException or ArgumentException or global::System.IO.IOException or InvalidOperationException or OverflowException)
        {
            message = $"Could not preview this {kind.DisplayName}: {ex.Message}";
            return null;
        }
    }
}
