using System.Buffers;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Segmenter for packets with no start marker: every <paramref name="length"/> bytes form one segment.
/// </summary>
/// <param name="onSegmentReceived">The callback to invoke when a segment is successfully extracted.</param>
/// <param name="length">The exact length of each segment in bytes.</param>
/// <param name="options">Options controlling segmentation behavior.</param>
internal sealed class FixedLengthSegmenter(OnSegmentReceived onSegmentReceived, long length, SegmentionOptions options) : SegmenterBase(onSegmentReceived, options)
{
    protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer) =>
        buffer.Length < length
            ? (SegmentationStatus.Incomplete, buffer)
            : (SegmentationStatus.Complete, buffer.Slice(0, length));
}
