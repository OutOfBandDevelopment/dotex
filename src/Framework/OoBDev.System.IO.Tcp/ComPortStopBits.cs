namespace OoBDev.System.IO.Tcp;

/// <summary>The stop-bit values of the RFC 2217 SET-STOPSIZE command.</summary>
public enum ComPortStopBits : byte
{
    /// <summary>One stop bit.</summary>
    One = 1,

    /// <summary>Two stop bits.</summary>
    Two = 2,

    /// <summary>One and a half stop bits.</summary>
    OnePointFive = 3,
}
