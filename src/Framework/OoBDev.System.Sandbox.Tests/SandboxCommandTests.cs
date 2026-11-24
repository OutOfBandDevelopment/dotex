using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Sandbox.Linux;
using OoBDev.System.Sandbox.Windows;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>Arbitrary-command mode of the launchers (what DevTerm process plugins use).</summary>
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[TestClass]
public sealed class SandboxCommandTests
{
    private static readonly string _cmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public void Quote_PlainSpacedAndQuotedArguments_FollowWindowsRules()
    {
        // Stage and Test
        var plain = SandboxedCommand.Quote("abc");
        var spaced = SandboxedCommand.Quote(@"C:\a b\c");
        var quoted = SandboxedCommand.Quote("say \"hi\"");
        var trailing = SandboxedCommand.Quote(@"C:\a b\");
        var empty = SandboxedCommand.Quote("");
        TestContext.WriteLine($"{plain} | {spaced} | {quoted} | {trailing} | {empty}");

        // Assert
        Assert.AreEqual("abc", plain);
        Assert.AreEqual("\"C:\\a b\\c\"", spaced);
        Assert.AreEqual("\"say \\\"hi\\\"\"", quoted);
        Assert.AreEqual("\"C:\\a b\\\\\"", trailing);
        Assert.AreEqual("\"\"", empty);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Arguments_Command_UnshareEverythingAndBindOnlyTheGrants()
    {
        // Test
        var args = BubblewrapProcess.Arguments("/usr/bin/python3", ["/plugins/p/main.py", "--x"], ["/plugins/p"], ["/data/out"]).ToList();
        TestContext.WriteLine(string.Join(' ', args));

        // Assert
        CollectionAssert.Contains(args, "--unshare-all");
        CollectionAssert.Contains(args, "--clearenv");
        CollectionAssert.Contains(args, "--die-with-parent");
        Assert.IsFalse(args.Contains("--share-net"));
        var binds = args.Select((value, index) => (value, index)).Where(x => x.value is "--ro-bind" or "--bind").Select(x => args[x.index + 1]).ToList();
        CollectionAssert.AreEqual(new[] { "/usr", Path.GetFullPath("/plugins/p"), Path.GetFullPath("/data/out") }, binds);
        var separator = args.IndexOf("--");
        CollectionAssert.AreEqual(new[] { "/usr/bin/python3", "/plugins/p/main.py", "--x" }, args.Skip(separator + 1).ToArray());
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void StartCommand_Windows_ProgramRunsInsideTheContainerWithPlainStreams()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Windows launcher (AppContainer).");
        }

        // Test
        using var process = new SandboxedCommand(_cmd, ["/c", "echo", "sandboxed"], new JobLimits(MemoryMb: 256));
        var output = new StreamReader(process.Output).ReadToEnd();
        var exited = process.WaitForExit(15_000);
        TestContext.WriteLine($"exited={exited} code={process.ExitCode} output={output.Trim()}");

        // Assert
        Assert.IsTrue(exited);
        Assert.AreEqual(0, process.ExitCode);
        StringAssert.Contains(output, "sandboxed");
        Assert.AreEqual(LimitHit.None, process.LimitHit);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void StartCommand_Windows_WallTimeEndsAProgramThatWaitsForInput()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Windows launcher (AppContainer).");
        }

        // Test
        using var process = new SandboxedCommand(_cmd, ["/c", "pause"], new JobLimits(MemoryMb: 256, WallSeconds: 1));
        var exited = process.WaitForExit(15_000);
        TestContext.WriteLine($"exited={exited} hit={process.LimitHit}");

        // Assert
        Assert.IsTrue(exited);
        Assert.AreEqual(LimitHit.Wall, process.LimitHit);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void StartCommand_Linux_ProgramRunsUnderBubblewrap()
    {
        if (!OperatingSystem.IsLinux() || !File.Exists("/usr/bin/bwrap"))
        {
            Assert.Inconclusive("Needs Linux with bubblewrap (/usr/bin/bwrap).");
        }

        // Test
        using var process = new BubblewrapProcess("/bin/echo", ["sandboxed"]);
        var output = new StreamReader(process.Output).ReadToEnd();
        var exited = process.WaitForExit(15_000);
        TestContext.WriteLine($"exited={exited} code={process.ExitCode} output={output.Trim()}");

        // Assert
        Assert.IsTrue(exited);
        StringAssert.Contains(output, "sandboxed");
    }
}
