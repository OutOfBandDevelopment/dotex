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
            Assert.IsNotNull(surface.PreviewCommand(id, "2,50"), id);
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
