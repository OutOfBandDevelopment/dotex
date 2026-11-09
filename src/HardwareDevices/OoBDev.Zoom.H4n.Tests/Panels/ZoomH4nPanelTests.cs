using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.UiDefinitions;

namespace OoBDev.Zoom.H4n.Panels.Tests;

[TestClass]
[TestCategory(TestCategories.Zoom_H4n)]
public sealed class ZoomH4nPanelTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("serial", true)]
    [DataRow("SERIAL", true)]
    [DataRow("hid", false)]
    [DataRow("tcp", false)]
    public void IsAvailable_Transport_OnlyForSerial(string transport, bool expected) =>
        Assert.AreEqual(expected, new ZoomH4nPanel().IsAvailable(transport, 0, 0));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Identity_IsTheH4nPanelWithItsDecoder()
    {
        var panel = new ZoomH4nPanel();

        Assert.AreEqual("h4n", panel.Id);
        Assert.AreEqual(panel.PresenterName, panel.CreateDecoder().Name);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void BuildDefinition_EveryRemoteButton_HasOneButtonControlTheSurfaceHandles()
    {
        var definition = new ZoomH4nPanel().BuildDefinition();
        var surface = new ZoomH4nControlSurface((_, _) => Task.CompletedTask);

        var buttons = definition.Sections.SelectMany(s => s.Controls).OfType<ButtonControl>().Select(b => b.Id).ToList();

        Assert.HasCount(Enum.GetValues<ZoomH4nButton>().Length, buttons);
        foreach (var id in buttons)
        {
            Assert.IsNotNull(surface.PreviewCommand(id, null), id);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_Record_SendsPressThenRelease()
    {
        var sent = new List<byte[]>();
        var surface = new ZoomH4nPanel().CreateSurface((data, _) =>
        {
            sent.Add(data.ToArray());
            return Task.CompletedTask;
        });

        await surface.InvokeAsync("record", null, TestContext.CancellationToken);

        Assert.HasCount(2, sent);
        CollectionAssert.AreEqual(new byte[] { 0x81, 0x00 }, sent[0]);
        CollectionAssert.AreEqual(new byte[] { 0x80, 0x00 }, sent[1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void PreviewCommand_VolumeUp_ShowsBothWrites() =>
        Assert.AreEqual("80 08 80 00", new ZoomH4nControlSurface((_, _) => Task.CompletedTask).PreviewCommand("volumeUp", null));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_UnknownCommand_Throws()
    {
        var surface = new ZoomH4nControlSurface((_, _) => Task.CompletedTask);

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => surface.InvokeAsync("eject", null, TestContext.CancellationToken));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_StatusBytes_PublishesOnlyChangedIndicators()
    {
        var decoder = new ZoomH4nDecoder();
        var published = new List<IReadOnlyDictionary<string, string>>();
        decoder.ValuesChanged += (_, values) => published.Add(values);

        var lines = decoder.Decode(new ReadOnlySequence<byte>(new byte[] { 0x01, 0x01, 0x03 }));

        Assert.HasCount(3, lines);
        Assert.StartsWith("STATUS: 0x01 Record", lines[0]);
        Assert.HasCount(2, published);
        Assert.AreEqual("On", published[0]["statusRecord"]);
        Assert.AreEqual("Off", published[0]["statusPeak"]);
        Assert.HasCount(1, published[1]);
        Assert.AreEqual("On", published[1]["statusPeak"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WakeByte_IgnoresTheUndocumentedHighBit()
    {
        var decoder = new ZoomH4nDecoder();

        var lines = decoder.Decode(new ReadOnlySequence<byte>(new byte[] { 0x80 }));

        Assert.AreEqual("STATUS: 0x80 none", lines.Single());
    }

    public required TestContext TestContext { get; set; }
}
