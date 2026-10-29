using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.System.IO.Messages;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Zoom.H4n.Tests;

[TestClass]
public class ZoomH4nTests
{
    private static byte[] Encode(ZoomH4nCommand command) => new ZoomH4nMessageEncoder().Encode(ref command).ToArray();

    private static ZoomH4nStatus Decode(params byte[] bytes) => new ZoomH4nMessageDecoder().Decode(new ReadOnlySequence<byte>(bytes));

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(ZoomH4nButton.Record, 0x81, 0x00)]
    [DataRow(ZoomH4nButton.Play, 0x82, 0x00)]
    [DataRow(ZoomH4nButton.Stop, 0x84, 0x00)]
    [DataRow(ZoomH4nButton.FastForward, 0x88, 0x00)]
    [DataRow(ZoomH4nButton.Rewind, 0x90, 0x00)]
    [DataRow(ZoomH4nButton.VolumeUp, 0x80, 0x08)]
    [DataRow(ZoomH4nButton.VolumeDown, 0x80, 0x10)]
    [DataRow(ZoomH4nButton.RecordLevelUp, 0x80, 0x20)]
    [DataRow(ZoomH4nButton.RecordLevelDown, 0x80, 0x40)]
    [DataRow(ZoomH4nButton.Mic, 0x80, 0x01)]
    [DataRow(ZoomH4nButton.Channel1, 0x80, 0x02)]
    [DataRow(ZoomH4nButton.Channel2, 0x80, 0x04)]
    public void Encode_Press_IsTheTwoByteButtonCode(ZoomH4nButton button, int first, int second)
    {
        CollectionAssert.AreEqual(new[] { (byte)first, (byte)second }, Encode(new ZoomH4nCommand(button)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encode_Release_IsTheSharedCodeForAnyButton()
    {
        CollectionAssert.AreEqual(new byte[] { 0x80, 0x00 }, Encode(ZoomH4nCommand.Release));
        CollectionAssert.AreEqual(new byte[] { 0x80, 0x00 }, Encode(new ZoomH4nCommand(ZoomH4nButton.Play, ZoomH4nAction.Release)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encode_UnknownButton_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Encode(new ZoomH4nCommand((ZoomH4nButton)99)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_NoBits_IsNone() => Assert.AreEqual(ZoomH4nStatus.None, Decode(0x00));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_RecordAndPeak_ReadsBoth() => Assert.AreEqual(ZoomH4nStatus.Record | ZoomH4nStatus.Peak, Decode(0x03));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_AllDocumentedBits_ReadsAllFive()
    {
        Assert.AreEqual(ZoomH4nStatus.Record | ZoomH4nStatus.Peak | ZoomH4nStatus.Mic | ZoomH4nStatus.Led1 | ZoomH4nStatus.Led2, Decode(0x73));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_UndocumentedBits_AreDropped() => Assert.AreEqual(ZoomH4nStatus.None, Decode(0x8C));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_MoreThanOneByte_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => Decode(0x01, 0x02));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsWakeByte_IsTheHighBit()
    {
        Assert.IsTrue(ZoomH4nProtocol.IsWakeByte(0x80));
        Assert.IsTrue(ZoomH4nProtocol.IsWakeByte(0xFF));
        Assert.IsFalse(ZoomH4nProtocol.IsWakeByte(0x7F));
        CollectionAssert.AreEqual(new byte[] { 0xA1, 0x80, 0x00 }, ZoomH4nProtocol.WakeSequence.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_ThreeBytes_YieldsThreeIndependentStatuses()
    {
        var pump = new SegmentPump<ZoomH4nStatus>(new ZoomH4nDevice());

        await pump.PushAsync(new byte[] { 0x01, 0x00, 0x30 }).ConfigureAwait(false);

        CollectionAssert.AreEqual(
            new[] { ZoomH4nStatus.Record, ZoomH4nStatus.None, ZoomH4nStatus.Mic | ZoomH4nStatus.Led1 },
            pump.Messages.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddZoomH4n_RegistersEncoderAndDecoder()
    {
        using var provider = new ServiceCollection().TryAddZoomH4n().BuildServiceProvider();

        Assert.IsInstanceOfType<ZoomH4nMessageDecoder>(provider.GetRequiredService<IMessageDecoder<ZoomH4nStatus>>());
        Assert.IsInstanceOfType<ZoomH4nMessageEncoder>(provider.GetRequiredService<IMessageEncoder<ZoomH4nCommand>>());
    }
}
