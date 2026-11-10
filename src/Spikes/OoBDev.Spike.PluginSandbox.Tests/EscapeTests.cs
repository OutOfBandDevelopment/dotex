using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>
/// Escape attempts from inside the AppContainer (REQ-001, REQ-005, REQ-007, REQ-008 on Windows). Each refusal has a positive control that
/// shows the same attempt works outside the sandbox or inside the grant, so a refusal means the sandbox did it.
/// </summary>
[TestCategory("Integration")]
[TestClass]
public sealed class EscapeTests
{
    private const string _secretName = "OOBDEV_SPIKE_SECRET";
    private static readonly string _whoami = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "whoami.exe");
    private static readonly TempFileFactory _temp = new(new ServiceCollection().BuildServiceProvider());

    [TestMethod]
    public void Echo_InsideTheContainer_FramesStillWork()
    {
        using var plugin = new SandboxedPlugin("echo", new JobLimits(MemoryMb: 256));

        var ready = plugin.Receive();
        var sequence = plugin.Send(FrameType.Request, "echo", """{"n":3}""");
        var response = plugin.Receive();
        plugin.Send(FrameType.Shutdown, "lifecycle.shutdown", "{}");

        Assert.AreEqual("lifecycle.ready", ready.Topic);
        Assert.AreEqual(sequence, response.Ack);
        Assert.IsTrue(plugin.WaitForExit(15_000));
        Assert.AreEqual(LimitHit.None, plugin.LimitHit);
    }

    [TestMethod]
    public void Network_LoopbackConnect_WorksOutsideAndIsRefusedInside()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var target = $"127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}";

        Assert.IsTrue(Outside("net", target), "Positive control: an unsandboxed plugin reaches the listener.");
        Assert.IsFalse(Inside("net", target, new JobLimits(MemoryMb: 256)), "The AppContainer has no network capability.");
    }

    [TestMethod]
    public void Files_ReadAndWrite_WorkInsideTheGrantAndAreRefusedOutsideIt()
    {
        using var granted = _temp.GetTempDirectory();
        using var writable = _temp.GetTempDirectory();
        using var other = _temp.GetTempDirectory();
        var allowedFile = Path.Combine(granted.DirectoryPath, "in.txt");
        var secretFile = Path.Combine(other.DirectoryPath, "secret.txt");
        File.WriteAllText(allowedFile, "ok");
        File.WriteAllText(secretFile, "secret");
        var limits = new JobLimits(MemoryMb: 256);
        string[] read = [granted.DirectoryPath];
        string[] write = [writable.DirectoryPath];

        Assert.IsTrue(Inside("file-read", allowedFile, limits, read, write), "Positive control: a read inside the granted folder works.");
        Assert.IsTrue(Inside("file-write", Path.Combine(writable.DirectoryPath, "out.txt"), limits, read, write), "Positive control: a write inside the write grant works.");
        Assert.IsFalse(Inside("file-read", secretFile, limits, read, write), "A read outside every grant is refused.");
        Assert.IsFalse(Inside("file-write", Path.Combine(other.DirectoryPath, "planted.txt"), limits, read, write), "A write outside every grant is refused.");
        Assert.IsFalse(Inside("file-write", Path.Combine(granted.DirectoryPath, "planted.txt"), limits, read, write), "A write into a read-only grant is refused.");
        Assert.IsFalse(File.Exists(Path.Combine(other.DirectoryPath, "planted.txt")));
    }

    [TestMethod]
    public void Environment_HostSecret_IsVisibleOutsideAndAbsentInside()
    {
        Environment.SetEnvironmentVariable(_secretName, "do-not-leak");
        try
        {
            Assert.IsTrue(Outside("env", _secretName), "Positive control: an unsandboxed child inherits the host environment.");
            Assert.IsFalse(Inside("env", _secretName, new JobLimits(MemoryMb: 256)), "The container gets an explicit environment only.");
        }
        finally
        {
            Environment.SetEnvironmentVariable(_secretName, null);
        }
    }

    [TestMethod]
    public void Spawn_ChildProcess_IsRefusedWithAProcessLimitOfOne()
    {
        Assert.IsTrue(Inside("spawn", _whoami, new JobLimits(MemoryMb: 256)), "Positive control: without a process cap the plugin can start a child.");
        Assert.IsFalse(Inside("spawn", _whoami, new JobLimits(MemoryMb: 256, MaxProcesses: 1)), "ActiveProcessLimit 1 refuses a child.");
    }

    [TestMethod]
    public void Limits_Memory_TheHostReportsMemory()
    {
        using var plugin = new SandboxedPlugin("hog", new JobLimits(MemoryMb: 256));

        Assert.IsTrue(plugin.WaitForExit(60_000));

        Assert.AreNotEqual(0, plugin.ExitCode);
        Assert.AreEqual(LimitHit.Memory, plugin.LimitHit, $"peak {plugin.PeakMemoryBytes}");
    }

    [TestMethod]
    public void Limits_CpuTime_TheHostStopsAndReportsCpu()
    {
        using var plugin = new SandboxedPlugin("spin", new JobLimits(MemoryMb: 256, CpuSeconds: 2));

        Assert.IsTrue(plugin.WaitForExit(60_000));

        Assert.AreEqual(LimitHit.Cpu, plugin.LimitHit);
    }

    [TestMethod]
    public void Limits_WallTime_TheHostStopsAndReportsWall()
    {
        using var plugin = new SandboxedPlugin("sleep", new JobLimits(MemoryMb: 256, WallSeconds: 2));

        Assert.IsTrue(plugin.WaitForExit(60_000));

        Assert.AreEqual(LimitHit.Wall, plugin.LimitHit);
    }

    [TestMethod]
    public void Limits_PluginWithinItsLimits_ReportsNone()
    {
        using var plugin = new SandboxedPlugin("echo", new JobLimits(MemoryMb: 256, CpuSeconds: 30, WallSeconds: 60));

        plugin.Receive();
        plugin.Send(FrameType.Shutdown, "lifecycle.shutdown", "{}");

        Assert.IsTrue(plugin.WaitForExit(15_000));
        Assert.AreEqual(LimitHit.None, plugin.LimitHit);
    }

    private static bool Outside(string kind, string target)
    {
        using var plugin = new PluginProcess($"probe {kind} \"{target}\"", 256);
        return Allowed(plugin.Receive());
    }

    private static bool Inside(string kind, string target, JobLimits limits, string[]? read = null, string[]? write = null)
    {
        using var plugin = new SandboxedPlugin($"probe {kind} \"{target}\"", limits, read, write);
        return Allowed(plugin.Receive());
    }

    private static bool Allowed(ReceivedFrame frame)
    {
        Assert.AreEqual("probe.result", frame.Topic);
        using var document = frame.ParseJson();
        return document.RootElement.GetProperty("allowed").GetBoolean();
    }
}
