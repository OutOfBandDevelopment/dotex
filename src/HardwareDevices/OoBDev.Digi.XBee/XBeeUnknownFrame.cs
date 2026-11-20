namespace OoBDev.Digi.XBee;

/// <summary>A well-formed frame of a type this package does not decode; the bytes are kept for the caller.</summary>
/// <param name="FrameType">The frame type byte.</param>
/// <param name="Data">The frame data after the type byte.</param>
public sealed record XBeeUnknownFrame(byte FrameType, byte[] Data) : XBeeFrame(FrameType);
