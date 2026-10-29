using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.ZWave.Tests;

[TestClass]
public class ZWaveReportTests
{
    private static ZWaveFrame Report(byte node, byte value, byte commandClass = 0x25, byte command = 0x03) =>
        ZWaveFrame.Create(ZWaveFrame.Request, ZWaveFunctions.ApplicationCommandHandler, [0x00, node, 0x03, commandClass, command, value]);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryReadBinarySwitch_ReportOn_ReadsNodeAndState()
    {
        Assert.IsTrue(ZWaveReports.TryReadBinarySwitch(Report(5, 0xFF), out var node, out var on));
        Assert.AreEqual(5, node);
        Assert.IsTrue(on);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryReadBinarySwitch_ReportOff_ReadsOff()
    {
        Assert.IsTrue(ZWaveReports.TryReadBinarySwitch(Report(5, 0x00), out _, out var on));
        Assert.IsFalse(on);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryReadBinarySwitch_OtherCommand_IsNotAReport()
    {
        Assert.IsFalse(ZWaveReports.TryReadBinarySwitch(Report(5, 0xFF, commandClass: 0x26), out _, out _));
        Assert.IsFalse(ZWaveReports.TryReadBinarySwitch(Report(5, 0xFF, command: 0x02), out _, out _));
        Assert.IsFalse(ZWaveReports.TryReadBinarySwitch(ZWaveRequests.GetVersion(), out _, out _));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryReadBinarySwitch_ShortPayload_IsNotAReport()
    {
        var frame = ZWaveFrame.Create(ZWaveFrame.Request, ZWaveFunctions.ApplicationCommandHandler, [0x00, 0x05]);

        Assert.IsFalse(ZWaveReports.TryReadBinarySwitch(frame, out _, out _));
    }
}
