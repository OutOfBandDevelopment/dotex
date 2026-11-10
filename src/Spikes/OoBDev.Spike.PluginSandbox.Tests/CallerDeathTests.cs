using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>REQ-011 caller death with a real process tree: a stand-in host starts a sandboxed plugin and is then killed.</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class CallerDeathTests
{
    [TestInitialize]
    public void RequireWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Windows launcher (AppContainer and job object); see CrossPlatformEscapeTests for Linux and macOS.");
        }
    }

    private static string Dll => typeof(SandboxedPlugin).Assembly.Location;

    private static bool Exists(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            return !process.WaitForExit(0);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool WaitGone(int pid) => SpinWait.SpinUntil(() => !Exists(pid), TimeSpan.FromSeconds(10));

    private static (Process Caller, int PluginPid) StartCaller(string mode)
    {
        var file = Path.Combine(Path.GetTempPath(), $"caller-{Guid.NewGuid():N}.pid");
        var caller = Process.Start(new ProcessStartInfo("dotnet", $"\"{Dll}\" caller {mode} \"{file}\"") { UseShellExecute = false, CreateNoWindow = true })!;
        Assert.IsTrue(SpinWait.SpinUntil(() => File.Exists(file) && new FileInfo(file).Length > 0, TimeSpan.FromSeconds(30)), "The stand-in host started the plugin.");
        Thread.Sleep(200);
        var pid = int.Parse(File.ReadAllText(file), global::System.Globalization.CultureInfo.InvariantCulture);
        File.Delete(file);
        return (caller, pid);
    }

    [TestMethod]
    public void Kill_CallerDies_PluginDiesWithIt()
    {
        var (caller, pluginPid) = StartCaller("kill");
        Assert.IsTrue(Exists(pluginPid), "The plugin runs while its caller lives.");

        caller.Kill();
        caller.WaitForExit();

        Assert.IsTrue(WaitGone(pluginPid), "Kill-on-close ends the plugin when the caller dies.");
    }

    [TestMethod]
    public void Survive_CallerDies_PluginKeepsRunning()
    {
        var (caller, pluginPid) = StartCaller("survive");
        try
        {
            caller.Kill();
            caller.WaitForExit();
            Thread.Sleep(1_500);

            Assert.IsTrue(Exists(pluginPid), "Without kill-on-close the plugin outlives its caller.");
        }
        finally
        {
            using var plugin = Process.GetProcessById(pluginPid);
            plugin.Kill(entireProcessTree: true);
        }
    }
}
