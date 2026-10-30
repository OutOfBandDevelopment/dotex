using System;

namespace OoBDev.DevTerm;

/// <summary>Tuning for <see cref="StreamContentWatcher"/>.</summary>
public sealed class StreamContentWatcherOptions
{
    /// <summary>
    /// How long a capture with no in-band end (HP-GL, TIFF, unrecognized declared data) waits for
    /// more bytes before it's considered finished. Also how long a quiet link has to be before the
    /// next byte counts as the start of a new reply (where HP-GL is allowed to be recognized).
    /// </summary>
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The idle wait for an HP-GL capture (when it hasn't ended with its own <c>SP0;</c>): never shorter than
    /// <see cref="IdleTimeout"/>. A plotter on a slow link can pause mid-plot for several seconds, and a cut there
    /// splits one plot into several files.
    /// </summary>
    public TimeSpan HpglIdleTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>A capture is cut off (and reported as <see cref="StreamCaptureEnd.SizeLimit"/>) at this many bytes, so a runaway stream can't grow without bound.</summary>
    public int MaxCaptureBytes { get; set; } = 64 * 1024 * 1024;
}
