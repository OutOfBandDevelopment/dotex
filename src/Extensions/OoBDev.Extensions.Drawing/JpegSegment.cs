namespace OoBDev.Extensions.Drawing;

/// <summary>One marker segment of a JPEG file.</summary>
/// <param name="Index">Position of the segment in the file, from 0.</param>
/// <param name="Type">The marker byte after <c>0xFF</c>, for example <c>0xE1</c> for APP1 (Exif).</param>
/// <param name="Data">The payload: for a start-of-scan segment the header followed by the entropy-coded data; empty for markers without a length.</param>
public record JpegSegment(int Index, byte Type, byte[] Data)
{
    /// <summary>Gets the payload length in bytes.</summary>
    public int Length => Data.Length;
}
