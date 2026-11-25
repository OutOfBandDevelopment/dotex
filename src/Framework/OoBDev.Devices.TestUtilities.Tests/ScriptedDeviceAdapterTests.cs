using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.Devices.TestUtilities.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class ScriptedDeviceAdapterTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Write_MatchingRequest_PushesTheScriptedReply()
    {
        // Stage
        await using var adapter = new ScriptedDeviceAdapter().When([0x01, 0x02], [0xAA, 0xBB]);
        await adapter.OpenAsync(TestContext.CancellationToken);

        // Test
        await adapter.Output.WriteAsync(new byte[] { 0x01, 0x02 }, TestContext.CancellationToken);
        var read = await adapter.Input.ReadAsync(TestContext.CancellationToken);

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0xAA, 0xBB }, read.Buffer.ToArray());
        Assert.HasCount(1, adapter.Written);
        Assert.IsEmpty(adapter.Unmatched);
    }

    [TestMethod]
    public async Task Write_UnknownRequest_IsRecordedAsUnmatchedWithNoReply()
    {
        // Stage
        await using var adapter = new ScriptedDeviceAdapter().When([0x01], [0xFF]);
        await adapter.OpenAsync(TestContext.CancellationToken);

        // Test
        await adapter.Output.WriteAsync(new byte[] { 0x09 }, TestContext.CancellationToken);

        // Assert
        Assert.HasCount(1, adapter.Unmatched);
        Assert.IsFalse(adapter.Input.TryRead(out _));
    }

    [TestMethod]
    public async Task Write_RuleWithTwoReplies_PushesBothInOrder()
    {
        // Stage
        await using var adapter = new ScriptedDeviceAdapter().When([0x15], [0x06], [0x01, 0x03]);
        await adapter.OpenAsync(TestContext.CancellationToken);

        // Test
        await adapter.Output.WriteAsync(new byte[] { 0x15 }, TestContext.CancellationToken);
        var read = await adapter.Input.ReadAsync(TestContext.CancellationToken);

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0x06, 0x01, 0x03 }, read.Buffer.ToArray());
    }

    [TestMethod]
    public async Task Push_Unprompted_ReachesTheReader()
    {
        // Stage
        await using var adapter = new ScriptedDeviceAdapter();

        // Test
        await adapter.PushAsync([0x42]);
        var read = await adapter.Input.ReadAsync(TestContext.CancellationToken);

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0x42 }, read.Buffer.ToArray());
    }

    [TestMethod]
    public async Task OpenAndClose_RaiseStateChanges()
    {
        // Stage
        var states = new List<ConnectionState>();
        await using var adapter = new ScriptedDeviceAdapter();
        adapter.StateChanged += (_, e) => states.Add(e.Current);

        // Test
        await adapter.OpenAsync(TestContext.CancellationToken);
        await adapter.CloseAsync(TestContext.CancellationToken);

        // Assert
        CollectionAssert.AreEqual(new[] { ConnectionState.Open, ConnectionState.Closed }, states);
    }
}
