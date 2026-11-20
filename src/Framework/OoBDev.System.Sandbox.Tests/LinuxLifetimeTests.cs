using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Sandbox.Linux;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>REQ-011 with a real bubblewrap process: shared leases use one process, a bound lease or an idle sweep ends it.</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class LinuxLifetimeTests
{
    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void RequireLinux()
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Inconclusive("Linux launcher (bubblewrap); see LifetimeTests for Windows.");
        }
    }

    private static BubblewrapPlugin Start() => new(TestPlugin.Dll, "echo");

    private static bool Gone(int pid)
    {
        try
        {
            using var process = global::System.Diagnostics.Process.GetProcessById(pid);
            return process.WaitForExit(5_000);
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    [TestMethod]
    public void Shared_TwoLeases_TalkToTheSameProcess()
    {
        // Stage
        using var pool = new PluginPool<BubblewrapPlugin>(TimeProvider.System, TimeSpan.Zero);

        // Test
        using var first = pool.Acquire("echo", "g", PluginLifetime.Shared, Start);
        using var second = pool.Acquire("echo", "g", PluginLifetime.Shared, Start);
        var ready = first.Plugin.Receive();
        var sequence = second.Plugin.Send(FrameType.Request, "echo", """{"n":2}""");
        var reply = second.Plugin.Receive();

        // Assert
        TestContext.WriteLine($"Ready topic {ready.Topic}, reply ack {reply.Ack}");
        Assert.AreSame(first.Plugin, second.Plugin);
        Assert.AreEqual("lifecycle.ready", ready.Topic);
        Assert.AreEqual(sequence, reply.Ack);
    }

    [TestMethod]
    public void Shared_IdleSweep_KillsTheProcess()
    {
        // Stage
        using var pool = new PluginPool<BubblewrapPlugin>(TimeProvider.System, TimeSpan.Zero);
        var lease = pool.Acquire("echo", "g", PluginLifetime.Shared, Start);
        lease.Plugin.Receive();
        var pid = lease.Plugin.ProcessId;

        // Test
        lease.Dispose();
        var swept = pool.Sweep();

        // Assert
        Assert.AreEqual(1, swept);
        Assert.IsTrue(Gone(pid), "Disposing the idle instance ends the process.");
    }

    [TestMethod]
    public void Bound_Dispose_KillsTheProcess()
    {
        // Stage
        var pool = new PluginPool<BubblewrapPlugin>(TimeProvider.System, TimeSpan.FromSeconds(30));
        var lease = pool.Acquire("echo", "g", PluginLifetime.Bound, Start);
        lease.Plugin.Receive();
        var pid = lease.Plugin.ProcessId;

        // Test
        lease.Dispose();

        // Assert
        Assert.IsTrue(Gone(pid));
    }
}
