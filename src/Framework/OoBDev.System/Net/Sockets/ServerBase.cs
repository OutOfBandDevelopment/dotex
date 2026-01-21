using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Sockets;

/// <summary>
/// Provides a base implementation for TCP socket servers with asynchronous client handling.
/// Manages client connections, message processing, and server lifecycle.
/// </summary>
public abstract class ServerBase : IServerBase
{
    /// <summary>
    /// Gets the IP address the server listens on.
    /// </summary>
    protected IPAddress IPAddress { get; init; }

    /// <summary>
    /// Gets the port number the server listens on.
    /// </summary>
    protected ushort Port { get; init; }

    /// <summary>
    /// Initializes a new instance of the ServerBase class.
    /// </summary>
    /// <param name="ipAddress">The IP address to listen on. Defaults to IPAddress.Loopback if null.</param>
    /// <param name="port">The port number to listen on. Defaults to 65535.</param>
    protected ServerBase(IPAddress? ipAddress = default, ushort port = 65535)
    {
        IPAddress = ipAddress ?? IPAddress.Loopback;
        Port = port;
    }

    /// <summary>
    /// Starts the TCP server and begins listening for client connections.
    /// Initializes the TCP listener and starts the service loop for accepting clients.
    /// </summary>
    /// <exception cref="ApplicationException">Thrown if the server is already started.</exception>
    public void Start()
    {
        if (_listener != null)
            throw new ApplicationException("Already Started!");
        Console.WriteLine($"{GetType()}::Starting: {Environment.CurrentManagedThreadId} [{IPAddress}:{Port}]");
        _listener = new TcpListener(IPAddress, Port);
        _listener.Start();
        _cts = new CancellationTokenSource();

        var serviceLoopTask = Task.Run(() => ServiceLoopAsync(_listener, _cts.Token));
        var startTask = Task.Run(() => OnStartAsync(_cts.Token));

        _task = Task.WhenAll(serviceLoopTask, startTask);
    }

    /// <summary>
    /// Called when the server starts. Override this method to perform custom initialization logic.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Stops the server and closes all client connections.
    /// </summary>
    /// <returns>An IAsyncDisposable representing the server instance for cleanup.</returns>
    public async Task<IAsyncDisposable> StopAsync()
    {
        _cts?.Cancel();
        await Task.Yield();

        foreach (var client in _clients)
        {
            if (client.Value.Connected)
            {
                client.Value.Close();
                await Task.Yield();
            }
        }

        _listener?.Stop();
        await Task.Yield();
        return this;
    }

    /// <summary>
    /// Runs the main service loop that accepts incoming client connections.
    /// Creates a new task for each accepted client and manages cleanup of completed tasks.
    /// </summary>
    /// <param name="listener">The TCP listener for accepting connections.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task ServiceLoopAsync(TcpListener listener, CancellationToken cancellationToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var clientIdSeed = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine($"{GetType()}::ServiceLoopAsync::Listening: {Environment.CurrentManagedThreadId}");
                var accepted = await listener.AcceptTcpClientAsync(cts.Token);
                var clientId = clientIdSeed++;
                _clients.Add(clientId, accepted);

                var clientTask = Task.Run(async () =>
                {
                    Console.WriteLine($"{GetType()}::ServiceLoopAsync::Accepted: {clientId}-{Environment.CurrentManagedThreadId}");
                    await AcceptClientAsync(clientId, accepted, cts.Token);
                    Console.WriteLine($"{GetType()}::ServiceLoopAsync::Closed:   {clientId}-{Environment.CurrentManagedThreadId}");
                });
                _tasks.Add(clientTask);

                await Task.Yield();

                var areCompleted = _tasks.Where(t => t.IsCompleted).ToArray();
                foreach (var completed in areCompleted)
                    _tasks.Remove(completed);

                var areNotCollected = _clients.Where(c => !c.Value.Connected).ToArray();
                foreach (var notCollected in areNotCollected)
                    _clients.Remove(notCollected.Key);
            }
            catch (OperationCanceledException ocex)
            {
                Console.WriteLine($"{GetType()}::ServiceLoopAsync::Canceled: {Environment.CurrentManagedThreadId} ({ocex.Message})");
            }
        }
    }

    /// <summary>
    /// Handles an accepted client connection. Reads data from the client stream and processes messages.
    /// Override MessageReceivedAsync to customize message handling.
    /// </summary>
    /// <param name="clientId">The unique identifier for this client connection.</param>
    /// <param name="accepted">The accepted TcpClient connection.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task AcceptClientAsync(int clientId, TcpClient accepted, CancellationToken cancellationToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var stream = accepted.GetStream();
        Memory<byte> buffer = new byte[1024];
        while (!cancellationToken.IsCancellationRequested && accepted.Connected)
        {
            try
            {
                if (stream.CanRead)
                {
                    var readLength = await stream.ReadAsync(buffer, cts.Token);
                    if (readLength > 0)
                    {
                        var sliced = buffer[..readLength];
                        Console.WriteLine($"{GetType()}: {clientId}-{Environment.CurrentManagedThreadId}: {Encoding.UTF8.GetString(sliced.ToArray())}");
                        await MessageReceivedAsync(clientId, accepted, sliced, cts.Token);
                    }
                    else if (readLength <= 0)
                    {
                        break;
                    }
                }
                await Task.Yield();
            }
            catch (OperationCanceledException ocex)
            {
                Console.WriteLine($"{GetType()}::ServiceLoopAsync::Canceled: {clientId}-{Environment.CurrentManagedThreadId} ({ocex.Message})");
            }
        }
    }

    /// <summary>
    /// Called when a message is received from a client. Implement this method to process incoming messages.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client connection.</param>
    /// <param name="accepted">The TcpClient connection that sent the message.</param>
    /// <param name="message">The received message data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task MessageReceivedAsync(int clientId, TcpClient accepted, Memory<byte> message, CancellationToken cancellationToken);

    private CancellationTokenSource? _cts;
    private TcpListener? _listener;
    private Task? _task;
    private readonly Dictionary<int, TcpClient> _clients = [];
    private readonly List<Task> _tasks = [];

    /// <summary>
    /// Gets a read-only dictionary of currently connected clients keyed by their unique identifiers.
    /// </summary>
    protected IReadOnlyDictionary<int, TcpClient> Clients => _clients;

    /// <summary>
    /// Disposes the server asynchronously, stopping all operations and closing all client connections.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _listener?.Stop();

        await Task.Yield();

        await Task.WhenAll(
            _task ?? Task.CompletedTask,
            Task.WhenAll(_tasks)
            );

        await Task.Yield();

        foreach (var client in _clients)
        {
            client.Value.Dispose();
            await Task.Yield();
        }

        await Task.Yield();

        _cts?.Dispose();
    }
}
