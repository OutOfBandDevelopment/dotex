using OoBDev.System.IO.Segmenters;
using System.Buffers;

namespace OoBDev.Kuando.Busylight;

/// <summary>
/// A HID read already returns exactly one report, so every non-empty read is one segment.
/// </summary>
/// <param name="onSegmentReceived">The callback to invoke when a report is extracted.</param>
public sealed class BusylightSegmenter(OnSegmentReceived onSegmentReceived) : SegmenterBase(onSegmentReceived, SegmentionOptions.None)
{
    /// <inheritdoc/>
    protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer) =>
        buffer.IsEmpty ? (SegmentationStatus.Incomplete, null) : (SegmentationStatus.Complete, buffer);
}
