using OoBDev.System.IO.Segmenters;
using System.Buffers;

namespace OoBDev.Scpi;

/// <summary>
/// Cuts replies into lines ended by CR, LF or CRLF, and a definite-length block (<c>#&lt;n&gt;&lt;length&gt;</c> then that many bytes, such as a screen dump) into one segment whatever bytes it holds. Some instruments end with a bare CR (a Tektronix TDS2024 over TCP), so a line feed
/// alone is not enough. A line ending that follows another one is skipped, which makes CRLF one terminator.
/// </summary>
/// <param name="onSegmentReceived">The callback to invoke when a line is extracted.</param>
public sealed class ScpiReplySegmenter(OnSegmentReceived onSegmentReceived) : SegmenterBase(onSegmentReceived, SegmentionOptions.SkipInvalidSegment)
{
    /// <inheritdoc/>
    protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer)
    {
        if (buffer.IsEmpty) return (SegmentationStatus.Incomplete, null);

        var first = buffer.FirstSpan[0];
        if (first is (byte)'\r' or (byte)'\n') return (SegmentationStatus.Invalid, buffer.Slice(0, 1));

        switch (ScpiBlockHeader.TryRead(buffer, out var headerLength, out var dataLength))
        {
            case ScpiBlockHeader.Result.Incomplete:
                return (SegmentationStatus.Incomplete, null);
            case ScpiBlockHeader.Result.Block:
                var total = (long)headerLength + dataLength;
                return buffer.Length < total ? (SegmentationStatus.Incomplete, null) : (SegmentationStatus.Complete, buffer.Slice(0, total));
        }

        var end = buffer.PositionOf((byte)'\n');
        var carriage = buffer.PositionOf((byte)'\r');
        if (end is null && carriage is null) return (SegmentationStatus.Incomplete, null);

        var stop = (end, carriage) switch
        {
            ({ } e, { } c) => buffer.Slice(0, e).Length < buffer.Slice(0, c).Length ? e : c,
            ({ } e, null) => e,
            (null, { } c) => c,
            _ => default,
        };
        return (SegmentationStatus.Complete, buffer.Slice(0, buffer.GetPosition(1, stop)));
    }
}
