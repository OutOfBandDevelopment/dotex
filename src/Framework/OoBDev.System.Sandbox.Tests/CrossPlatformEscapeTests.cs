using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>
/// The same escape attempts on whichever sandbox the current OS has (<see cref="SandboxLauncher"/>): AppContainer on Windows, bubblewrap on Linux,
/// Seatbelt on macOS. Each refusal has a positive control. They end Inconclusive when the sandbox is not installed; the macOS run has never
/// been executed because no Mac was available while writing it.
/// </summary>
[TestCategory("Integration")]
[TestClass]
public sealed class CrossPlatformEscapeTests
{
    private static readonly TempFileFactory _temp = new(new ServiceCollection().BuildServiceProvider());

    [TestInitialize]
    public void RequireSandbox()
    {
        if (!SandboxLauncher.IsAvailable)
        {
            Assert.Inconclusive($"{SandboxLauncher.Mechanism} is not available on this machine.");
        }
    }

    [TestMethod]
    public void Echo_InsideTheSandbox_FramesStillWork()
    {
        using var plugin = SandboxLauncher.Start("echo");

        var ready = plugin.Receive();
        var sequence = plugin.Send(FrameType.Request, "echo", """{"n":5}""");
        var response = plugin.Receive();
        plugin.Send(FrameType.Shutdown, "lifecycle.shutdown", "{}");

        Assert.AreEqual("lifecycle.ready", ready.Topic);
        Assert.AreEqual(sequence, response.Ack);
        Assert.IsTrue(plugin.WaitForExit(20_000));
    }

    [TestMethod]
    public void Network_LoopbackConnect_WorksOutsideAndIsRefusedInside()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var target = $"127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}";

        Assert.IsTrue(Probe(new PlainPlugin($"probe net {target}")), "Positive control: an unsandboxed plugin reaches the listener.");
        Assert.IsFalse(Probe(SandboxLauncher.Start($"probe net {target}")), "The sandbox refuses the connection.");
    }

    [TestMethod]
    public void Files_GrantsWork_AndEverythingElseIsRefused()
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

        Assert.IsTrue(Probe(SandboxLauncher.Start($"probe file-read \"{allowedFile}\"", read, write)), "Positive control: a read inside the grant works.");
        Assert.IsTrue(Probe(SandboxLauncher.Start($"probe file-write \"{Path.Combine(writable.DirectoryPath, "out.txt")}\"", read, write)), "Positive control: a write inside the write grant works.");
        Assert.IsTrue(Probe(new PlainPlugin($"probe file-read \"{secretFile}\"")), "Positive control: unsandboxed, the secret is readable.");
        Assert.IsFalse(Probe(SandboxLauncher.Start($"probe file-read \"{secretFile}\"", read, write)), "A read outside every grant is refused.");
        Assert.IsFalse(Probe(SandboxLauncher.Start($"probe file-write \"{Path.Combine(other.DirectoryPath, "planted.txt")}\"", read, write)), "A write outside every grant is refused.");
        Assert.IsFalse(Probe(SandboxLauncher.Start($"probe file-write \"{Path.Combine(granted.DirectoryPath, "planted.txt")}\"", read, write)), "A write into a read-only grant is refused.");
    }

    [TestMethod]
    public void Environment_HostSecret_IsVisibleOutsideAndAbsentInside()
    {
        Environment.SetEnvironmentVariable("OOBDEV_SPIKE_SECRET", "do-not-leak");
        try
        {
            Assert.IsTrue(Probe(new PlainPlugin("probe env OOBDEV_SPIKE_SECRET")), "Positive control: an unsandboxed child inherits the host environment.");
            Assert.IsFalse(Probe(SandboxLauncher.Start("probe env OOBDEV_SPIKE_SECRET")), "The sandbox does not pass the host environment on.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("OOBDEV_SPIKE_SECRET", null);
        }
    }

    private static bool Probe(ISandboxedPlugin plugin)
    {
        using (plugin)
        {
            var frame = plugin.Receive();
            Assert.AreEqual("probe.result", frame.Topic);
            using var document = frame.ParseJson();
            return document.RootElement.GetProperty("allowed").GetBoolean();
        }
    }
}
