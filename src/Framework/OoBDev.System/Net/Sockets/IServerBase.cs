using System;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Sockets;

/// <summary>
/// Defines the base contract for TCP/IP server implementations.
/// Provides methods for starting, stopping, and disposing of server instances.
/// </summary>
public interface IServerBase : IAsyncDisposable
{
    /// <summary>
    /// Starts the server and begins listening for incoming client connections.
    /// </summary>
    void Start();

    /// <summary>
    /// Asynchronously stops the server and ceases accepting new connections.
    /// Existing connections may be allowed to complete.
    /// </summary>
    /// <returns>A task that completes when the server has stopped, returning a disposable handle.</returns>
    Task<IAsyncDisposable> StopAsync();
}
