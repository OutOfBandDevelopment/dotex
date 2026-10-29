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

    /// <summary>Set the Serial API timeouts.</summary>
    public const byte SetTimeouts = 0x06;

    /// <summary>Reset the controller to defaults: a new home id, every node forgotten.</summary>
    public const byte SetDefault = 0x42;

    /// <summary>Ask a node to refresh its neighbour list.</summary>
    public const byte RequestNodeNeighborUpdate = 0x48;

    /// <summary>A node announced itself or its node information arrived.</summary>
    public const byte ApplicationUpdate = 0x49;

    /// <summary>Include a node.</summary>
    public const byte AddNodeToNetwork = 0x4A;

    /// <summary>Exclude a node.</summary>
    public const byte RemoveNodeFromNetwork = 0x4B;

    /// <summary>The SUC node id.</summary>
    public const byte GetSucNodeId = 0x56;

    /// <summary>Join another network as a secondary controller.</summary>
    public const byte SetLearnMode = 0x50;

    /// <summary>Ask a node for its node information frame.</summary>
    public const byte RequestNodeInfo = 0x60;

    /// <summary>Remove a node the controller considers failed.</summary>
    public const byte RemoveFailedNode = 0x61;

    /// <summary>Whether the controller considers a node failed.</summary>
    public const byte IsFailedNode = 0x62;
}
