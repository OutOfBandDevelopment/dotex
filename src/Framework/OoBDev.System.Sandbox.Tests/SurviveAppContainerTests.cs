using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Sandbox.Windows;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>REQ-011 Survive inside an AppContainer: the job does not kill the plugin when the host's handle goes, and a host outside the container reaches its pipe.</summary>
[TestCategory("Integration")]
[TestClass]
[SupportedOSPlatform("windows")]
public sealed class SurviveAppContainerTests
{
    private static readonly FrameFlags _flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign;

    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void Start_NoKillOnClose_PluginOutlivesHostHandleAndAnswersOverPipe()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("AppContainer is Windows only.");
        }

        // Stage
        var pipe = $"oobd-survive-ac-{Guid.NewGuid():N}";
        int pid;
        using (var plugin = new SandboxedPlugin(TestPlugin.Dll, $"survive {pipe} 20", new JobLimits(MemoryMb: 256), flags: _flags, killOnClose: false))
        {
            pid = plugin.ProcessId;
            plugin.Detach();
        }

        TestContext.WriteLine($"Plugin pid {pid} in an AppContainer, pipe {pipe}, job handle released");

        // Test
        using var process = Process.GetProcessById(pid);
        var aliveAfterRelease = !process.HasExited;
        using var connection = new SurviveConnection(AppContainerLauncher.PluginPipeName(pipe), RandomNumberGenerator.GetBytes(32), _flags, TimeSpan.FromSeconds(15));
        var hello = connection.Receive();
        connection.Request("echo", """{"n":1}""");
        var echoed = connection.Receive();

        // Assert
        TestContext.WriteLine($"{hello.Topic} {hello.Text} {echoed.Text}");
        Assert.IsTrue(aliveAfterRelease, "Releasing the job handle did not end the plugin.");
        Assert.AreEqual("""{"n":1}""", echoed.Text);
        process.Kill();
    }
}
