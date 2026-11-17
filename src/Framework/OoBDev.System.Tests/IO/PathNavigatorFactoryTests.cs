using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class PathNavigatorFactoryTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToNavigable_Directory_ExposesFilesAndFoldersWithMetadata()
    {
        // Stage
        using var temp = new TempFileFactory(null!).GetTempDirectory();
        Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "sub"));
        File.WriteAllText(Path.Combine(temp.DirectoryPath, "sub", "a.txt"), "abc");
        File.WriteAllText(Path.Combine(temp.DirectoryPath, "root.md"), "x");

        // Test
        var nav = new DirectoryInfo(temp.DirectoryPath).ToNavigable().CreateNavigator()!;

        // Assert
        Assert.AreEqual("sub", nav.SelectSingleNode("/Directory/Directory/@Name")?.Value);
        Assert.AreEqual("a.txt", nav.SelectSingleNode("/Directory/Directory/File/@Name")?.Value);
        Assert.AreEqual("3", nav.SelectSingleNode("/Directory/Directory/File/@Length")?.Value);
        Assert.AreEqual(Path.Combine("sub", "a.txt").Insert(0, Path.DirectorySeparatorChar.ToString()), nav.SelectSingleNode("/Directory/Directory/File/@RelativeName")?.Value);
        Assert.AreEqual(".md", nav.SelectSingleNode("/Directory/File/@Extension")?.Value);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToNavigable_CustomRootName_IsUsed()
    {
        using var temp = new TempFileFactory(null!).GetTempDirectory();

        var nav = new DirectoryInfo(temp.DirectoryPath).ToNavigable("Folder").CreateNavigator()!;

        Assert.IsNotNull(nav.SelectSingleNode("/Folder"));
    }
}
