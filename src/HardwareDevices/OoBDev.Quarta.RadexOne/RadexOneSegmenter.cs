using OoBDev.System.IO.Segmenters;
using System;
using System.Buffers;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// Cuts the serial stream into reply packets. Bytes before the <c>7A FF</c> prefix, and headers that fail their checksum, are dropped
/// one byte at a time so the search resynchronises on the next prefix.
/// </summary>
/// <param name="onSegmentReceived">The callback to invoke when a packet is extracted.</param>
public sealed class RadexOneSegmenter(OnSegmentReceived onSegmentReceived) : SegmenterBase(onSegmentReceived, SegmentionOptions.SkipInvalidSegment)
{
    /// <inheritdoc/>
    protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer)
    {
        if (buffer.Length < 2) return (SegmentationStatus.Incomplete, null);

        Span<byte> prefix = stackalloc byte[2];
        buffer.Slice(0, 2).CopyTo(prefix);
        if (prefix[0] != RadexOneFramer.InboundPrefix0 || prefix[1] != RadexOneFramer.InboundPrefix1)
            return (SegmentationStatus.Invalid, buffer.Slice(0, 1));

        if (buffer.Length < RadexOneFramer.HeaderLength) return (SegmentationStatus.Incomplete, null);

        Span<byte> header = stackalloc byte[RadexOneFramer.HeaderLength];
        buffer.Slice(0, RadexOneFramer.HeaderLength).CopyTo(header);
        if (!RadexOneFramer.TryValidateHeader(header, out var extensionLength))
            return (SegmentationStatus.Invalid, buffer.Slice(0, 1));

        var total = RadexOneFramer.HeaderLength + extensionLength;
        return buffer.Length < total ? (SegmentationStatus.Incomplete, null) : (SegmentationStatus.Complete, buffer.Slice(0, total));
    }
}
