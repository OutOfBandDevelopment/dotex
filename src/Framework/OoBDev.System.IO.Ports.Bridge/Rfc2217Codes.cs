namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>Telnet (RFC 854) and COM-PORT-OPTION (RFC 2217) byte values.</summary>
public static class Rfc2217Codes
{
    public const byte Se = 240;
    public const byte Sb = 250;
    public const byte Will = 251;
    public const byte Wont = 252;
    public const byte Do = 253;
    public const byte Dont = 254;
    public const byte Iac = 255;

    public const byte OptionBinary = 0;
    public const byte OptionSuppressGoAhead = 3;
    public const byte OptionComPort = 44;

    /// <summary>Server-to-client replies use the client command code plus this offset.</summary>
    public const byte ServerOffset = 100;

    public const byte Signature = 0;
    public const byte SetBaudRate = 1;
    public const byte SetDataSize = 2;
    public const byte SetParity = 3;
    public const byte SetStopSize = 4;
    public const byte SetControl = 5;
    public const byte NotifyLineState = 6;
    public const byte NotifyModemState = 7;
    public const byte FlowControlSuspend = 8;
    public const byte FlowControlResume = 9;
    public const byte SetLineStateMask = 10;
    public const byte SetModemStateMask = 11;
    public const byte PurgeData = 12;
}
