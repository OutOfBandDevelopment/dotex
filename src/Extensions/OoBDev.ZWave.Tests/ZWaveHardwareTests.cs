using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.ZWave.Tests;

[TestClass]
public class ZWaveHardwareTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task GetVersion_OnTheStick_AnswersWithALibraryNameAndNoCanTest()
    {
        // Stage
        var portName = DevicePreflight.RequireSerialPort(TestContext, "ZWAVE_SERIAL_PORT");
        using var port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One) { ReadTimeout = 200 };
        try
        {
            port.Open();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            Assert.Inconclusive($"'{portName}' cannot be opened (is another program using it?): {ex.Message}");
        }

        var pump = new SegmentPump<ZWaveFrame>(new ZWaveDevice());
        port.DiscardInBuffer();

        // Test
        var request = ZWaveRequests.GetVersion();
        port.Write(request.Raw, 0, request.Raw.Length);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        var buffer = new byte[256];
        while (DateTime.UtcNow < deadline && !pump.Messages.Any(m => m.IsResponse))
        {
            int read;
            try { read = port.Read(buffer, 0, buffer.Length); }
            catch (TimeoutException) { continue; }
            await pump.PushAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
        }

        var reply = pump.Messages.FirstOrDefault(m => m.IsResponse);
        if (reply is not null) port.Write(ZWaveFrame.Ack.Raw, 0, 1);

        // Assert
        Assert.IsNotNull(reply, "no GetVersion response within 5 seconds");
        Assert.IsFalse(pump.Messages.Any(m => m.Kind is ZWaveFrameKind.Can or ZWaveFrameKind.Nak or ZWaveFrameKind.Invalid));
        Assert.AreEqual(ZWaveFunctions.GetVersion, reply.Function);
        var text = Encoding.ASCII.GetString(reply.Payload[..^1]);
        TestContext.WriteLine($"GetVersion: {text}");
        StringAssert.StartsWith(text, "Z-Wave ");
    }
}
