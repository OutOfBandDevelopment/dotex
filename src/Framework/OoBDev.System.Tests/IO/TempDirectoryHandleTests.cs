using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System.IO;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class TempDirectoryHandleTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Dispose_DirectoryWithNestedContent_DeletesEverything()
    {
        // Stage
        string path;
        var handle = new TempDirectoryHandle();
        path = handle.DirectoryPath;
        Directory.CreateDirectory(Path.Combine(path, "sub"));
        File.WriteAllText(Path.Combine(path, "sub", "a.txt"), "x");

        // Test
        Assert.IsTrue(Directory.Exists(path));
        handle.Dispose();

        // Assert
        Assert.IsFalse(Directory.Exists(path));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var handle = new TempDirectoryHandle();
        handle.Dispose();
        handle.Dispose();
        Assert.IsFalse(Directory.Exists(handle.DirectoryPath));
        Assert.AreEqual(handle.DirectoryPath, handle.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Dispose_FileHeldOpen_DoesNotThrow()
    {
        var handle = new TempDirectoryHandle();
        using var held = File.Create(Path.Combine(handle.DirectoryPath, "held.bin"));

        handle.Dispose();

        Assert.IsNotNull(held);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetTempDirectory_TwoCalls_ReturnDistinctExistingDirectories()
    {
        var factory = new TempFileFactory(null!);
        using var a = factory.GetTempDirectory();
        using var b = factory.GetTempDirectory();

        Assert.AreNotEqual(a.DirectoryPath, b.DirectoryPath);
        Assert.IsTrue(Directory.Exists(a.DirectoryPath));
        Assert.IsTrue(Directory.Exists(b.DirectoryPath));
    }
}
