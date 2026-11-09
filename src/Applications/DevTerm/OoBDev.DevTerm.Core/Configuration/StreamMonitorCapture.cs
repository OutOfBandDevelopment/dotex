using System;
using System.Globalization;

namespace OoBDev.DevTerm.Configuration;

/// <summary>One capture the <see cref="StreamMonitor"/> took, and where (or whether) it was saved.</summary>
/// <param name="Capture">The detected content itself.</param>
/// <param name="DeviceName">The connection it came from, as used in its file name.</param>
/// <param name="LocalStartedAt">When it started, in local time — the timestamp its file name uses.</param>
/// <param name="SavedPath">The file it was auto-saved to, or <see langword="null"/> if saving failed.</param>
/// <param name="SaveError">Why saving failed, when it did.</param>
/// <param name="ConvertedFrom">For an entry that is a converter's output, the display name of what it was converted from (e.g. <c>HP-GL plot</c>); otherwise <see langword="null"/>.</param>
/// <param name="Source">The key the originating session was tracked under (see <see cref="StreamMonitor.Track"/>), so a front end can route a message to that session's tab; <see langword="null"/> if unknown.</param>
public sealed record StreamMonitorCapture(StreamCapture Capture, string DeviceName, DateTimeOffset LocalStartedAt, string? SavedPath, string? SaveError, string? ConvertedFrom = null, object? Source = null)
{
    /// <summary>The front ends' one-line status message for this capture, e.g. <c>Captured 4,213 bytes of BMP image to C:\…\hp34401a_20260923-143512.bmp.</c></summary>
    public string Describe()
    {
        var size = Capture.Data.Length.ToString("N0", CultureInfo.InvariantCulture);
        var what = $"{size} bytes of {Capture.Kind.DisplayName}{EndNote(Capture.EndReason)}";
        return SavedPath is not null
            ? $"Captured {what} to {SavedPath}."
            : $"Captured {what}, but could not save it: {SaveError}";
    }

    /// <summary>What the capture is, for a detail line: e.g. <c>BMP image, 70 bytes, complete (declared by the command).</c></summary>
    public string Summary
    {
        get
        {
            var size = Capture.Data.Length.ToString("N0", CultureInfo.InvariantCulture);
            if (ConvertedFrom is not null)
            {
                return $"{Capture.Kind.DisplayName}, {size} bytes, converted from {ConvertedFrom}.";
            }

            var declared = Capture.WasDeclared ? " (declared by the command)" : string.Empty;
            return $"{Capture.Kind.DisplayName}, {size} bytes, {EndLabel}{declared}.";
        }
    }

    /// <summary>A short, fixed-width-friendly description of how the capture ended (for a capture list).</summary>
    public string EndLabel => Capture.EndReason switch
    {
        StreamCaptureEnd.Complete => "complete",
        StreamCaptureEnd.IdleTimeout => "went quiet",
        StreamCaptureEnd.SizeLimit => "size limit",
        _ => "stopped",
    };

    private static string EndNote(StreamCaptureEnd reason) => reason switch
    {
        StreamCaptureEnd.IdleTimeout => " (ended when the device went quiet)",
        StreamCaptureEnd.SizeLimit => " (cut off at the size limit - probably incomplete)",
        StreamCaptureEnd.Flushed => " (monitoring stopped mid-capture - probably incomplete)",
        _ => string.Empty,
    };
}
