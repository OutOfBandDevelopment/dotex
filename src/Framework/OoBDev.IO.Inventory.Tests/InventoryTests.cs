using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.IO.Inventory.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class InventoryTests
{
    public TestContext TestContext { get; set; } = null!;

    private static ITempDirectory NewRoot()
    {
        var temp = new TempFileFactory(null!).GetTempDirectory();
        Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "a"));
        Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "b"));
        return temp;
    }

    private static async Task<List<DuplicateGroup>> GroupsAsync(IInventoryStore store)
    {
        var list = new List<DuplicateGroup>();
        await foreach (var g in new DuplicateFinder().FindAsync(store, CancellationToken.None)) list.Add(g);
        return list;
    }

    [TestMethod]
    public async Task Find_IdenticalFilesInTwoFolders_OneGroupAndFilesUntouched()
    {
        // Stage
        using var root = NewRoot();
        var x = Path.Combine(root.DirectoryPath, "a", "x.txt");
        var y = Path.Combine(root.DirectoryPath, "b", "y.txt");
        File.WriteAllText(x, "same content");
        File.WriteAllText(y, "same content");
        File.WriteAllText(Path.Combine(root.DirectoryPath, "b", "z.txt"), "other content");
        var store = new InMemoryInventoryStore();

        // Test
        var report = await new FileScanner(store).ScanAsync(root.DirectoryPath, false, CancellationToken.None);
        var groups = await GroupsAsync(store);
        TestContext.WriteLine($"scanned {report.Scanned}, hashed {report.Hashed}, groups {groups.Count}");

        // Assert
        Assert.AreEqual(1, groups.Count);
        Assert.AreEqual(2, groups[0].Paths.Count);
        Assert.IsTrue(File.Exists(x) && File.Exists(y));
    }

    [TestMethod]
    public async Task Scan_Twice_UnchangedTreeRehashesNothing()
    {
        using var root = NewRoot();
        File.WriteAllText(Path.Combine(root.DirectoryPath, "a", "x.txt"), "one");
        var scanner = new FileScanner(new InMemoryInventoryStore());

        var first = await scanner.ScanAsync(root.DirectoryPath, false, CancellationToken.None);
        var second = await scanner.ScanAsync(root.DirectoryPath, false, CancellationToken.None);
        TestContext.WriteLine($"first hashed {first.Hashed}, second hashed {second.Hashed}");

        Assert.AreEqual(1, first.Hashed);
        Assert.AreEqual(1, second.Scanned);
        Assert.AreEqual(0, second.Hashed);
    }

    [TestMethod]
    public async Task Scan_ReadOnlyFiles_CompletesAndLeavesThemReadOnly()
    {
        using var root = NewRoot();
        var file = Path.Combine(root.DirectoryPath, "a", "x.txt");
        File.WriteAllText(file, "locked");
        File.SetAttributes(file, FileAttributes.ReadOnly);
        try
        {
            var report = await new FileScanner(new InMemoryInventoryStore()).ScanAsync(root.DirectoryPath, false, CancellationToken.None);
            TestContext.WriteLine($"scanned {report.Scanned}, hashed {report.Hashed}, skipped {report.Skipped.Count}");

            Assert.AreEqual(1, report.Hashed);
            Assert.AreEqual(0, report.Skipped.Count);
            Assert.IsTrue(File.GetAttributes(file).HasFlag(FileAttributes.ReadOnly));
        }
        finally
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }
    }

    [TestMethod]
    public async Task Scan_ChangedFile_IsRehashed()
    {
        using var root = NewRoot();
        var file = Path.Combine(root.DirectoryPath, "a", "x.txt");
        File.WriteAllText(file, "one");
        var scanner = new FileScanner(new InMemoryInventoryStore());
        await scanner.ScanAsync(root.DirectoryPath, false, CancellationToken.None);
        File.WriteAllText(file, "two!");

        var second = await scanner.ScanAsync(root.DirectoryPath, false, CancellationToken.None);
        TestContext.WriteLine($"second hashed {second.Hashed}");

        Assert.AreEqual(1, second.Hashed);
    }

    [TestMethod]
    public async Task Find_FastHashCollision_NotGroupedAfterSha512()
    {
        using var root = NewRoot();
        var a = Path.Combine(root.DirectoryPath, "a", "x.bin");
        var b = Path.Combine(root.DirectoryPath, "b", "y.bin");
        File.WriteAllText(a, "AAAA");
        File.WriteAllText(b, "BBBB");
        var store = new InMemoryInventoryStore();
        await store.SaveAsync(new InventoryEntry(a, 4, DateTimeOffset.UtcNow, "COLLIDE"), CancellationToken.None);
        await store.SaveAsync(new InventoryEntry(b, 4, DateTimeOffset.UtcNow, "COLLIDE"), CancellationToken.None);

        var groups = await GroupsAsync(store);
        TestContext.WriteLine($"groups {groups.Count}");

        Assert.AreEqual(0, groups.Count);
    }
}
