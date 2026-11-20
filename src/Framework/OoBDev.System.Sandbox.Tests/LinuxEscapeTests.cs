using OoBDev.System.Sandbox.Linux;
using OoBDev.System.Sandbox.Windows;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>
/// Escape attempts from inside bubblewrap (REQ-008 on Linux). They end Inconclusive anywhere but Linux with bwrap installed; run them in the
/// container described in the spike readme. Each refusal has a positive control that shows the same attempt works outside the sandbox or inside the grant.
/// </summary>
[global::System.Runtime.Versioning.SupportedOSPlatform("linux")]
[TestCategory("Integration")]
[TestClass]
public sealed class LinuxEscapeTests
{
    private static readonly TempFileFactory _temp = new(new ServiceCollection().BuildServiceProvider());

    [TestInitialize]
    public void RequireBubblewrap()
    {
        if (!OperatingSystem.IsLinux() || !File.Exists("/usr/bin/bwrap"))
        {
            Assert.Inconclusive("Needs Linux with bubblewrap (/usr/bin/bwrap).");
        }
    }

    [TestMethod]
    public void Arguments_Default_UnshareEverythingAndMountNothingElse()
    {
        var args = BubblewrapPlugin.Arguments("/app/plugin.dll", "/usr/share/dotnet", "00", "7", [], [], "echo");

        CollectionAssert.Contains(args.ToList(), "--unshare-all");
        CollectionAssert.Contains(args.ToList(), "--clearenv");
        CollectionAssert.Contains(args.ToList(), "--die-with-parent");
        Assert.IsFalse(args.Contains("--share-net"));
        var binds = args.Select((value, index) => (value, index)).Where(x => x.value is "--ro-bind" or "--bind").Select(x => args[x.index + 1]).ToList();
        CollectionAssert.AreEqual(new[] { "/usr", "/app" }, binds, "Only /usr and the assembly folder are bound.");
    }

    [TestMethod]
    public void Echo_InsideBubblewrap_FramesStillWork()
    {
        using var plugin = new BubblewrapPlugin(TestPlugin.Dll, "echo");

        var ready = plugin.Receive();
        var sequence = plugin.Send(FrameType.Request, "echo", """{"n":3}""");
        var response = plugin.Receive();
        plugin.Send(FrameType.Shutdown, "lifecycle.shutdown", "{}");

        Assert.AreEqual("lifecycle.ready", ready.Topic);
        Assert.AreEqual(sequence, response.Ack);
        Assert.IsTrue(plugin.WaitForExit(15_000));
    }

    [TestMethod]
    public void Network_LoopbackConnect_WorksOutsideAndIsRefusedInside()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var target = $"127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}";

        Assert.IsTrue(Probe("net", target, sandboxed: false), "Positive control: an unsandboxed plugin reaches the listener.");
        Assert.IsFalse(Probe("net", target, sandboxed: true), "The network namespace is empty.");
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
        string[] read = [granted.DirectoryPath];
        string[] write = [writable.DirectoryPath];

        Assert.IsTrue(Probe("file-read", allowedFile, true, read, write), "Positive control: a read inside the granted folder works.");
        Assert.IsTrue(Probe("file-write", Path.Combine(writable.DirectoryPath, "out.txt"), true, read, write), "Positive control: a write inside the write grant works.");
        Assert.IsTrue(Probe("file-read", secretFile, false, read, write), "Positive control: unsandboxed, the secret is readable.");
        Assert.IsFalse(Probe("file-read", secretFile, true, read, write), "A read outside every grant is refused.");
        Assert.IsFalse(Probe("file-write", Path.Combine(other.DirectoryPath, "planted.txt"), true, read, write), "A write outside every grant is refused.");
        Assert.IsFalse(Probe("file-write", Path.Combine(granted.DirectoryPath, "planted.txt"), true, read, write), "A write into a read-only grant is refused.");
        Assert.IsFalse(File.Exists(Path.Combine(other.DirectoryPath, "planted.txt")));
        Assert.IsFalse(File.Exists(Path.Combine(granted.DirectoryPath, "planted.txt")));
    }

    [TestMethod]
    public void Environment_HostSecret_IsVisibleOutsideAndAbsentInside()
    {
        Environment.SetEnvironmentVariable("OOBDEV_SPIKE_SECRET", "do-not-leak");
        try
        {
            Assert.IsTrue(Probe("env", "OOBDEV_SPIKE_SECRET", sandboxed: false), "Positive control: an unsandboxed child inherits the host environment.");
            Assert.IsFalse(Probe("env", "OOBDEV_SPIKE_SECRET", sandboxed: true), "The sandbox starts with a cleared environment.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("OOBDEV_SPIKE_SECRET", null);
        }
    }

    [TestMethod]
    public void Processes_HostProcesses_AreVisibleOutsideAndHiddenInside()
    {
        Assert.IsTrue(Probe("pids", "5", sandboxed: false), "Positive control: the host sees many processes.");
        Assert.IsFalse(Probe("pids", "5", sandboxed: true), "The PID namespace shows only the plugin's own processes.");
    }

    private static bool Probe(string kind, string target, bool sandboxed, string[]? read = null, string[]? write = null)
    {
        using ISandboxedPlugin plugin = sandboxed ? new BubblewrapPlugin(TestPlugin.Dll, $"probe {kind} {target}", read, write) : new PlainPlugin(TestPlugin.Dll, $"probe {kind} {target}");
        var frame = plugin.Receive();
        Assert.AreEqual("probe.result", frame.Topic);
        using var document = frame.ParseJson();
        return document.RootElement.GetProperty("allowed").GetBoolean();
    }
}
