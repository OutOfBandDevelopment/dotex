using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Sandbox.Linux;
using OoBDev.System.Sandbox.MacOS;
using OoBDev.System.Sandbox.Windows;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>The <c>surviveCaller</c> option of <see cref="ISandboxLauncher.Start"/>.</summary>
[TestClass]
public sealed class LauncherSurviveTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public void Start_SurviveOnLinuxAndMacOS_IsRefused()
    {
        // Test
        var linux = Assert.ThrowsExactly<NotSupportedException>(() => new LinuxSandboxLauncher("x.dll").Start("echo", surviveCaller: true));
        var mac = Assert.ThrowsExactly<NotSupportedException>(() => new MacOSSandboxLauncher("x.dll").Start("echo", surviveCaller: true));

        // Assert
        TestContext.WriteLine($"{linux.Message} | {mac.Message}");
    }

    [TestMethod]
    [TestCategory("Integration")]
    [SupportedOSPlatform("windows")]
    public void Start_SurviveOnWindows_PluginKeepsRunningAfterDetach()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("AppContainer is Windows only.");
        }

        // Stage
        ISandboxLauncher launcher = new WindowsSandboxLauncher(TestPlugin.Dll);
        var plugin = launcher.Start($"survive oobd-launch-{Guid.NewGuid():N} 20", surviveCaller: true);
        var pid = ((SandboxedPlugin)plugin).ProcessId;

        // Test
        plugin.Detach();
        using var process = Process.GetProcessById(pid);
        var alive = !process.HasExited;
        process.Kill();

        // Assert
        TestContext.WriteLine($"Plugin pid {pid} alive after Detach: {alive}");
        Assert.IsTrue(alive);
    }
}
