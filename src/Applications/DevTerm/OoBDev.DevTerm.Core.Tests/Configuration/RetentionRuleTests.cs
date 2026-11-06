using System;
using System.Collections.Generic;
using System.IO;
using OoBDev.System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;

using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Configuration.Discovery;
namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class RetentionRuleTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    private readonly List<TempDirectoryHandle> _directories = [];

    [TestCleanup]
    public void DeleteTempDirectories()
    {
        foreach (var handle in _directories)
        {
            handle.Dispose();
        }
    }

    private string CreateTempDirectory()
    {
        var handle = new TempDirectoryHandle();
        _directories.Add(handle);
        return handle.DirectoryPath;
    }

    private string Folder(params (string Name, int AgeDays)[] files)
    {
        var dir = CreateTempDirectory();
        foreach (var (name, age) in files)
        {
            var path = Path.Combine(dir, name);
            File.WriteAllText(path, "x");
            File.SetLastWriteTimeUtc(path, _now.UtcDateTime.AddDays(-age));
        }

        return dir;
    }

    [TestMethod]
    public void NoLimits_KeepsEverything()
    {
        var dir = Folder(("a.jsonl", 900), ("b.jsonl", 1));

        Assert.AreEqual(0, new RetentionRule().Apply(dir, "*", _now).Count);
        Assert.AreEqual(2, Directory.GetFiles(dir).Length);
    }

    [TestMethod]
    public void MaxAge_RemovesOnlyOlderFiles()
    {
        var dir = Folder(("old.jsonl", 40), ("new.jsonl", 5));

        var removed = new RetentionRule { MaxAgeDays = 30 }.Apply(dir, "*", _now);

        Assert.AreEqual(1, removed.Count);
        Assert.IsTrue(File.Exists(Path.Combine(dir, "new.jsonl")));
    }

    [TestMethod]
    public void MaxFiles_KeepsTheNewest()
    {
        var dir = Folder(("a.jsonl", 3), ("b.jsonl", 2), ("c.jsonl", 1));

        new RetentionRule { MaxFiles = 2 }.Apply(dir, "*", _now);

        CollectionAssert.AreEquivalent(new[] { "b.jsonl", "c.jsonl" }, Directory.GetFiles(dir).Select(Path.GetFileName).ToArray());
    }

    [TestMethod]
    public void Pattern_LeavesOtherFilesAlone()
    {
        var dir = Folder(("a.jsonl", 100), ("notes.txt", 100));

        new RetentionRule { MaxAgeDays = 1 }.Apply(dir, "*.jsonl", _now);

        Assert.IsTrue(File.Exists(Path.Combine(dir, "notes.txt")));
    }

    [TestMethod]
    public void MissingFolder_IsANoOp() =>
        Assert.AreEqual(0, new RetentionRule { MaxFiles = 1 }.Apply(Path.Combine(CreateTempDirectory(), "none"), "*", _now).Count);

    [TestMethod]
    public void Sweeper_AppliesLogAndExportRulesToTheirOwnFolders()
    {
        var logs = Folder(("a.jsonl", 90), ("b.jsonl", 1));
        var exports = Folder(("x.png", 90), ("y.png", 1));

        var removed = RetentionSweeper.Sweep(
            new AppPreferences { LogRetention = new RetentionRule { MaxAgeDays = 30 }, ExportRetention = new RetentionRule { MaxFiles = 1 } },
            new Microsoft.Extensions.Time.Testing.FakeTimeProvider(_now), logs, exports);

        Assert.AreEqual(2, removed.Count);
        Assert.IsTrue(File.Exists(Path.Combine(logs, "b.jsonl")));
        Assert.IsTrue(File.Exists(Path.Combine(exports, "y.png")));
    }
}
