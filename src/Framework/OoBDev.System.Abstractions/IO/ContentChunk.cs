namespace OoBDev.System.IO;

/// <summary>
/// Represents a chunk of content along with metadata.
/// </summary>
public record ContentChunk
{
    /// <summary>
    /// Gets the data of the content chunk.
    /// </summary>
    public string Data { get; }

    /// <summary>
    /// Gets the sequence number of the chunk within the original content.
    /// </summary>
    public int Sequence { get; }

    /// <summary>
    /// Gets the starting position of the chunk within the original content.
    /// </summary>
    public long Start { get; }

    /// <summary>
    /// Gets the length of the chunk.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentChunk"/> class.
    /// </summary>
    /// <param name="data">The data of the content chunk.</param>
    /// <param name="sequence">The sequence number of the chunk within the original content.</param>
    /// <param name="start">The starting position of the chunk within the original content.</param>
    /// <param name="length">The length of the chunk.</param>
    public ContentChunk(string data, int sequence, long start, int length)
    {
        Data = data;
        Sequence = sequence;
        Start = start;
        Length = length;
    }
}
