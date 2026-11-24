using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>REQ-011 caller death on Linux: bubblewrap's die-with-parent ends the sandbox when the stand-in host is killed.</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class LinuxCallerDeathTests
{
    public required TestContext TestContext { get; set; }

    private static bool Exists(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    [TestMethod]
    public void Kill_CallerDies_BubblewrapPluginDiesWithIt()
    {
        // Stage
        if (!OperatingSystem.IsLinux())
        {
            Assert.Inconclusive("Linux launcher (bubblewrap); see CallerDeathTests for Windows.");
        }

        var file = Path.Combine(Path.GetTempPath(), $"caller-{Guid.NewGuid():N}.pid");
        using var caller = Process.Start(new ProcessStartInfo("dotnet", $"\"{TestPlugin.Dll}\" caller kill \"{file}\"") { UseShellExecute = false })!;
        Assert.IsTrue(SpinWait.SpinUntil(() => File.Exists(file) && new FileInfo(file).Length > 0, TimeSpan.FromSeconds(30)), "The stand-in host started the plugin.");
        Thread.Sleep(200);
        var pid = int.Parse(File.ReadAllText(file), global::System.Globalization.CultureInfo.InvariantCulture);
        File.Delete(file);
        TestContext.WriteLine($"Caller {caller.Id}, bubblewrap process {pid}");
        Assert.IsTrue(Exists(pid), "The plugin runs while its caller lives.");

        // Test
        caller.Kill();
        caller.WaitForExit();

        // Assert
        var gone = SpinWait.SpinUntil(() => !Exists(pid), TimeSpan.FromSeconds(10));
        TestContext.WriteLine($"Plugin gone: {gone}");
        Assert.IsTrue(gone, "die-with-parent ends the plugin when the caller dies.");
    }
}
