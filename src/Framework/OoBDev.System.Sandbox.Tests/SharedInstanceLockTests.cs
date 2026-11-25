using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>The named mutex of a shared plugin instance (REQ-011), including across processes.</summary>
[TestClass]
public sealed class SharedInstanceLockTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void Name_DifferentGrants_GiveDifferentNames()
    {
        Assert.AreNotEqual(SharedInstanceLock.Name("telemetry", "grants-a"), SharedInstanceLock.Name("telemetry", "grants-b"));
        Assert.AreEqual(SharedInstanceLock.Name("telemetry", "grants-a"), SharedInstanceLock.Name("telemetry", "grants-a"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Name_Singleton_IsMachineWideAndIgnoresTheGrants()
    {
        var name = SharedInstanceLock.Name("device", "grants-a", singleton: true);

        Assert.StartsWith("Global\\", name, StringComparison.Ordinal);
        Assert.AreEqual(name, SharedInstanceLock.Name("device", "grants-b", singleton: true));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Name_PathCharactersInId_AreRefused()
    {
        Assert.ThrowsExactly<ArgumentException>(() => SharedInstanceLock.Name("..\\evil", "g"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryEnter_FreeName_Succeeds()
    {
        var name = SharedInstanceLock.Name("t-" + Guid.NewGuid().ToString("N"), "g");

        using var entry = SharedInstanceLock.TryEnter(name, TimeSpan.Zero);

        Assert.IsNotNull(entry);
        Assert.IsFalse(entry.Abandoned);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void TryEnter_HeldByAnotherProcess_FailsThenReportsAbandonedAfterItDies()
    {
        var name = SharedInstanceLock.Name("t-" + Guid.NewGuid().ToString("N"), "g");
        var file = Path.Combine(Path.GetTempPath(), $"mutex-{Guid.NewGuid():N}.held");
        var dll = TestPlugin.Dll;
        using var holder = Process.Start(new ProcessStartInfo("dotnet", $"\"{dll}\" holdmutex \"{name}\" \"{file}\"") { UseShellExecute = false, CreateNoWindow = true })!;
        Assert.IsTrue(SpinWait.SpinUntil(() => File.Exists(file) && new FileInfo(file).Length > 0, TimeSpan.FromSeconds(30)), "The other process holds the mutex.");

        using (var blocked = SharedInstanceLock.TryEnter(name, TimeSpan.FromMilliseconds(200)))
        {
            Assert.IsNull(blocked, "A second caller cannot enter while another process holds the lock.");
        }

        // The kernel object lives only while a handle is open: a joined caller keeps its handle for its whole life, or the abandonment is lost.
        using var joinedCallerHandle = new global::System.Threading.Mutex(false, name);
        holder.Kill();
        holder.WaitForExit();
        using var next = SharedInstanceLock.TryEnter(name, TimeSpan.FromSeconds(5));

        Assert.IsNotNull(next);
        Assert.IsTrue(next.Abandoned, "A dead holder leaves the lock abandoned, so the plugin is restarted.");
        File.Delete(file);
    }
}
