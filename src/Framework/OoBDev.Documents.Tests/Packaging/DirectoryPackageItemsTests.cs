using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Tests.Packaging;

[TestClass]
public class DirectoryPackageItemsTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ReadAsync_NestedFolder_NamesEntriesRelativeWithForwardSlashes()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        Directory.CreateDirectory(Path.Combine(folder.DirectoryPath, "sub"));
        await File.WriteAllTextAsync(Path.Combine(folder.DirectoryPath, "b.txt"), "beta", TestContext.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(folder.DirectoryPath, "sub", "a.txt"), "alpha", TestContext.CancellationToken);

        // Test
        var items = await DirectoryPackageItems.ReadAsync(folder.DirectoryPath, TestContext.CancellationToken);
        TestContext.WriteLine("Entries: {0}", string.Join(", ", items.Select(i => i.EntryName)));

        // Assert
        CollectionAssert.AreEqual(new[] { "b.txt", "sub/a.txt" }, items.Select(i => i.EntryName).ToArray());
        Assert.AreEqual(5, items.Last().Content.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ReadAsync_MissingFolder_Throws()
    {
        await Assert.ThrowsExactlyAsync<DirectoryNotFoundException>(() => DirectoryPackageItems.ReadAsync(Path.Combine(Path.GetTempPath(), "no-such-folder-xyz"), TestContext.CancellationToken));
    }
}
