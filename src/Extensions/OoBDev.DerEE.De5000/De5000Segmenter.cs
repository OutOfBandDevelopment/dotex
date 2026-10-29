using OoBDev.System.IO.Segmenters;
using System.Buffers;

namespace OoBDev.DerEE.De5000;

/// <summary>
/// Cuts the meter's unprompted byte stream into 17-byte packets. The meter has no framing beyond its header and footer, and the
/// optical-to-BLE adapter adds none, so a packet can arrive split. When 17 bytes do not match header and footer the first byte is
/// dropped as invalid (<see cref="SegmentionOptions.SkipInvalidSegment"/>) and the search moves on one byte.
/// </summary>
/// <param name="onSegmentReceived">The callback to invoke when a packet is extracted.</param>
public sealed class De5000Segmenter(OnSegmentReceived onSegmentReceived) : SegmenterBase(onSegmentReceived, SegmentionOptions.SkipInvalidSegment)
{
    /// <inheritdoc/>
    protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer)
    {
        if (buffer.Length < De5000Frame.Length) return (SegmentationStatus.Incomplete, null);

        var packet = buffer.Slice(0, De5000Frame.Length).ToArray();
        return packet[0] == De5000Frame.Header0 && packet[1] == De5000Frame.Header1
            && packet[15] == De5000Frame.Footer0 && packet[16] == De5000Frame.Footer1
            ? (SegmentationStatus.Complete, buffer.Slice(0, De5000Frame.Length))
            : (SegmentationStatus.Invalid, buffer.Slice(0, 1));
    }
}
