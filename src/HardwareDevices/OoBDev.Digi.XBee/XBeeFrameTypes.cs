namespace OoBDev.Digi.XBee;

/// <summary>The frame type bytes this package builds or decodes.</summary>
public static class XBeeFrameTypes
{
    /// <summary>AT command, sent to the local radio.</summary>
    public const byte AtCommand = 0x08;

    /// <summary>Transmit request to a remote radio.</summary>
    public const byte TransmitRequest = 0x10;

    /// <summary>AT command response from the local radio.</summary>
    public const byte AtCommandResponse = 0x88;

    /// <summary>Receive packet from a remote radio.</summary>
    public const byte ReceivePacket = 0x90;

    /// <summary>I/O data sample from a remote radio.</summary>
    public const byte IoDataSample = 0x92;
}
