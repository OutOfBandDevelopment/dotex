using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace OoBDev.ZWave.Tests;

/// <summary>
/// A real stick on a serial port for Hardware tests. Every data frame the stick sends is ACKed as it is read, because the stick
/// retransmits and then answers CAN when a reply goes unacknowledged.
/// </summary>
internal sealed class ZWaveStick : IDisposable
{
    private readonly SerialPort _port;
    private readonly SegmentPump<ZWaveFrame> _pump = new(new ZWaveDevice());
    private int _acked;

    private ZWaveStick(SerialPort port) => _port = port;

    public IList<ZWaveFrame> Frames => _pump.Messages;

    /// <summary>Opens the port, or ends the test Inconclusive when it is busy.</summary>
    public static ZWaveStick Open(string portName)
    {
        var port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One) { ReadTimeout = 100 };
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

    public void Send(ZWaveFrame frame) => _port.Write(frame.Raw, 0, frame.Raw.Length);

    /// <summary>Reads and ACKs until a frame at or after <paramref name="from"/> matches, or the time runs out.</summary>
    public async Task<ZWaveFrame?> WaitForAsync(Func<ZWaveFrame, bool> match, int from, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        var buffer = new byte[256];
        while (true)
        {
            for (var i = from; i < Frames.Count; i++)
                if (match(Frames[i])) return Frames[i];

            if (DateTime.UtcNow >= deadline) return null;

            try
            {
                var read = _port.Read(buffer, 0, buffer.Length);
                await _pump.PushAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
            }

            for (; _acked < Frames.Count; _acked++)
                if (Frames[_acked].Kind == ZWaveFrameKind.Data) _port.Write(ZWaveFrame.Ack.Raw, 0, 1);
        }
    }

    public void Dispose() => _port.Dispose();
}
