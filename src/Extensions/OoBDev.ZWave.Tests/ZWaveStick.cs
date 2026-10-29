using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
using OoBDev.System.IO.Ports;
using System;
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
    private readonly SerialPort _port;
    private readonly CancellationTokenSource _cancel = new();
    private readonly ZWaveSession _session;
    private readonly List<ZWaveFrame> _frames = [];

    private ZWaveStick(SerialPort port)
    {
        _port = port;
        var device = new StreamDevice<ZWaveFrame>(new SerialPortDeviceAdapter(port), new ZWaveDevice(), minimumTransmissionDelay: 50, token: _cancel.Token);
        _session = new ZWaveSession(device);
        _session.FrameReceived += (_, frame) =>
        {
            lock (_frames) _frames.Add(frame);
        };
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
        var port = new SerialPort(portName, 115200, global::System.IO.Ports.Parity.None, 8, global::System.IO.Ports.StopBits.One) { ReadTimeout = 100 };
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
        return new ZWaveStick(port);
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
        _port.Dispose();
        _cancel.Dispose();
    }
}
