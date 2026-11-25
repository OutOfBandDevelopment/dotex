using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Sandbox.Linux;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>REQ-011 Survive on Linux: without die-with-parent the sandbox outlives the host's handle, and the host reaches the plugin through a socket in a granted folder.</summary>
[TestCategory("Integration")]
[TestClass]
[SupportedOSPlatform("linux")]
public sealed class LinuxSurviveTests
{
    private static readonly FrameFlags _flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign;

    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void Start_SurviveCaller_PluginOutlivesDetachAndAnswersOverSocket()
    {
        if (!OperatingSystem.IsLinux() || !new LinuxSandboxLauncher(TestPlugin.Dll).IsAvailable)
        {
            Assert.Inconclusive("Needs Linux with bubblewrap; see scripts/spikes/run-sandbox-linux.sh.");
        }

        // Stage
        var folder = Directory.CreateTempSubdirectory("oobd-survive-").FullName;
        var socket = Path.Combine(folder, "plugin.sock");
        var plugin = new BubblewrapPlugin(TestPlugin.Dll, $"survive {socket} 20", writeDirectories: [folder], flags: _flags, surviveCaller: true);
        var pid = plugin.ProcessId;

        // Test
        plugin.Detach();
        using var process = Process.GetProcessById(pid);
        var alive = !process.HasExited;
        using var connection = new SurviveConnection(socket, RandomNumberGenerator.GetBytes(32), _flags, TimeSpan.FromSeconds(15));
        var hello = connection.Receive();
        connection.Request("echo", """{"n":1}""");
        var echoed = connection.Receive();

        // Assert
        TestContext.WriteLine($"Plugin pid {pid} alive after Detach: {alive}; {hello.Topic} {hello.Text} {echoed.Text}");
        process.Kill();
        Directory.Delete(folder, recursive: true);
        Assert.IsTrue(alive);
        Assert.AreEqual("""{"n":1}""", echoed.Text);
    }
}
