using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.ZWave.Tests;

[TestClass]
public class ZWaveAckResponderTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Respond_DataFrame_AcksIt()
    {
        // Stage
        var frame = new ZWaveFrame(ZWaveFrameKind.Data, ZWaveFrameTests.Bytes(ZWaveFrameTests.VersionReply));

        // Test
        var answer = ZWaveAckResponder.Respond(frame);

        // Assert
        Assert.AreSame(ZWaveFrame.Ack, answer);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Respond_FrameWithBadChecksum_NaksIt()
    {
        // Stage
        var raw = ZWaveFrameTests.Bytes(ZWaveFrameTests.VersionReply);
        raw[^1] ^= 0xFF;
        var frame = new ZWaveFrame(ZWaveFrameKind.Invalid, raw);

        // Test
        var answer = ZWaveAckResponder.Respond(frame);

        // Assert
        Assert.AreSame(ZWaveFrame.Nak, answer);
        CollectionAssert.AreEqual(new byte[] { 0x15 }, answer!.Raw);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Respond_LoneControlBytesAndStrayBytes_AnswersNothing()
    {
        // Test / Assert
        Assert.IsNull(ZWaveAckResponder.Respond(ZWaveFrame.Ack));
        Assert.IsNull(ZWaveAckResponder.Respond(ZWaveFrame.Nak));
        Assert.IsNull(ZWaveAckResponder.Respond(new ZWaveFrame(ZWaveFrameKind.Can, [ZWaveFrame.CanByte])));
        Assert.IsNull(ZWaveAckResponder.Respond(new ZWaveFrame(ZWaveFrameKind.Invalid, [0x42])));
        Assert.IsNull(ZWaveAckResponder.Respond(new ZWaveFrame(ZWaveFrameKind.Invalid, [ZWaveFrame.StartOfFrame])));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Respond_Null_Throws() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => ZWaveAckResponder.Respond(null!));
}
