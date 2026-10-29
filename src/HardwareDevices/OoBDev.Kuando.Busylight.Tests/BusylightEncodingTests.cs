using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Linq;

namespace OoBDev.Kuando.Busylight.Tests;

[TestClass]
public class BusylightEncodingTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encode_SolidGreen_IsTheVerifiedNineByteFrame()
    {
        // verified on a real device by dev-term: report id 00, then NextStep Repeat R G B On Off Audio
        var command = BusylightCommand.Solid(BusylightColors.Green);

        var bytes = new BusylightMessageEncoder().Encode(ref command).ToArray();

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x01, 0x00, 0x80 }, bytes);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Audio_ToByteAndBack_RoundTripsTheFields()
    {
        var audio = new BusylightAudio(true, 5, 3);

        Assert.AreEqual((byte)0b1010_1011, audio.ToByte());
        Assert.AreEqual(audio, BusylightAudio.FromByte(audio.ToByte()));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Audio_MutedDefault_HasNoPlayBit()
    {
        Assert.AreEqual((byte)0x00, BusylightAudio.Muted.ToByte());
        Assert.AreEqual((byte)0x80, BusylightAudio.Default.ToByte());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("red", 255, 0, 0)]
    [DataRow("Off", 0, 0, 0)]
    [DataRow("1,2,3", 1, 2, 3)]
    public void TryParse_PresetOrTriple_ReturnsTheColor(string text, int r, int g, int b)
    {
        Assert.IsTrue(BusylightColors.TryParse(text, out var color));
        Assert.AreEqual(new BusylightColor((byte)r, (byte)g, (byte)b), color);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(null)]
    [DataRow("mauve")]
    [DataRow("1,2")]
    [DataRow("1,2,256")]
    public void TryParse_Garbage_ReturnsFalse(string? text) => Assert.IsFalse(BusylightColors.TryParse(text, out _));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Poll_MatchesTheRecordedRequest()
    {
        // dev-term trace: 8f, zeros, 06 04 55 ff ff ff, checksum 03 eb (sum of the 62 bytes before it)
        var poll = BusylightSequence.Poll();

        Assert.AreEqual(65, poll.Length);
        CollectionAssert.AreEqual(new byte[] { 0x00, 0x8F }, poll.Take(2).ToArray());
        CollectionAssert.AreEqual(new byte[] { 0x06, 0x04, 0x55, 0xFF, 0xFF, 0xFF, 0x03, 0xEB }, poll.TakeLast(8).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encode_SixStepSequence_ChecksumIsBigEndianUnlikeTheBinaryDataDecodersFrame()
    {
        // The BinaryDataDecoders Class1 frame has these steps but ends 86 0C, the same sum (0x0C86) stored little-endian; dev-term traces store it big-endian (03 EB, 04 ED), so that frame is probably why the program never acted on the device
        static BusylightCommand Step(byte next, byte r, byte g, byte b) =>
            new(next, 0x01, new BusylightColor(r, g, b), 0x05, 0x00, BusylightAudio.Muted);

        var frame = BusylightSequence.Encode(
        [
            Step(1, 0xFF, 0x00, 0x00),
            Step(2, 0xFF, 0xFF, 0x00),
            Step(3, 0x00, 0xFF, 0x00),
            Step(4, 0x00, 0xFF, 0xFF),
            Step(5, 0x00, 0x00, 0xFF),
            Step(0, 0xFF, 0x00, 0xFF),
        ]);

        Assert.AreEqual(65, frame.Length);
        CollectionAssert.AreEqual(new byte[] { 0x01, 0x01, 0xFF, 0x00, 0x00, 0x05, 0x00, 0x00 }, frame.Skip(1).Take(8).ToArray());
        CollectionAssert.AreEqual(new byte[] { 0x06, 0x04, 0x55, 0xFF, 0xFF, 0xFF, 0x0C, 0x86 }, frame.TakeLast(8).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encode_NoStepsOrTooMany_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => BusylightSequence.Encode([]));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            BusylightSequence.Encode(Enumerable.Repeat(BusylightCommand.Solid(BusylightColors.Red), 8).ToArray()));
    }
}
