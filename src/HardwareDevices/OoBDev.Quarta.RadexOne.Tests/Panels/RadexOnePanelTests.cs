using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.UiDefinitions;

namespace OoBDev.Quarta.RadexOne.Panels.Tests;

[TestClass]
[TestCategory(TestCategories.Radex_One)]
public sealed class RadexOnePanelTests
{
    private static byte[] ReadDataReply(ushort ambient, ushort accumulated, ushort cpm)
    {
        var extension = new byte[22];
        BinaryPrimitives.WriteUInt16LittleEndian(extension, RadexOneCommandCode.ReadData);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(4), 0x000C);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(8), ambient);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(12), accumulated);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(16), cpm);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(20), RadexOneFramer.ComputeChecksum(extension.AsSpan(0, 20)));
        return RadexOneFramer.BuildReply(1, extension);
    }

    private static byte[] SettingsReply(byte alarm, ushort threshold)
    {
        var extension = new byte[16];
        BinaryPrimitives.WriteUInt16LittleEndian(extension, RadexOneCommandCode.ReadSettings);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(4), 0x0005);
        extension[8] = alarm;
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(9), threshold);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(14), RadexOneFramer.ComputeChecksum(extension.AsSpan(0, 14)));
        return RadexOneFramer.BuildReply(2, extension);
    }

    private static OoBDev.UiDefinitions.IControlSurface Surface(OoBDev.UiDefinitions.IDevicePanel panel, OoBDev.UiDefinitions.DeviceSend send) =>
        panel.CreateSurface(send, panel.CreateDecoder());

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("serial", true)]
    [DataRow("hid", false)]
    public void IsAvailable_Transport_OnlyForSerial(string transport, bool expected) =>
        Assert.AreEqual(expected, new RadexOnePanel().IsAvailable(transport, 0, 0));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void BuildDefinition_EveryButton_HasACommandTheSurfaceHandles()
    {
        var panel = new RadexOnePanel();
        var surface = new RadexOneControlSurface((_, _) => Task.CompletedTask);

        var buttons = panel.BuildDefinition().Sections.SelectMany(s => s.Controls).OfType<ButtonControl>().Select(b => b.Id).ToList();

        Assert.HasCount(4, buttons);
        foreach (var id in buttons)
        {
            Assert.IsNotNull(surface.PreviewCommand(id, null), id);
        }

        Assert.AreEqual(panel.PresenterName, panel.CreateDecoder().Name);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_ReadDataTwice_SendsTheRequestWithRisingPacketNumbers()
    {
        var sent = new List<byte[]>();
        var surface = Surface(new RadexOnePanel(), (data, _) =>
        {
            sent.Add(data.ToArray());
            return Task.CompletedTask;
        });

        await surface.InvokeAsync("readData", null, TestContext.CancellationToken);
        await surface.InvokeAsync("readData", null, TestContext.CancellationToken);

        var first = new RadexOneRequest(RadexOneRequestKind.ReadData, 1);
        var second = new RadexOneRequest(RadexOneRequestKind.ReadData, 2);
        var encoder = new RadexOneMessageEncoder();
        CollectionAssert.AreEqual(encoder.Encode(ref first).ToArray(), sent[0]);
        CollectionAssert.AreEqual(encoder.Encode(ref second).ToArray(), sent[1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_UnknownCommand_Throws()
    {
        var surface = new RadexOneControlSurface((_, _) => Task.CompletedTask);

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => surface.InvokeAsync("writeSettings", null, TestContext.CancellationToken));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_ReadDataSplitAcrossChunksWithNoise_PublishesTheReadings()
    {
        var decoder = new RadexOneDecoder();
        var published = new List<IReadOnlyDictionary<string, string>>();
        decoder.ValuesChanged += (_, values) => published.Add(values);
        var reply = new byte[] { 0x00, 0x11 }.Concat(ReadDataReply(14, 3049, 18)).ToArray();

        var first = decoder.Decode(new ReadOnlySequence<byte>(reply.AsMemory(0, 10)));
        var second = decoder.Decode(new ReadOnlySequence<byte>(reply.AsMemory(10)));

        Assert.IsEmpty(first);
        Assert.AreEqual("DATA: ambient=14 accumulated=3049 cpm=18", second.Single());
        var values = published.Single();
        Assert.AreEqual("14", values["ambient"]);
        Assert.AreEqual("3049", values["accumulated"]);
        Assert.AreEqual("18", values["cpm"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_SettingsReply_PublishesAlarmAndThreshold()
    {
        var decoder = new RadexOneDecoder();
        IReadOnlyDictionary<string, string>? published = null;
        decoder.ValuesChanged += (_, values) => published = values;

        var lines = decoder.Decode(new ReadOnlySequence<byte>(SettingsReply(2, 20)));

        Assert.AreEqual("SETTINGS: alarm=Audio threshold=20", lines.Single());
        Assert.AreEqual("Audio", published!["alarmMode"]);
        Assert.AreEqual("20", published["threshold"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_TwoRepliesInOneChunk_ReturnsALineEach()
    {
        var decoder = new RadexOneDecoder();
        var both = ReadDataReply(1, 2, 3).Concat(SettingsReply(0, 5)).ToArray();

        var lines = decoder.Decode(new ReadOnlySequence<byte>(both));

        Assert.HasCount(2, lines);
    }

    public required TestContext TestContext { get; set; }
}
