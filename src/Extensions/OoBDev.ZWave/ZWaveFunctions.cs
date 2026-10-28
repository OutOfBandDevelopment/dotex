namespace OoBDev.ZWave;

/// <summary>
/// Serial API function ids (Silicon Labs INS12350) the package builds requests for.
/// </summary>
public static class ZWaveFunctions
{
    /// <summary>Node bitmap and chip information.</summary>
    public const byte GetInitData = 0x02;

    /// <summary>A command class report or request arriving from a node.</summary>
    public const byte ApplicationCommandHandler = 0x04;

    /// <summary>Controller role flags.</summary>
    public const byte GetControllerCapabilities = 0x05;

    /// <summary>Serial API version, manufacturer ids and supported functions.</summary>
    public const byte GetCapabilities = 0x07;

    /// <summary>Restart the Serial API application.</summary>
    public const byte SoftReset = 0x08;

    /// <summary>Send a command class payload to a node.</summary>
    public const byte SendData = 0x13;

    /// <summary>Library name and version string.</summary>
    public const byte GetVersion = 0x15;

    /// <summary>Home id and the controller's own node id.</summary>
    public const byte MemoryGetId = 0x20;

    /// <summary>Listening, routing and device class bytes for a node.</summary>
    public const byte GetNodeProtocolInfo = 0x41;
}
