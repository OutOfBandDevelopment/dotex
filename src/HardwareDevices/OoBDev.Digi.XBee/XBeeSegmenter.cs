using System;
using System.Buffers;
using OoBDev.System.IO.Segmenters;

namespace OoBDev.Digi.XBee;

/// <summary>
/// Cuts a serial byte stream into whole API frames. Noise before a start delimiter is skipped; a frame with a bad length or checksum is dropped
/// by one byte so the search resumes at the next start delimiter; a frame split across reads waits for the rest.
/// </summary>
/// <param name="onSegmentReceived">Called with the raw bytes of each good frame (still escaped in API mode 2; decode them with <see cref="XBeeFrameDecoder"/>).</param>
/// <param name="escaped">True for API mode 2.</param>
public sealed class XBeeSegmenter(OnSegmentReceived onSegmentReceived, bool escaped = false) : SegmenterBase(onSegmentReceived, SegmentionOptions.SkipInvalidSegment)
{
    /// <inheritdoc />
    protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer)
    {
        if (buffer.IsEmpty)
            return (SegmentationStatus.Incomplete, null);

        var raw = buffer.ToArray();
        switch (XBeeFraming.TryRead(raw, escaped, out _, out var consumed))
        {
            case XBeeReadStatus.Complete:
                return (SegmentationStatus.Complete, buffer.Slice(0, consumed));
            case XBeeReadStatus.Incomplete:
                return (SegmentationStatus.Incomplete, null);
            default:
                var next = raw.AsSpan(1).IndexOf(XBeeFraming.StartDelimiter);
                return (SegmentationStatus.Invalid, buffer.Slice(0, next < 0 ? raw.Length : next + 1));
        }
    }
}
