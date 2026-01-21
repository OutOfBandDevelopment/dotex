using System.Buffers;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Represents the result of a segment read operation.
/// </summary>
public interface ISegmentReadResult
{
    /// <summary>
    /// Gets the status of the segmentation operation.
    /// </summary>
    SegmentationStatus Status { get; }

    /// <summary>
    /// Gets the remaining data that was not part of the extracted segment.
    /// </summary>
    ReadOnlySequence<byte> RemainingData { get; }
}