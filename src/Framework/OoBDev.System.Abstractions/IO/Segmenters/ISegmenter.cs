using System.Buffers;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Provides functionality for segmenting byte sequences according to defined rules.
/// </summary>
public interface ISegmenter
{
    /// <summary>
    /// Attempts to read and extract a segment from the provided buffer asynchronously.
    /// </summary>
    /// <param name="buffer">The byte sequence to segment.</param>
    /// <returns>A task representing the asynchronous operation, containing the segmentation result.</returns>
    ValueTask<ISegmentReadResult> TryReadAsync(ReadOnlySequence<byte> buffer);
}