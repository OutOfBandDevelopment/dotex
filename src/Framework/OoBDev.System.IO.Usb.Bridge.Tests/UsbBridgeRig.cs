using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using OoBDev.System.IO.UsbHids;
using OoBDev.System.IO.UsbTmc;

namespace OoBDev.System.IO.Usb.Bridge.Tests;

internal sealed class UsbBridgeRig : IAsyncDisposable
{
    private readonly UsbBridgeServer _server;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _run;
    private readonly TcpClient _client = new();
    private uint _id;

    private UsbBridgeRig(UsbBridgeServer server)
    {
        _server = server;
        _run = server.RunAsync(_cts.Token);
    }

    public int Port => _server.BoundPort;

    public Stream Stream => _client.GetStream();

    public static async Task<UsbBridgeRig> StartAsync(UsbBridgeOptions options, IUsbTmcDeviceFactory? tmc, IUsbHidDeviceFactory? hid)
    {
        var server = new UsbBridgeServer(options with { BindAddress = "127.0.0.1", TcpPort = 0 }, tmc!, hid!, NullLogger.Instance);
        server.Start();
        var rig = new UsbBridgeRig(server);
        await rig._client.ConnectAsync("127.0.0.1", server.BoundPort);
        return rig;
    }

    public async Task<UsbFrame> CallAsync(UsbBridgeOp op, byte[] payload)
    {
        using var timeout = new CancellationTokenSource(3000);
        await UsbFrameCodec.WriteAsync(Stream, new UsbFrame(op, ++_id, payload), timeout.Token);
        var reply = await UsbFrameCodec.ReadAsync(Stream, timeout.Token);
        return reply ?? new UsbFrame(UsbBridgeOp.Error, _id, new byte[] { 0 });
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _cts.CancelAsync();
        await _run;
        _server.Dispose();
        _cts.Dispose();
    }
}
