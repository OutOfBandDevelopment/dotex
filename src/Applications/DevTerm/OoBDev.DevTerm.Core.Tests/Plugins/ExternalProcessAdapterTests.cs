using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Tests;

/// <summary>A transport plugin written in PowerShell (an inline echo device) driven through <see cref="ExternalProcessAdapter"/>.</summary>
[TestClass]
public sealed class ExternalProcessAdapterTests
{
    private const string EchoDevice = """$i = [Console]::In; while ($l = $i.ReadLine()) { $m = $l | ConvertFrom-Json; if ($m.type -eq 'open') { '{"type":"opened"}' } elseif ($m.type -eq 'write') { '{"type":"data","hex":"' + $m.hex + '"}' } elseif ($m.type -eq 'close') { exit } }""";

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Echo_WriteComesBackAsInput()
    {
        // Stage
        ToolProbe.RequireOrSkip("pwsh", "-NoProfile -Command exit");
        await using var adapter = new ExternalProcessAdapter("echo", "pwsh", ["-NoProfile", "-Command", EchoDevice], TimeSpan.FromSeconds(10));

        // Test
        await adapter.OpenAsync(TestContext.CancellationToken);
        TestContext.WriteLine("State after open: {0}", adapter.State);
        await adapter.Output.WriteAsync(Encoding.ASCII.GetBytes("ping"), TestContext.CancellationToken);
        var read = await adapter.Input.ReadAsync(TestContext.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(ConnectionState.Open, adapter.State);
        Assert.AreEqual("ping", Encoding.ASCII.GetString(read.Buffer.ToArray()));
        Assert.AreEqual("plugin://echo", adapter.Path);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Open_SilentPlugin_FaultsAndThrows()
    {
        ToolProbe.RequireOrSkip("pwsh", "-NoProfile -Command exit");
        await using var adapter = new ExternalProcessAdapter("silent", "pwsh", ["-NoProfile", "-Command", "Start-Sleep 60"], TimeSpan.FromMilliseconds(500));

        await Assert.ThrowsExactlyAsync<global::System.IO.IOException>(() => adapter.OpenAsync(TestContext.CancellationToken));

        Assert.AreEqual(ConnectionState.Faulted, adapter.State);
        StringAssert.Contains(adapter.FaultReason, "no answer");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Open_PluginReportsError_FaultsWithItsMessage()
    {
        ToolProbe.RequireOrSkip("pwsh", "-NoProfile -Command exit");
        const string refuses = """$i = [Console]::In; while ($l = $i.ReadLine()) { if (($l | ConvertFrom-Json).type -eq 'open') { '{"type":"error","message":"no device"}' } }""";
        await using var adapter = new ExternalProcessAdapter("refuses", "pwsh", ["-NoProfile", "-Command", refuses], TimeSpan.FromSeconds(10));

        await Assert.ThrowsExactlyAsync<global::System.IO.IOException>(() => adapter.OpenAsync(TestContext.CancellationToken));

        Assert.AreEqual(ConnectionState.Faulted, adapter.State);
        Assert.AreEqual("no device", adapter.FaultReason);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Open_MissingProgram_Throws()
    {
        await using var adapter = new ExternalProcessAdapter("none", "definitely-not-a-program-xyz", [], TimeSpan.FromSeconds(1));

        await Assert.ThrowsExactlyAsync<global::System.IO.IOException>(() => adapter.OpenAsync(TestContext.CancellationToken));

        Assert.AreEqual(ConnectionState.Faulted, adapter.State);
    }
}
