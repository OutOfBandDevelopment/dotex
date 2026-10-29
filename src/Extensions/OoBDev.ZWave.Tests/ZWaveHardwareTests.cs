using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.ZWave.Tests;

[TestClass]
public class ZWaveHardwareTests
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

    public TestContext TestContext { get; set; } = null!;

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task GetVersion_OnTheStick_AnswersWithALibraryNameAndNoCanTest()
    {
        // Stage
        var portName = DevicePreflight.RequireSerialPort(TestContext, "ZWAVE_SERIAL_PORT");
        using var stick = ZWaveStick.Open(portName);

        // Test
        stick.Send(ZWaveRequests.GetVersion());
        var reply = await stick.WaitForAsync(f => f.IsResponse && f.Function == ZWaveFunctions.GetVersion, 0, _timeout).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(reply, "no GetVersion response");
        Assert.IsFalse(stick.Frames.Any(m => m.Kind is ZWaveFrameKind.Can or ZWaveFrameKind.Nak or ZWaveFrameKind.Invalid));
        var text = Encoding.ASCII.GetString(reply.Payload[..^1]);
        TestContext.WriteLine($"GetVersion: {text}");
        StringAssert.StartsWith(text, "Z-Wave ");
    }

    /// <summary>
    /// Switches a real Binary Switch node (a plug-in relay) to the opposite of its current state, confirms the node reports the
    /// new state, and always puts it back. Opt-in on top of the port: it needs <c>ZWAVE_SWITCH_NODE</c> because it actuates
    /// whatever is plugged into that outlet.
    /// </summary>
    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task Switch_AgainstRealNode_TogglesAndRestoresTest()
    {
        // Stage
        var portName = DevicePreflight.RequireSerialPort(TestContext, "ZWAVE_SERIAL_PORT");
        var node = TestContext.GetPropertyOrDefault("ZWAVE_SWITCH_NODE", 0);
        if (node is < 2 or > 232)
            Assert.Inconclusive("No 'ZWAVE_SWITCH_NODE' run parameter: it actuates a real outlet, so name the relay node (2 to 232) in .runsettings.");

        using var stick = ZWaveStick.Open(portName);
        byte callback = 0;

        async Task<bool?> ReadStateAsync()
        {
            var from = stick.Frames.Count;
            stick.Send(ZWaveRequests.SwitchGet((byte)node, ++callback));
            var report = await stick.WaitForAsync(
                f => ZWaveReports.TryReadBinarySwitch(f, out var n, out _) && n == node, from, _timeout).ConfigureAwait(false);
            if (report is null) return null;
            ZWaveReports.TryReadBinarySwitch(report, out _, out var on);
            return on;
        }

        // Test
        var original = await ReadStateAsync().ConfigureAwait(false);
        Assert.IsNotNull(original, "The node did not answer Binary Switch Get.");
        try
        {
            stick.Send(ZWaveRequests.SwitchSet((byte)node, !original.Value, ++callback));
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            await stick.WaitForAsync(_ => false, stick.Frames.Count, TimeSpan.FromMilliseconds(300)).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(!original.Value, await ReadStateAsync().ConfigureAwait(false), "The node did not report the switched state.");
        }
        finally
        {
            stick.Send(ZWaveRequests.SwitchSet((byte)node, original.Value, ++callback));
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            await stick.WaitForAsync(_ => false, stick.Frames.Count, TimeSpan.FromMilliseconds(300)).ConfigureAwait(false);
        }

        // Verify
        Assert.AreEqual(original, await ReadStateAsync().ConfigureAwait(false), "The node was not restored to its original state.");
    }
}
