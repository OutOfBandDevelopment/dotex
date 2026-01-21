using System;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Defines options for controlling data segmentation behavior.
/// </summary>
[Flags]
public enum SegmentionOptions
{
    /// <summary>
    /// No special options are applied.
    /// </summary>
    None = 0,

    /// <summary>
    /// When a segment is invalid, skip it and continue processing the remaining data.
    /// </summary>
    SkipInvalidSegment = 1,

    /// <summary>
    /// Treat a second start marker as an indication of an invalid segment.
    /// </summary>
    SecondStartInvalid = 2,
}