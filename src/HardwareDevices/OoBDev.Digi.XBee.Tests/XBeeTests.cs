using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Digi.XBee;
using OoBDev.TestUtilities;

namespace OoBDev.Digi.XBee.Tests;

[TestClass]
public class XBeeTests
{
    public required TestContext TestContext { get; set; }

    // 7E 00 0A 01 01 50 01 00 48 65 6C 6C 6F B8 is Digi's documented TX example; ours: the AT command NJ with frame id 1 is 7E 00 04 08 01 4E 4A 5E.
    private static readonly byte[] AtNj = [0x7E, 0x00, 0x04, 0x08, 0x01, 0x4E, 0x4A, 0x5E];

    private static async Task<List<byte[]>> PumpAsync(bool escaped, params byte[][] chunks)
    {
        var frames = new List<byte[]>();
        var segmenter = new XBeeSegmenter(segment =>
        {
            frames.Add(segment.ToArray());
            return Task.CompletedTask;
        }, escaped);
        var buffer = Array.Empty<byte>();
        foreach (var chunk in chunks)
        {
            buffer = [.. buffer, .. chunk];
            while (buffer.Length > 0)
            {
                var result = await segmenter.TryReadAsync(new ReadOnlySequence<byte>(buffer));
                if (result.Status == OoBDev.System.IO.Segmenters.SegmentationStatus.Incomplete)
                    break;
                buffer = result.RemainingData.ToArray();
            }
        }

        return frames;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AtCommand_Built_HasLengthFrameIdAndChecksum()
    {
        var frame = XBeeFrames.AtCommand(1, "NJ");

        TestContext.WriteLine("frame {0}", Convert.ToHexString(frame));
        CollectionAssert.AreEqual(AtNj, frame);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AtCommand_BadCommand_Throws() => Assert.Throws<ArgumentException>(() => XBeeFrames.AtCommand(1, "NJX"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Segmenter_NoiseBeforeFrame_SkipsTheNoise()
    {
        var frames = await PumpAsync(false, [0x01, 0x02, 0x03, .. AtNj]);

        Assert.HasCount(1, frames);
        CollectionAssert.AreEqual(AtNj, frames[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Segmenter_BadChecksum_ResyncsOnNextDelimiter()
    {
        var bad = (byte[])AtNj.Clone();
        bad[^1] ^= 0xFF;

        var frames = await PumpAsync(false, [.. bad, .. AtNj]);

        Assert.HasCount(1, frames, "the bad frame is dropped and the good one after it is found");
        CollectionAssert.AreEqual(AtNj, frames[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Segmenter_FrameSplitAcrossReads_WaitsForTheRest()
    {
        var frames = await PumpAsync(false, AtNj[..5], AtNj[5..]);

        Assert.HasCount(1, frames);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Segmenter_AbsurdLength_IsRejectedNotAwaited()
    {
        var frames = await PumpAsync(false, [0x7E, 0xFF, 0xFF, 0x10, 0x00, .. AtNj]);

        Assert.HasCount(1, frames);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Escaped_FrameWithSpecialBytes_RoundTrips()
    {
        // frame data containing 7E, 7D, 11 and 13 must be escaped on the wire and restored on read
        byte[] data = [0x08, 0x7E, 0x7D, 0x11, 0x13];

        var wire = XBeeFraming.Encode(data, escaped: true);
        var status = XBeeFraming.TryRead(wire, true, out var back, out var consumed);

        TestContext.WriteLine("wire {0}", Convert.ToHexString(wire));
        Assert.AreEqual(XBeeReadStatus.Complete, status);
        Assert.AreEqual(wire.Length, consumed);
        CollectionAssert.AreEqual(data, back);
        Assert.AreEqual(1, wire.Count(b => b == 0x7E), "only the delimiter is left unescaped");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Segmenter_EscapedMode_ReturnsTheEscapedFrame()
    {
        var wire = XBeeFraming.Encode([0x08, 0x7E, 0x01], escaped: true);

        var frames = await PumpAsync(true, wire);

        Assert.HasCount(1, frames);
        CollectionAssert.AreEqual(wire, frames[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_ReceivePacket_GivesAddressAndPayload()
    {
        byte[] data = [0x90, 0x00, 0x13, 0xA2, 0x00, 0x40, 0xAB, 0xCD, 0xEF, 0xFF, 0xFE, 0x01, 0x48, 0x69];
        var wire = XBeeFraming.Encode(data);

        Assert.IsTrue(XBeeFrameDecoder.TryDecode(wire, false, out var frame));

        var packet = (XBeeReceivePacket)frame!;
        Assert.AreEqual("0013A20040ABCDEF", packet.AddressText);
        Assert.AreEqual("Hi", global::System.Text.Encoding.ASCII.GetString(packet.Payload));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_IoSample_GivesDigitalAndAnalogLines()
    {
        // one digital line (DIO2 high) and AD1 = 0x0200
        byte[] data = [0x92, 0, 0x13, 0xA2, 0, 0x40, 0xAB, 0xCD, 0xEF, 0xFF, 0xFE, 0x01, 0x01, 0x00, 0x04, 0x02, 0x00, 0x04, 0x02, 0x00];

        var frame = (XBeeIoSample)XBeeFrameDecoder.DecodeData(data);

        Assert.IsTrue(frame.IsHigh(2));
        Assert.IsFalse(frame.IsHigh(1));
        Assert.AreEqual((ushort)0x0200, frame.Analog[1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_AtResponse_GivesStatusAndValue()
    {
        var frame = (XBeeAtCommandResponse)XBeeFrameDecoder.DecodeData([0x88, 0x01, 0x4E, 0x4A, 0x00, 0xFF]);

        Assert.AreEqual("NJ", frame.Command);
        Assert.IsTrue(frame.IsOk);
        CollectionAssert.AreEqual(new byte[] { 0xFF }, frame.Value);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_ShortBodyAndUnknownType_BecomeUnknownFrames()
    {
        Assert.IsInstanceOfType<XBeeUnknownFrame>(XBeeFrameDecoder.DecodeData([0x90, 0x01]));
        Assert.IsInstanceOfType<XBeeUnknownFrame>(XBeeFrameDecoder.DecodeData([0xA1, 0x01]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TransmitRequest_Built_RoundTripsThroughTheDecoderEnvelope()
    {
        var wire = XBeeFrames.TransmitRequest(1, 0x0013A20040ABCDEF, "Hi"u8);

        Assert.AreEqual(XBeeReadStatus.Complete, XBeeFraming.TryRead(wire, false, out var data, out _));
        Assert.AreEqual(XBeeFrameTypes.TransmitRequest, data[0]);
        Assert.AreEqual(16, data.Length);
        Assert.AreEqual(wire.Length - 4, data.Length);
    }
}
