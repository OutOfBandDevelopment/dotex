namespace OoBDev.Digi.XBee;

/// <summary>A decoded API frame.</summary>
/// <param name="FrameType">The frame type byte.</param>
public abstract record XBeeFrame(byte FrameType);
