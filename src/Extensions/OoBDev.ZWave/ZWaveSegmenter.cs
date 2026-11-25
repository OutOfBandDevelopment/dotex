using OoBDev.System.IO.Segmenters;
using System.Buffers;

namespace OoBDev.ZWave;

/// <summary>
/// Cuts the Serial API byte stream into units: a lone ACK, NAK or CAN byte, a whole <c>01 len ...</c> frame, or one stray byte.
/// The segmenter never judges a frame; the decoder checks the checksum, so a damaged frame still comes out as one unit.
/// </summary>
/// <param name="onSegmentReceived">The callback to invoke when a unit is extracted.</param>
public sealed class ZWaveSegmenter(OnSegmentReceived onSegmentReceived) : SegmenterBase(onSegmentReceived, SegmentionOptions.None)
{
    // type, function and checksum are the least a data frame can hold after its length byte.
    private const int MinimumLength = 3;

    /// <inheritdoc/>
    protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer)
    {
        if (buffer.IsEmpty) return (SegmentationStatus.Incomplete, null);

        if (buffer.FirstSpan[0] != ZWaveFrame.StartOfFrame) return (SegmentationStatus.Complete, buffer.Slice(0, 1));
        if (buffer.Length < 2) return (SegmentationStatus.Incomplete, null);

        var length = buffer.Slice(1, 1).FirstSpan[0];
        if (length < MinimumLength) return (SegmentationStatus.Complete, buffer.Slice(0, 1));

        var total = length + 2;
        return buffer.Length < total
            ? (SegmentationStatus.Incomplete, null)
            : (SegmentationStatus.Complete, buffer.Slice(0, total));
    }
}
