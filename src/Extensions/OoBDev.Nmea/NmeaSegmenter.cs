using OoBDev.System.IO.Segmenters;
using System.Buffers;

namespace OoBDev.Nmea;

/// <summary>
/// Cuts the stream into lines that start with <c>$</c> and end with a line feed. Bytes before a <c>$</c> are dropped, and a line that
/// passes <see cref="MaxLength"/> bytes without a line feed is abandoned so the search resynchronises on the next <c>$</c>.
/// </summary>
/// <param name="onSegmentReceived">The callback to invoke when a line is extracted.</param>
public sealed class NmeaSegmenter(OnSegmentReceived onSegmentReceived) : SegmenterBase(onSegmentReceived, SegmentionOptions.SkipInvalidSegment)
{
    /// <summary>The longest line kept; the standard maximum is 82, and HID padding is allowed on top.</summary>
    public const int MaxLength = 256;

    /// <inheritdoc/>
    protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer)
    {
        var start = buffer.PositionOf((byte)'$');
        if (start is null) return (SegmentationStatus.Invalid, buffer);

        var line = buffer.Slice(start.Value);
        if (!line.Start.Equals(buffer.Start)) return (SegmentationStatus.Invalid, buffer.Slice(0, start.Value));

        var end = line.PositionOf((byte)'\n');
        if (end is null) return line.Length > MaxLength ? (SegmentationStatus.Invalid, line.Slice(0, 1)) : (SegmentationStatus.Incomplete, null);

        return (SegmentationStatus.Complete, line.Slice(0, line.GetPosition(1, end.Value)));
    }
}
