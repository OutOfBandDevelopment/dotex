using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace OoBDev.Zoom.H4n.Tests;

[TestClass]
public class ZoomH4nHardwareTests
{
    public TestContext TestContext { get; set; } = null!;

    private static void Send(SerialPort port, ZoomH4nCommand command)
    {
        var bytes = new ZoomH4nMessageEncoder().Encode(ref command).ToArray();
        port.Write(bytes, 0, bytes.Length);
    }

    // Probes until a byte with the high bit answers (or the attempts run out), then sends the wake sequence.
    private static async Task<bool> HandshakeAsync(SerialPort port)
    {
        var woke = false;
        for (var attempt = 0; attempt < ZoomH4nProtocol.MaxHandshakeAttempts && !woke; attempt++)
        {
            port.Write([ZoomH4nProtocol.ProbeByte], 0, 1);
            await Task.Delay(ZoomH4nProtocol.HandshakeAttemptTimeout).ConfigureAwait(false);
            while (port.BytesToRead > 0)
                woke |= ZoomH4nProtocol.IsWakeByte((byte)port.ReadByte());
        }

        var wake = ZoomH4nProtocol.WakeSequence.ToArray();
        port.Write(wake, 0, wake.Length);
        return woke;
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task Handshake_ThenMic_CompletesWithoutFaultTest()
    {
        var portName = DevicePreflight.RequireSerialPort(TestContext, "ZOOM_H4N_SERIAL_PORT");
        using var port = new SerialPort(portName, ZoomH4nProtocol.BaudRate, Parity.None, 8, StopBits.One) { Handshake = Handshake.None, ReadTimeout = 200 };
        port.Open();

        var woke = await HandshakeAsync(port).ConfigureAwait(false);
        TestContext.WriteLine($"Recorder answered the handshake: {woke}");
        Send(port, new ZoomH4nCommand(ZoomH4nButton.Mic));
        await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
        Send(port, ZoomH4nCommand.Release);

        while (port.BytesToRead > 0)
        {
            var status = new ZoomH4nMessageDecoder().Decode(new global::System.Buffers.ReadOnlySequence<byte>([(byte)port.ReadByte()]));
            TestContext.WriteLine($"Status: {status}");
        }
    }
}
