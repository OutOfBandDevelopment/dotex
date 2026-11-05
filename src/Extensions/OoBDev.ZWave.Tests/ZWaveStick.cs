using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
using OoBDev.System.IO.Ports;
using OoBDev.System.IO.Tcp;
using System;
using OoBDev.Devices.TestUtilities;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ZWave.Tests;

/// <summary>
/// A real stick on a serial port for Hardware tests, driven through <see cref="ZWaveSession"/> over a <see cref="StreamDevice{TMessage}"/>.
/// The session ACKs every data frame as it arrives, because the stick retransmits and then answers CAN when a reply goes unacknowledged.
/// </summary>
internal sealed class ZWaveStick : IDisposable
{
    private readonly SerialPort? _port;
    private readonly CancellationTokenSource _cancel = new();
    private readonly ZWaveSession _session;
    private readonly List<ZWaveFrame> _frames = [];

    private ZWaveStick(SerialPort? port, IDeviceAdapter adapter)
    {
        _port = port;
        var device = new StreamDevice<ZWaveFrame>(adapter, new ZWaveDevice(), minimumTransmissionDelay: 50, token: _cancel.Token);
        _session = new ZWaveSession(device);
        _session.FrameReceived += (_, frame) =>
        {
            lock (_frames) _frames.Add(frame);
        };
    }

    private static IDeviceAdapter OpenAdapter(IDeviceAdapter adapter)
    {
        adapter.OpenAsync().GetAwaiter().GetResult();
        return adapter;
    }

    /// <summary>
    /// Opens the stick the run parameters name: <c>ZWAVE_TCP_HOST</c> (<c>host:port</c>, a serial-to-Ethernet bridge) wins over
    /// <c>ZWAVE_SERIAL_PORT</c>; the test ends Inconclusive when neither is set or the stick cannot be reached.
    /// </summary>
    public static ZWaveStick Open(TestContext testContext)
    {
        var endpoint = testContext.Properties["ZWAVE_TCP_HOST"]?.ToString();
        if (string.IsNullOrWhiteSpace(endpoint))
            return Open(DevicePreflight.RequireSerialPort(testContext, "ZWAVE_SERIAL_PORT"));

        endpoint = endpoint.Replace("tcp://", string.Empty, StringComparison.OrdinalIgnoreCase);
        var tcpPort = 0;
        var split = endpoint.LastIndexOf(':');
        if (split <= 0 || !int.TryParse(endpoint[(split + 1)..], out tcpPort))
            Assert.Inconclusive($"'ZWAVE_TCP_HOST' must be host:port, not '{endpoint}'.");

        try
        {
            return new ZWaveStick(null, OpenAdapter(new TcpDeviceAdapter(endpoint[..split], tcpPort)));
        }
        catch (Exception ex) when (ex is IOException or global::System.Net.Sockets.SocketException or InvalidOperationException or TimeoutException)
        {
            Assert.Inconclusive($"'{endpoint}' cannot be reached: {ex.Message}");
            throw;
        }
    }

    /// <summary>A snapshot of the frames received so far.</summary>
    public IList<ZWaveFrame> Frames
    {
        get
        {
            lock (_frames) return [.. _frames];
        }
    }

    /// <summary>Opens the port, or ends the test Inconclusive when it is busy.</summary>
    public static ZWaveStick Open(string portName)
    {
        var port = new SerialPort(portName, 115200, global::System.IO.Ports.Parity.None, 8, global::System.IO.Ports.StopBits.One);
        try
        {
            port.Open();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            port.Dispose();
            Assert.Inconclusive($"'{portName}' cannot be opened (is another program using it?): {ex.Message}");
            throw;
        }

        port.DiscardInBuffer();
        return new ZWaveStick(port, OpenAdapter(new SerialPortDeviceAdapter(port)));
    }

    public void Send(ZWaveFrame frame) => _session.SendAsync(frame).GetAwaiter().GetResult();

    /// <summary>Waits until a frame at or after <paramref name="from"/> matches, or the time runs out.</summary>
    public async Task<ZWaveFrame?> WaitForAsync(Func<ZWaveFrame, bool> match, int from, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            var frames = Frames;
            for (var i = from; i < frames.Count; i++)
                if (match(frames[i])) return frames[i];

            if (DateTime.UtcNow >= deadline) return null;
            await Task.Delay(20).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        Thread.Sleep(500); // lets the last ACK leave the queue, or the stick retransmits into the next test
        _cancel.Cancel();
        _session.Dispose();
        _port?.Dispose();
        _cancel.Dispose();
    }
}
