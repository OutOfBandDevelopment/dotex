using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.ZWave.Tests;

[TestClass]
public class ZWaveStreamTests
{
    private static SegmentPump<ZWaveFrame> Pump() => new(new ZWaveDevice());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_AckThenReply_YieldsBothInOrderTest()
    {
        var pump = Pump();

        await pump.PushAsync(ZWaveFrameTests.Bytes("06 " + ZWaveFrameTests.VersionReply)).ConfigureAwait(false);

        CollectionAssert.AreEqual(
            new[] { ZWaveFrameKind.Ack, ZWaveFrameKind.Data },
            pump.Messages.Select(m => m.Kind).ToArray());
        Assert.AreEqual(ZWaveFunctions.GetVersion, pump.Messages[1].Function);
        Assert.AreEqual(0, pump.Pending.Length);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_FrameSplitAcrossReads_WaitsForTheRestTest()
    {
        var pump = Pump();
        var bytes = ZWaveFrameTests.Bytes(ZWaveFrameTests.MemoryIdReply);

        await pump.PushAsync(bytes.AsMemory(0, 1)).ConfigureAwait(false);
        await pump.PushAsync(bytes.AsMemory(1, 4)).ConfigureAwait(false);
        Assert.AreEqual(0, pump.Messages.Count);
        await pump.PushAsync(bytes.AsMemory(5)).ConfigureAwait(false);

        Assert.AreEqual(1, pump.Messages.Count);
        Assert.AreEqual(ZWaveFrameKind.Data, pump.Messages[0].Kind);
        CollectionAssert.AreEqual(bytes, pump.Messages[0].Raw);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_NakAndCan_AreReportedNotDroppedTest()
    {
        var pump = Pump();

        await pump.PushAsync(ZWaveFrameTests.Bytes("15 18")).ConfigureAwait(false);

        CollectionAssert.AreEqual(
            new[] { ZWaveFrameKind.Nak, ZWaveFrameKind.Can },
            pump.Messages.Select(m => m.Kind).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_StrayBytesBeforeAFrame_ResynchroniseOnTheFrameTest()
    {
        var pump = Pump();

        await pump.PushAsync(ZWaveFrameTests.Bytes("FF 42 " + ZWaveFrameTests.MemoryIdReply)).ConfigureAwait(false);

        CollectionAssert.AreEqual(
            new[] { ZWaveFrameKind.Invalid, ZWaveFrameKind.Invalid, ZWaveFrameKind.Data },
            pump.Messages.Select(m => m.Kind).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_BadChecksum_IsInvalidAndTheNextFrameStillDecodesTest()
    {
        var pump = Pump();

        await pump.PushAsync(ZWaveFrameTests.Bytes("01 04 01 56 01 AE " + ZWaveFrameTests.MemoryIdReply)).ConfigureAwait(false);

        CollectionAssert.AreEqual(
            new[] { ZWaveFrameKind.Invalid, ZWaveFrameKind.Data },
            pump.Messages.Select(m => m.Kind).ToArray());
        Assert.AreEqual(ZWaveFunctions.MemoryGetId, pump.Messages[1].Function);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_ImpossibleLength_IsInvalidAndDoesNotStallTest()
    {
        var pump = Pump();

        await pump.PushAsync(ZWaveFrameTests.Bytes("01 01 " + ZWaveFrameTests.MemoryIdReply)).ConfigureAwait(false);

        Assert.AreEqual(ZWaveFrameKind.Data, pump.Messages[^1].Kind);
        Assert.AreEqual(0, pump.Pending.Length);
    }
}
