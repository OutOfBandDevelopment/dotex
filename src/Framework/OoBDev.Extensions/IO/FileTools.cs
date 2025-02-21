using OoBDev.System.IO;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.Extensions.IO;

/// <summary>
/// Provides methods for working with files.
/// </summary>
public static class FileTools
{
    /// <summary>
    /// Asynchronously splits a file into chunks of specified length and overlap.
    /// </summary>
    /// <param name="filename">The path of the file to split.</param>
    /// <param name="chunkLength">The length of each chunk.</param>
    /// <param name="overlap">The overlap between consecutive chunks.</param>
    /// <returns>An asynchronous enumerable sequence of <see cref="ContentChunk"/> representing the chunks of the file.</returns>
    public static async IAsyncEnumerable<ContentChunk> SplitFileAsync(
        string filename,
        int chunkLength = StreamExtensions.DefaultChunkLength,
        int overlap = StreamExtensions.DefaultOverlap
        )
    {
        using var file = File.OpenRead(filename);

        await foreach (var item in file.SplitStreamAsync(chunkLength, overlap))
            yield return item;
    }
}
