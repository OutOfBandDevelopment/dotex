using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;

namespace OoBDev.System.IO.Ports.Bridge.Tests.TestKit;

/// <summary>Runs a <see cref="BridgeWorker"/> on a loopback ephemeral port against a fake serial factory.</summary>
public sealed class BridgeHarness : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _run;

    public BridgeHarness(
        ISerialPortFactory serial,
        TimeSpan? retry = null,
        BridgeProtocol protocol = BridgeProtocol.Raw,
        IServiceAdvertiser? advertiser = null,
        bool replaceExisting = false,
        TimeSpan? grace = null,
        TimeSpan? writeTimeout = null,
        string comPort = "COMTEST")
    {
        Serial = serial;
        Worker = new BridgeWorker(
            new BridgeOptions { ComPort = comPort, TcpPort = 0, BindAddress = "127.0.0.1", Protocol = protocol, Name = "bench", ReplaceExistingClient = replaceExisting },
            serial,
            NullLogger.Instance,
            retry ?? TimeSpan.FromMilliseconds(20),
            advertiser)
        {
            ClientReleaseGrace = grace ?? TimeSpan.FromMilliseconds(100),
            ClientWriteTimeout = writeTimeout ?? TimeSpan.FromSeconds(10),
        };
        Worker.Start();
        _run = Worker.RunAsync(_cts.Token);
    }

    public ISerialPortFactory Serial { get; }

    public BridgeWorker Worker { get; }

    public async Task<TcpClient> ConnectAsync()
    {
        var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, Worker.BoundPort);
        return client;
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        await _run.WaitAsync(TimeSpan.FromSeconds(5));
        _cts.Dispose();
    }
}
