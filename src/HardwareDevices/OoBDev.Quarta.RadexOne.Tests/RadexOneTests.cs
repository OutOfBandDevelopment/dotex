using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.System.IO.Messages;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Quarta.RadexOne.Tests;

[TestClass]
public class RadexOneTests
{
    private static byte[] ReadDataExtension(ushort ambient, ushort accumulated, ushort cpm)
    {
        var extension = new byte[22];
        BinaryPrimitives.WriteUInt16LittleEndian(extension, RadexOneCommandCode.ReadData);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(4), 0x000C);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(8), ambient);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(12), accumulated);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(16), cpm);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(20), RadexOneFramer.ComputeChecksum(extension.AsSpan(0, 20)));
        return extension;
    }

    private static byte[] SettingsExtension(byte alarm, ushort threshold)
    {
        var extension = new byte[16];
        BinaryPrimitives.WriteUInt16LittleEndian(extension, RadexOneCommandCode.ReadSettings);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(4), 0x0005);
        extension[8] = alarm;
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(9), threshold);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(14), RadexOneFramer.ComputeChecksum(extension.AsSpan(0, 14)));
        return extension;
    }

    private static byte[] Ack(ushort command)
    {
        var extension = new byte[6];
        BinaryPrimitives.WriteUInt16LittleEndian(extension, command);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(4), RadexOneFramer.ComputeChecksum(extension.AsSpan(0, 4)));
        return extension;
    }

    private static byte[] Encode(RadexOneRequest request) => new RadexOneMessageEncoder().Encode(ref request).ToArray();

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encode_ReadData_MatchesTheCapturedRealDeviceRequest()
    {
        // dev-term source trace: 7BFF 2000 0600 1800 0000 4600 0008 0C00 F3F7
        byte[] expected = [0x7B, 0xFF, 0x20, 0x00, 0x06, 0x00, 0x18, 0x00, 0x00, 0x00, 0x46, 0x00, 0x00, 0x08, 0x0C, 0x00, 0xF3, 0xF7];

        CollectionAssert.AreEqual(expected, Encode(new RadexOneRequest(RadexOneRequestKind.ReadData, 0x0018)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encode_ResetAccumulated_MatchesTheCapturedRealDeviceRequest()
    {
        // 7bff 2000 0600 4e01 0000 0fff 0308 0100 fbf7
        byte[] expected = [0x7B, 0xFF, 0x20, 0x00, 0x06, 0x00, 0x4E, 0x01, 0x00, 0x00, 0x0F, 0xFF, 0x03, 0x08, 0x01, 0x00, 0xFB, 0xF7];

        CollectionAssert.AreEqual(expected, Encode(new RadexOneRequest(RadexOneRequestKind.ResetAccumulated, 0x014E)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_CapturedResetAccumulatedReply_IsAnAcknowledgement()
    {
        // 7aff 2080 0600 4e01 0000 107f 0308 0000 fcf7
        byte[] reply = [0x7A, 0xFF, 0x20, 0x80, 0x06, 0x00, 0x4E, 0x01, 0x00, 0x00, 0x10, 0x7F, 0x03, 0x08, 0x00, 0x00, 0xFC, 0xF7];

        var decoded = new RadexOneMessageDecoder().Decode(new ReadOnlySequence<byte>(reply));

        Assert.AreEqual((ushort)0x014E, decoded.PacketNumber);
        Assert.IsTrue(decoded.IsAcknowledgement);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_ReadDataReply_ReadsTheThreeValues()
    {
        var reply = RadexOneFramer.BuildReply(1, ReadDataExtension(10, 20, 30));

        var decoded = new RadexOneMessageDecoder().Decode(new ReadOnlySequence<byte>(reply));

        Assert.IsTrue(decoded.TryGetReadData(out var data));
        Assert.AreEqual(new RadexOneReadData(10, 20, 30), data);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_ReadDataReplyWithBadInnerChecksum_IsNotAReading()
    {
        var extension = ReadDataExtension(10, 20, 30);
        extension[10] ^= 0xFF;
        var decoded = new RadexOneMessageDecoder().Decode(new ReadOnlySequence<byte>(RadexOneFramer.BuildReply(1, extension)));

        Assert.IsFalse(decoded.TryGetReadData(out _));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_SettingsReply_ReadsAlarmAndThreshold()
    {
        var decoded = new RadexOneMessageDecoder().Decode(new ReadOnlySequence<byte>(RadexOneFramer.BuildReply(1, SettingsExtension(3, 500))));

        Assert.IsTrue(decoded.TryGetSettings(out var settings));
        Assert.AreEqual(new RadexOneSettings(RadexOneAlarmMode.VibrationAndAudio, 500), settings);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_SerialVersionReply_ShowsPrintableAsciiAndDotsForTheRest()
    {
        byte[] extension = [0x01, 0x00, 0x00, 0x00, (byte)'A', (byte)'B', 0x01, (byte)'7', 0x00, 0x00];
        var decoded = new RadexOneMessageDecoder().Decode(new ReadOnlySequence<byte>(RadexOneFramer.BuildReply(1, extension)));

        Assert.AreEqual("AB.7", decoded.SerialVersionText);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WriteSettingsAck_IsAnAcknowledgement()
    {
        var decoded = new RadexOneMessageDecoder().Decode(new ReadOnlySequence<byte>(RadexOneFramer.BuildReply(1, Ack(RadexOneCommandCode.WriteSettings))));

        Assert.IsTrue(decoded.IsAcknowledgement);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_CorruptedHeaderChecksum_Throws()
    {
        var reply = RadexOneFramer.BuildReply(1, ReadDataExtension(1, 2, 3));
        reply[10] ^= 0xFF;

        Assert.ThrowsExactly<ArgumentException>(() => new RadexOneMessageDecoder().Decode(new ReadOnlySequence<byte>(reply)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParseReply_OutboundPrefix_Fails()
    {
        Assert.IsFalse(RadexOneFramer.TryParseReply(RadexOneFramer.BuildRequest(1, []), out _, out _));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParseReply_TruncatedExtension_Fails()
    {
        var packet = RadexOneFramer.BuildReply(1, [1, 2, 3, 4, 5, 6]);

        Assert.IsFalse(RadexOneFramer.TryParseReply(packet.AsSpan(..^2), out _, out _));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encode_WriteSettings_CarriesAlarmAndThresholdAndAValidChecksum()
    {
        var packet = Encode(new RadexOneRequest(RadexOneRequestKind.WriteSettings, 7, new RadexOneSettings(RadexOneAlarmMode.Audio, 0x0123)));

        Assert.AreEqual(12 + 16, packet.Length);
        Assert.AreEqual((byte)2, packet[12 + 8]);
        Assert.AreEqual((ushort)0x0123, BinaryPrimitives.ReadUInt16LittleEndian(packet.AsSpan(12 + 9)));
        Assert.AreEqual(RadexOneFramer.ComputeChecksum(packet.AsSpan(12, 14)), BinaryPrimitives.ReadUInt16LittleEndian(packet.AsSpan(12 + 14)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_NoiseThenSplitReplies_YieldsBothReplies()
    {
        var first = RadexOneFramer.BuildReply(1, ReadDataExtension(1, 2, 3));
        var second = RadexOneFramer.BuildReply(2, ReadDataExtension(4, 5, 6));
        var pump = new SegmentPump<RadexOneReply>(new RadexOneDevice());

        await pump.PushAsync(new byte[] { 0x00, 0x7A, 0x11 }.Concat(first.Take(9)).ToArray()).ConfigureAwait(false);
        Assert.AreEqual(0, pump.Messages.Count);
        await pump.PushAsync(first.Skip(9).Concat(second).ToArray()).ConfigureAwait(false);

        Assert.AreEqual(2, pump.Messages.Count);
        Assert.IsTrue(pump.Messages[1].TryGetReadData(out var data));
        Assert.AreEqual((ushort)6, data.Cpm);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddRadexOne_RegistersEncoderAndDecoder()
    {
        using var provider = new ServiceCollection().TryAddRadexOne().BuildServiceProvider();

        Assert.IsInstanceOfType<RadexOneMessageDecoder>(provider.GetRequiredService<IMessageDecoder<RadexOneReply>>());
        Assert.IsInstanceOfType<RadexOneMessageEncoder>(provider.GetRequiredService<IMessageEncoder<RadexOneRequest>>());
    }
}
