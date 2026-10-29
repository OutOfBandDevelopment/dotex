namespace OoBDev.DevTerm;

/// <summary>The lifecycle state of a transport.</summary>
public enum ConnectionState
{
    /// <summary>Not connected.</summary>
    Closed,

    /// <summary>Opening is in progress.</summary>
    Opening,

    /// <summary>Connected; <see cref="ITransport.Input"/> is valid.</summary>
    Open,

    /// <summary>Closing is in progress.</summary>
    Closing,

    /// <summary>The connection failed and must be closed before it is reopened.</summary>
    Faulted,
}
