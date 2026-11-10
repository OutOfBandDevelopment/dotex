using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>REQ-011 with a real sandboxed process: shared leases use one process, a bound lease ends the process on dispose.</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class LifetimeTests
{
    private static SandboxedPlugin Start() => new("echo", new JobLimits(MemoryMb: 256));

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
        var time = TimeProvider.System;
        using var pool = new PluginPool<SandboxedPlugin>(time, TimeSpan.Zero);

        // Test
        using var first = pool.Acquire("echo", "g", PluginLifetime.Shared, Start);
        using var second = pool.Acquire("echo", "g", PluginLifetime.Shared, Start);
        var ready = first.Plugin.Receive();
        first.Plugin.Send(FrameType.Request, "echo", """{"n":1}""");
        var viaFirst = first.Plugin.Receive();
        var sequence = second.Plugin.Send(FrameType.Request, "echo", """{"n":2}""");
        var viaSecond = second.Plugin.Receive();

        // Assert
        Assert.AreSame(first.Plugin, second.Plugin);
        Assert.AreEqual("lifecycle.ready", ready.Topic);
        Assert.AreEqual(sequence, viaSecond.Ack);
        Assert.IsNotNull(viaFirst);
    }

    [TestMethod]
    public void Shared_IdleSweep_KillsTheProcess()
    {
        using var pool = new PluginPool<SandboxedPlugin>(TimeProvider.System, TimeSpan.Zero);
        var lease = pool.Acquire("echo", "g", PluginLifetime.Shared, Start);
        var plugin = lease.Plugin;
        plugin.Receive();
        var pid = plugin.ProcessId;

        lease.Dispose();
        Assert.AreEqual(1, pool.Sweep());

        Assert.IsTrue(Gone(pid), "Disposing the idle instance ends the process.");
    }

    [TestMethod]
    public void Bound_Dispose_KillsTheProcess()
    {
        var pool = new PluginPool<SandboxedPlugin>(TimeProvider.System, TimeSpan.FromSeconds(30));
        var lease = pool.Acquire("echo", "g", PluginLifetime.Bound, Start);
        var plugin = lease.Plugin;
        plugin.Receive();
        var pid = plugin.ProcessId;

        lease.Dispose();

        Assert.IsTrue(Gone(pid));
    }
}
