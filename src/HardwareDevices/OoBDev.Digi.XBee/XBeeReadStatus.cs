namespace OoBDev.Digi.XBee;

/// <summary>The outcome of reading one API frame from the front of a byte buffer.</summary>
public enum XBeeReadStatus
{
    /// <summary>A whole frame with a good checksum was read.</summary>
    Complete,

    /// <summary>The buffer starts like a frame but ends before the frame does; wait for more bytes.</summary>
    Incomplete,

    /// <summary>The buffer does not start with a valid frame (no start delimiter, bad length, bad checksum, or a stray delimiter inside an escaped frame).</summary>
    Invalid,
}
