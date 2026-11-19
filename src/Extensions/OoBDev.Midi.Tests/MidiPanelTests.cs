using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Midi.Panels;
using OoBDev.TestUtilities;
using OoBDev.UiDefinitions;

namespace OoBDev.Midi.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class MidiPanelTests
{
    public required TestContext TestContext { get; set; }


    [TestMethod]
    public void Panel_Serial_IsOfferedAndDecoderNamed()
    {
        var panel = new MidiPanel();

        Assert.IsTrue(panel.IsAvailable("serial", 0, 0));
        Assert.IsFalse(panel.IsAvailable("hid", 0, 0));
        Assert.AreEqual(panel.PresenterName, panel.CreateDecoder().Name);
    }

    [TestMethod]
    public async Task InvokeAsync_SlidersThenNoteOn_SendsTheChosenMessage()
    {
        var sent = new List<byte[]>();
        var surface = new MidiControlSurface((d, _) => { sent.Add(d.ToArray()); return Task.CompletedTask; });

        await surface.InvokeAsync("channel", "3", TestContext.CancellationToken);
        await surface.InvokeAsync("note", "64", TestContext.CancellationToken);
        await surface.InvokeAsync("velocity", "90", TestContext.CancellationToken);
        await surface.InvokeAsync("noteOn", null, TestContext.CancellationToken);
        await surface.InvokeAsync("noteOff", null, TestContext.CancellationToken);

        TestContext.WriteLine(string.Join(" | ", sent.Select(global::System.Convert.ToHexString)));
        CollectionAssert.AreEqual(new byte[] { 0x92, 64, 90 }, sent[0]);
        CollectionAssert.AreEqual(new byte[] { 0x82, 64, 0 }, sent[1]);
    }

    [TestMethod]
    public async Task InvokeAsync_SliderOutOfRange_IsClamped()
    {
        byte[]? last = null;
        var surface = new MidiControlSurface((d, _) => { last = d.ToArray(); return Task.CompletedTask; });

        await surface.InvokeAsync("channel", "99", TestContext.CancellationToken);
        await surface.InvokeAsync("allNotesOff", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0xBF, 123, 0 }, last);
    }

    [TestMethod]
    public async Task InvokeAsync_UnknownOrBadValue_Throws()
    {
        var surface = new MidiControlSurface((_, _) => Task.CompletedTask);

        await Assert.ThrowsExactlyAsync<global::System.ArgumentException>(() => surface.InvokeAsync("nope", null, TestContext.CancellationToken));
        await Assert.ThrowsExactlyAsync<global::System.ArgumentException>(() => surface.InvokeAsync("note", "x", TestContext.CancellationToken));
    }

    [TestMethod]
    public void BuildDefinition_EveryButton_HasAPreview()
    {
        var surface = new MidiControlSurface((_, _) => Task.CompletedTask);

        var ids = new MidiPanel().BuildDefinition().Sections.SelectMany(s => s.Controls).OfType<ButtonControl>().Select(b => b.Id).ToList();

        Assert.HasCount(9, ids);
        foreach (var id in ids)
        {
            Assert.IsNotNull(surface.PreviewCommand(id, null), id);
        }

        Assert.AreEqual("90 3C 64", surface.PreviewCommand("noteOn", null));
    }

    [TestMethod]
    public void Decode_NotesClockAndStart_ListsAllButTheClock()
    {
        var decoder = new MidiDecoder();
        var values = new List<string>();
        decoder.ValuesChanged += (_, v) => values.Add(v["last"]);

        var lines = decoder.Decode(new ReadOnlySequence<byte>(new byte[] { 0x91, 60, 100, 0xF8, 60, 0, 0xFA }));

        TestContext.WriteLine(string.Join(" | ", lines));
        CollectionAssert.AreEqual(new[] { "Note on ch2 60 vel 100", "Note off ch2 60", "Start" }, lines.ToArray());
        Assert.HasCount(3, values);
    }
}
