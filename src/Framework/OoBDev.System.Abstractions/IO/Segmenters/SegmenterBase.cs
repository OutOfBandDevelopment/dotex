using System;
using System.Buffers;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Provides a base implementation for data segmenters that extract segments from byte streams.
/// </summary>
public abstract class SegmenterBase : ISegmenter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SegmenterBase"/> class.
    /// </summary>
    /// <param name="onSegmentReceived">The callback to invoke when a segment is successfully extracted.</param>
    /// <param name="options">Options controlling segmentation behavior.</param>
    protected SegmenterBase(
        OnSegmentReceived onSegmentReceived,
        SegmentionOptions options
        )
    {
        OnSegmentReceived = onSegmentReceived;
        Options = options;
    }

    private OnSegmentReceived OnSegmentReceived { get; }

    /// <summary>
    /// Gets the options controlling segmentation behavior.
    /// </summary>
    public SegmentionOptions Options { get; }

    public async ValueTask<ISegmentReadResult> TryReadAsync(ReadOnlySequence<byte> buffer)
    {
        var result = Read(buffer);
        if (result.status == SegmentationStatus.Complete)
        {
            if (result.segment == null) throw new NotSupportedException("\"Valid\" segmentation without data is not possible");

            await OnSegmentReceived(result.segment.Value);
            buffer = buffer.Slice(buffer.GetPosition(0, result.segment.Value.End));
        }
        else if (result.status == SegmentationStatus.Invalid && Options.HasFlag(SegmentionOptions.SkipInvalidSegment))
        {
            if (result.segment != null)
            {
                buffer = buffer.Slice(buffer.GetPosition(0, result.segment.Value.End)); //Assume this end marks the second start for next segment
            }
            else
            {
                buffer = buffer.Slice(buffer.GetPosition(0, buffer.End)); // if segment isn't provided just fast forward to end
            }

            return new SegmentReadResult(SegmentationStatus.Complete, buffer);
        }

        return new SegmentReadResult(result.status, buffer);
    }

    /// <summary>
    /// When overridden in a derived class, attempts to read and extract a segment from the provided buffer.
    /// </summary>
    /// <param name="buffer">The byte sequence to segment.</param>
    /// <returns>A tuple containing the segmentation status and the extracted segment (if complete).</returns>
    protected abstract (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer);
}