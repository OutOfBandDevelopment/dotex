using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions;
using OoBDev.ZWave.Panels;

namespace OoBDev.ZWave.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class ZWavePanelTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void IsAvailable_Transport_OnlyForSerial()
    {
        // Stage
        var panel = new ZWavePanel();

        // Test / Assert
        Assert.IsTrue(panel.IsAvailable("serial", 0, 0));
        Assert.IsFalse(panel.IsAvailable("tcp", 0, 0));
        Assert.AreEqual(panel.PresenterName, panel.CreateDecoder().Name);
    }

    [TestMethod]
    public void BuildDefinition_EveryButton_HasACommandTheSurfaceHandles()
    {
        // Stage
        var surface = new ZWaveControlSurface((_, _) => Task.CompletedTask);
        var ids = new ZWavePanel().BuildDefinition().Sections.SelectMany(s => s.Controls).OfType<ButtonControl>().Select(b => b.Id).ToList();

        // Test / Assert
        CollectionAssert.AreEquivalent(ZWaveUiDefinition.CommandIds.ToList(), ids);
        foreach (var id in ids)
        {
            Assert.IsNotNull(surface.PreviewCommand(id, id switch { "configSet" => "2,5 1", "sendRaw" => "2,26 01 32", _ => "2,50" }), id);
        }
    }

    [TestMethod]
    public async Task InvokeAsync_SwitchOn_SendsASendDataFrameForTheNode()
    {
        // Stage
        byte[]? sent = null;
        var surface = new ZWavePanel().CreateSurface((data, _) =>
        {
            sent = data.ToArray();
            return Task.CompletedTask;
        });

        // Test
        await surface.InvokeAsync("switchOn", "5", TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(sent);
        var frame = new ZWaveMessageDecoder().Decode(new ReadOnlySequence<byte>(sent));
        Assert.AreEqual(ZWaveFrameKind.Data, frame.Kind);
        Assert.AreEqual(ZWaveFunctions.SendData, frame.Function);
        Assert.AreEqual(5, frame.Payload[0]);
    }

    [TestMethod]
    public async Task InvokeAsync_TwoNetworkCommands_UseDifferentCallbackIds()
    {
        // Stage
        var sent = new List<byte[]>();
        var surface = new ZWaveControlSurface((data, _) =>
        {
            sent.Add(data.ToArray());
            return Task.CompletedTask;
        });

        // Test
        await surface.InvokeAsync("addNode", null, TestContext.CancellationToken);
        await surface.InvokeAsync("addNode", null, TestContext.CancellationToken);

        // Assert
        Assert.AreNotEqual(sent[0][^2], sent[1][^2]);
    }

    [TestMethod]
    public async Task InvokeAsync_SetLevel_CarriesNodeAndLevel()
    {
        // Stage
        byte[]? sent = null;
        var surface = new ZWaveControlSurface((data, _) =>
        {
            sent = data.ToArray();
            return Task.CompletedTask;
        });

        // Test
        await surface.InvokeAsync("setLevel", "7,42", TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(sent);
        Assert.AreEqual(7, sent[4]);
        CollectionAssert.AreEqual(new byte[] { 0x26, 0x01, 42 }, sent[6..9]);
    }

    [TestMethod]
    public async Task InvokeAsync_ConfigurationSet_WritesTheValueBigEndianWithTheSmallestSize()
    {
        // Stage
        var sent = new List<byte[]>();
        var surface = new ZWaveControlSurface((data, _) =>
        {
            sent.Add(data.ToArray());
            return Task.CompletedTask;
        });

        // Test
        await surface.InvokeAsync("configSet", "4,7 300", TestContext.CancellationToken);
        await surface.InvokeAsync("configSet", "4,7 5 4", TestContext.CancellationToken);

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0x70, 0x04, 7, 2, 0x01, 0x2C }, sent[0][6..12]);
        CollectionAssert.AreEqual(new byte[] { 0x70, 0x04, 7, 4, 0, 0, 0, 5 }, sent[1][6..14]);
    }

    [TestMethod]
    public async Task InvokeAsync_ConfigurationGetAndRawPayload_CarryTheirBytes()
    {
        // Stage
        var sent = new List<byte[]>();
        var surface = new ZWaveControlSurface((data, _) =>
        {
            sent.Add(data.ToArray());
            return Task.CompletedTask;
        });

        // Test
        await surface.InvokeAsync("configGet", "3,12", TestContext.CancellationToken);
        await surface.InvokeAsync("sendRaw", "3,26 01 32", TestContext.CancellationToken);

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0x70, 0x05, 12 }, sent[0][6..9]);
        CollectionAssert.AreEqual(new byte[] { 0x26, 0x01, 0x32 }, sent[1][6..9]);
    }

    [TestMethod]
    public async Task InvokeAsync_HealIsFailedAndRemoveFailed_UseTheirFunctions()
    {
        // Stage
        var functions = new List<byte>();
        var surface = new ZWaveControlSurface((data, _) =>
        {
            functions.Add(new ZWaveMessageDecoder().Decode(new ReadOnlySequence<byte>(data.ToArray())).Function);
            return Task.CompletedTask;
        });

        // Test
        foreach (var id in new[] { "heal", "isFailed", "removeFailed", "learnOn", "learnOff", "resetNetwork" })
        {
            await surface.InvokeAsync(id, "6", TestContext.CancellationToken);
        }

        // Assert
        CollectionAssert.AreEqual(
            new[] { ZWaveFunctions.RequestNodeNeighborUpdate, ZWaveFunctions.IsFailedNode, ZWaveFunctions.RemoveFailedNode, ZWaveFunctions.SetLearnMode, ZWaveFunctions.SetLearnMode, ZWaveFunctions.SetDefault },
            functions);
    }

    [TestMethod]
    public void BuildDefinition_DestructiveButtons_AskFirst()
    {
        var buttons = new ZWavePanel().BuildDefinition().Sections.SelectMany(s => s.Controls).OfType<ButtonControl>().ToDictionary(b => b.Id);

        foreach (var id in new[] { "resetNetwork", "removeFailed", "learnOn", "configSet", "softReset" })
        {
            Assert.IsNotNull(buttons[id].ConfirmMessage, id);
        }
    }

    [TestMethod]
    [DataRow("configGet", "2")]
    [DataRow("configGet", "2,x")]
    [DataRow("configSet", "2,5")]
    [DataRow("configSet", "2,5 1 3")]
    [DataRow("sendRaw", "2,ZZ")]
    [DataRow("sendRaw", "2,26")]
    [DataRow("heal", null)]
    [DataRow("switchOn", null)]
    [DataRow("switchOn", "0")]
    [DataRow("switchOn", "233")]
    [DataRow("setLevel", "3,100")]
    [DataRow("setLevel", "3")]
    [DataRow("bogus", null)]
    public async Task InvokeAsync_BadInput_Throws(string command, string? value)
    {
        // Stage
        var surface = new ZWaveControlSurface((_, _) => Task.CompletedTask);

        // Test / Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => surface.InvokeAsync(command, value, TestContext.CancellationToken));
    }

    [TestMethod]
    public void PreviewCommand_UnknownCommand_ReturnsNull()
    {
        var surface = new ZWaveControlSurface((_, _) => Task.CompletedTask);

        Assert.IsNull(surface.PreviewCommand("bogus", null));
    }

    [TestMethod]
    public void Decode_AckThenFrameSplitAcrossReads_DescribesBoth()
    {
        // Stage
        var decoder = new ZWaveDecoder();
        var changes = new List<IReadOnlyDictionary<string, string>>();
        decoder.ValuesChanged += (_, values) => changes.Add(values);
        var frame = ZWaveRequests.MemoryGetId().Raw;

        // Test
        var first = decoder.Decode(new ReadOnlySequence<byte>([ZWaveFrame.AckByte, .. frame[..3]]));
        var second = decoder.Decode(new ReadOnlySequence<byte>(frame[3..]));

        // Assert
        Assert.HasCount(1, first);
        Assert.AreEqual("ACK", first[0]);
        Assert.HasCount(1, second);
        Assert.HasCount(2, changes);
        Assert.AreEqual(second[0], changes[1]["last"]);
    }

    [TestMethod]
    public void Decode_NakCanAndStrayByte_AreReportedWithoutThrowing()
    {
        var decoder = new ZWaveDecoder();

        var lines = decoder.Decode(new ReadOnlySequence<byte>([ZWaveFrame.NakByte, ZWaveFrame.CanByte, 0x7F]));

        Assert.HasCount(3, lines);
        Assert.AreEqual("NAK", lines[0]);
        Assert.AreEqual("CAN", lines[1]);
        StringAssert.StartsWith(lines[2], "INVALID");
    }
}
