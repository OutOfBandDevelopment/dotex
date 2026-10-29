using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.DerEE.De5000.Tests;

[TestClass]
public class De5000StreamTests
{
    private static SegmentPump<De5000Frame> Pump() => new(new De5000Device());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_TwoPackets_YieldsBothTest()
    {
        var pump = Pump();

        await pump.PushAsync(De5000FrameTests.Valid.Concat(De5000FrameTests.Valid).ToArray()).ConfigureAwait(false);

        Assert.AreEqual(2, pump.Messages.Count);
        Assert.AreEqual("Ls", pump.Messages[1].PrimaryQuantity);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_PacketSplitAcrossReads_WaitsForTheRestTest()
    {
        var pump = Pump();
        var bytes = De5000FrameTests.Valid;

        await pump.PushAsync(bytes.AsMemory(0, 10)).ConfigureAwait(false);
        Assert.AreEqual(0, pump.Messages.Count);
        await pump.PushAsync(bytes.AsMemory(10)).ConfigureAwait(false);

        Assert.AreEqual(1, pump.Messages.Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_LeadingGarbage_ResyncsOnTheHeaderTest()
    {
        var pump = Pump();

        await pump.PushAsync(new byte[] { 0x55, 0xAA, 0x01 }.Concat(De5000FrameTests.Valid).ToArray()).ConfigureAwait(false);

        Assert.AreEqual(1, pump.Messages.Count);
        Assert.AreEqual(1.234, pump.Messages[0].PrimaryValue, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Interpret_ValidPacket_FormatsTheLineAndValues()
    {
        De5000Frame.TryParse(De5000FrameTests.Valid, out var frame);

        var reading = De5000Interpreter.Interpret(frame);

        Assert.AreEqual("Ls=1.234mH D=0.012 @1 kHz", reading.Text);
        Assert.AreEqual("Ls=1.234mH", reading.Values["primary"]);
        Assert.AreEqual("0", reading.Values["hold"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void FormatMeasurement_OverloadAndStatus_AreShownTest()
    {
        Assert.AreEqual("Cp=OL", De5000Interpreter.FormatMeasurement("Cp", 0, "nF", "OL"));
        Assert.AreEqual("Rs=5Ohm (FAIL)", De5000Interpreter.FormatMeasurement("Rs", 5, "Ohm", "FAIL"));
    }
}
