using OoBDev.System.IO;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.Extensions.IO;

/// <summary>
/// Provides methods for working with streams.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Create an in memory copy of the provided stream
    /// </summary>
    /// <param name="stream">stream to copy</param>
    /// <returns>copy of stream</returns>
    public static Stream CopyOf(this Stream stream)
    {
        var ms = new MemoryStream();
        stream.CopyTo(ms);
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// The default length of context used when splitting files.
    /// </summary>
    public const int DefaultChunkLength = 768;

    /// <summary>
    /// The default overlap between chunks when splitting files.
    /// </summary>
    public const int DefaultOverlap = 0;

    /// <summary>
    /// Asynchronously splits a file into chunks of specified length and overlap.
    /// </summary>
    /// <param name="stream">The stream to split.</param>
    /// <param name="contextLength">The length of each chunk.</param>
    /// <param name="overlap">The overlap between consecutive chunks.</param>
    /// <returns>An asynchronous enumerable sequence of <see cref="ContentChunk"/> representing the chunks of the file.</returns>
    public static async IAsyncEnumerable<ContentChunk> SplitStreamAsync(
        this Stream stream,
        int contextLength = DefaultChunkLength,
        int overlap = DefaultOverlap
        )
    {
        using var reader = new StreamReader(stream, leaveOpen: true);

        var buffer = new char[contextLength];

        var sequence = 0;
        while (stream.Position < stream.Length)
        {
            var start = stream.Position;
            var read = await reader.ReadBlockAsync(buffer, 0, contextLength);
            if (read > 0)
                yield return new(new string(buffer, 0, read), sequence, start, read);
            else if (read < contextLength)
                break;
            stream.Position -= overlap;
            sequence++;
        }
    }
}
