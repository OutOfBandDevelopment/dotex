namespace OoBDev.System.IO;

/// <summary>The lifecycle state of a device connection.</summary>
public enum ConnectionState
{
    /// <summary>Not connected.</summary>
    Closed,

    /// <summary>Opening is in progress.</summary>
    Opening,

    /// <summary>Connected; <see cref="IDeviceAdapter.Input"/> and <see cref="IDeviceAdapter.Output"/> are valid.</summary>
    Open,

    /// <summary>Closing is in progress.</summary>
    Closing,

    /// <summary>The connection failed and must be closed before it is reopened.</summary>
    Faulted,
}
