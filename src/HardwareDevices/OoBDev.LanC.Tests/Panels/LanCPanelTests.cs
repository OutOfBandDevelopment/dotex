using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions;

namespace OoBDev.LanC.Panels.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class LanCPanelTests
{
    [TestMethod]
    public void IsAvailable_Transport_OnlyForSerial()
    {
        var panel = new LanCPanel();

        Assert.IsTrue(panel.IsAvailable("serial", 0, 0));
        Assert.IsFalse(panel.IsAvailable("ble", 0, 0));
        Assert.AreEqual(panel.PresenterName, panel.CreateDecoder().Name);
    }

    [TestMethod]
    public void BuildDefinition_EveryButton_HasACommandTheSurfaceHandles()
    {
        var surface = new LanCControlSurface((_, _) => Task.CompletedTask);

        var ids = new LanCPanel().BuildDefinition().Sections.SelectMany(s => s.Controls).OfType<ButtonControl>().Select(b => b.Id).ToList();

        Assert.HasCount(LanCUiDefinition.Buttons.Count, ids);
        foreach (var id in ids)
        {
            Assert.IsNotNull(surface.PreviewCommand(id, null), id);
        }
    }

    [TestMethod]
    public async Task InvokeAsync_Play_SendsTheCameraVtrTablePair()
    {
        byte[]? sent = null;
        var surface = new LanCPanel().CreateSurface((data, _) =>
        {
            sent = data.ToArray();
            return Task.CompletedTask;
        });

        await surface.InvokeAsync("play", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x18, 0x34 }, sent);
    }

    [TestMethod]
    public void PreviewCommand_ZoomTele_ShowsTheSpecialTablePair() =>
        Assert.AreEqual("28 06", new LanCControlSurface((_, _) => Task.CompletedTask).PreviewCommand("zoomTele", null));

    [TestMethod]
    public async Task InvokeAsync_UnknownCommand_Throws()
    {
        var surface = new LanCControlSurface((_, _) => Task.CompletedTask);

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => surface.InvokeAsync("beep", null, TestContext.CancellationToken));
    }

    [TestMethod]
    public void Decode_FrameLines_NamesTheStatusAndPublishesChanges()
    {
        var decoder = new LanCDecoder();
        var published = new List<IReadOnlyDictionary<string, string>>();
        decoder.ValuesChanged += (_, values) => published.Add(values);

        var lines = decoder.Decode(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("18-34-06-00\r\n18-34-06-01\n18-34")));

        Assert.HasCount(2, lines);
        Assert.AreEqual("STATUS: Play (18-34-06-00)", lines[0]);
        Assert.HasCount(2, published);
        Assert.AreEqual("Play", published[0]["status"]);
        Assert.IsFalse(published[1].ContainsKey("status"));
        Assert.AreEqual("18-34-06-01", published[1]["frame"]);
    }

    [TestMethod]
    public void Decode_NonHexLine_IsPassedThrough()
    {
        var lines = new LanCDecoder().Decode(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("ready\n")));

        Assert.AreEqual("LINE: ready", lines.Single());
    }

    public required TestContext TestContext { get; set; }
}
