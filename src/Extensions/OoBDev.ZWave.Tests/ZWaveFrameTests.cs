using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.ZWave.Tests;

[TestClass]
public class ZWaveFrameTests
{
    // Byte-exact frames from an Aeotec Z-Stick S2 (Z-Wave 2.78, home id 016A1C28), captured by dev-term.
    internal const string VersionReply = "01 10 01 15 5A 2D 57 61 76 65 20 32 2E 37 38 00 01 9B";
    internal const string MemoryIdReply = "01 08 01 20 01 6A 1C 28 01 88";

    internal static byte[] Bytes(string hex) => Convert.FromHexString(hex.Replace(" ", string.Empty, StringComparison.Ordinal));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetVersionRequest_MatchesCapturedBytes() =>
        CollectionAssert.AreEqual(Bytes("01 03 00 15 E9"), ZWaveRequests.GetVersion().Raw);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetNodeProtocolInfoRequest_MatchesCapturedBytes() =>
        CollectionAssert.AreEqual(Bytes("01 04 00 41 02 B8"), ZWaveRequests.GetNodeProtocolInfo(2).Raw);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Checksum_OfCapturedReplies_MatchesTheirLastByte()
    {
        foreach (var reply in new[] { VersionReply, MemoryIdReply })
        {
            var raw = Bytes(reply);
            Assert.AreEqual(raw[^1], ZWaveFrame.Checksum(raw.AsSpan(1, raw.Length - 2)), reply);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void SendData_WrapsTheCommandForTheNode()
    {
        var frame = ZWaveRequests.SwitchSet(node: 2, on: true, callbackId: 0x05);

        Assert.AreEqual(ZWaveFunctions.SendData, frame.Function);
        CollectionAssert.AreEqual(new byte[] { 0x02, 0x03, 0x25, 0x01, 0xFF, 0x25, 0x05 }, frame.Payload.ToArray());
        Assert.AreEqual(ZWaveFrame.Checksum(frame.Raw.AsSpan(1, frame.Raw.Length - 2)), frame.Raw[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Accessors_OfAResponse_ReadTypeFunctionAndPayload()
    {
        var frame = new ZWaveFrame(ZWaveFrameKind.Data, Bytes(MemoryIdReply));

        Assert.IsTrue(frame.IsResponse);
        Assert.AreEqual(ZWaveFunctions.MemoryGetId, frame.Function);
        CollectionAssert.AreEqual(Bytes("01 6A 1C 28 01"), frame.Payload.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Ack_IsTheLoneAckByte() =>
        CollectionAssert.AreEqual(new byte[] { 0x06 }, ZWaveFrame.Ack.Raw);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encoder_WritesTheRawBytes()
    {
        var frame = ZWaveRequests.GetVersion();

        CollectionAssert.AreEqual(frame.Raw, new ZWaveMessageEncoder().Encode(ref frame).ToArray());
    }
}
