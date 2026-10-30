using System;

namespace OoBDev.DevTerm;

/// <summary>Why a <see cref="StreamCapture"/> ended.</summary>
public enum StreamCaptureEnd
{
    /// <summary>The content's own structure (or a block's declared length) said it was complete.</summary>
    Complete,

    /// <summary>No more bytes arrived for <see cref="StreamContentWatcherOptions.IdleTimeout"/> — the usual end for formats with no in-band end marker (HP-GL, TIFF).</summary>
    IdleTimeout,

    /// <summary>The capture reached <see cref="StreamContentWatcherOptions.MaxCaptureBytes"/> and was cut off there.</summary>
    SizeLimit,

    /// <summary>The watcher was flushed (detached or stopped) while the capture was still in progress.</summary>
    Flushed,
}
