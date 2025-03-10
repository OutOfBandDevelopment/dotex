﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Sockets;

public abstract class ServerBase : IServerBase
{
    protected IPAddress IPAddress { get; init; }
    protected ushort Port { get; init; }

    protected ServerBase(IPAddress? ipAddress = default, ushort port = 65535)
    {
        IPAddress = ipAddress ?? IPAddress.Loopback;
        Port = port;
    }

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

    protected virtual Task OnStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

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

    protected abstract Task MessageReceivedAsync(int clientId, TcpClient accepted, Memory<byte> message, CancellationToken cancellationToken);

    private CancellationTokenSource? _cts;
    private TcpListener? _listener;
    private Task? _task;
    private readonly Dictionary<int, TcpClient> _clients = [];
    private readonly List<Task> _tasks = [];

    protected IReadOnlyDictionary<int, TcpClient> Clients => _clients;

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
