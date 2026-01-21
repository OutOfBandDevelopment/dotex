using System;
using System.Buffers;
using System.Linq;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Segmenter that extracts segments starting with specific byte values and having a fixed or variable length.
/// </summary>
/// <param name="onSegmentReceived">The callback to invoke when a segment is successfully extracted.</param>
/// <param name="starts">The byte values that can mark the start of a segment.</param>
/// <param name="fixedLength">The base fixed length of the segment.</param>
/// <param name="options">Options controlling segmentation behavior.</param>
/// <param name="extensionDefinition">Optional definition for reading an embedded length field to extend the segment.</param>
public sealed class StartAndFixLengthSegmenter(
    OnSegmentReceived onSegmentReceived,
    byte[] starts,
    long fixedLength,
    SegmentionOptions options,
    SegmentExtensionDefinition? extensionDefinition = null) : SegmenterBase(onSegmentReceived, options)
{
    /// <summary>
    /// Gets the byte values that mark the start of a segment.
    /// </summary>
    public byte[] Starts { get; } = starts;

    /// <summary>
    /// Gets the base fixed length of the segment.
    /// </summary>
    public long FixedLength { get; } = fixedLength;

    /// <summary>
    /// Gets the optional definition for reading an embedded length field to extend the segment.
    /// </summary>
    public SegmentExtensionDefinition? ExtensionDefinition { get; } = extensionDefinition;

    /// <inheritdoc/>
    protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer)
    {
        var startOfSegment = Starts.Select(start => buffer.PositionOf(start)).FirstOrDefault(start => start != null);
        if (startOfSegment != null)
        {
            var segment = buffer.Slice(startOfSegment.Value);
            if (segment.Length >= FixedLength)
            {
                var completeSegment = segment.Slice(0, buffer.GetPosition(FixedLength, startOfSegment.Value));

                if (Options.HasFlag(SegmentionOptions.SecondStartInvalid))
                {
                    var secondStart = Starts.Select(start => completeSegment.PositionOf(start)).FirstOrDefault(start => start != null);
                    if (secondStart != null)
                    {
                        // Second start detected
                        return (SegmentationStatus.Invalid, buffer.Slice(0, secondStart.Value));
                    }
                }

                if (ExtensionDefinition != null)
                {
                    var valueData = completeSegment.Slice(ExtensionDefinition.Postion, ExtensionDefinition.Length);
                    //TODO, drop the endian check... only support little and convert 
                    var set = ExtensionDefinition.Endianness == Endianness.Little ? valueData.ToArray() : [.. valueData.ToArray().Reverse()];

                    ulong extendedLength = 0;
                    for (var i = 0; i < ExtensionDefinition.Length; i++)
                    {
                        extendedLength |= (ulong)set[i] << 8 * i;
                    }

                    var actualLength = FixedLength + (long)extendedLength;

                    if (segment.Length < actualLength)
                    {
                        return (SegmentationStatus.Incomplete, buffer);
                    }

                    completeSegment = segment.Slice(0, buffer.GetPosition(actualLength, startOfSegment.Value));
                }

                return (SegmentationStatus.Complete, completeSegment);
            }
        }
        else if (buffer.Length > FixedLength)
        {
            var leftover = buffer.Length % FixedLength;
            buffer = buffer.Slice(0, buffer.GetPosition(-leftover, buffer.End));
            return (SegmentationStatus.Invalid, buffer);
        }

        return (SegmentationStatus.Incomplete, buffer);
    }
}