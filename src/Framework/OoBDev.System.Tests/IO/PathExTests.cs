using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class PathExTests
{
    private static string Touch(string root, string relative)
    {
        var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "x");
        return path;
    }

    private static string[] Names(string root, params string[] found) =>
        [.. found.Select(f => Path.GetRelativePath(root, f).Replace('\\', '/')).Order(StringComparer.Ordinal)];

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EnumerateFiles_SegmentWildcardsAndDoubleStar_MatchAcrossDirectories()
    {
        using var directory = new TempFileFactory(null!).GetTempDirectory();
        var root = directory.DirectoryPath;
        Touch(root, "src/A.Tests/unit/One.cs");
        Touch(root, "src/B.Tests/deep/er/Two.cs");
        Touch(root, "src/C/unit/Three.cs");
        Touch(root, "src/A.Tests/unit/skip.txt");

        var found = PathEx.EnumerateFiles(Path.Combine(root, "src", "**", "*.Tests", "**", "*.cs"));

        CollectionAssert.AreEqual(new[] { "src/A.Tests/unit/One.cs", "src/B.Tests/deep/er/Two.cs" }, Names(root, [.. found]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EnumerateFiles_QuestionMarkAndSingleStar_MatchOneLevel()
    {
        using var directory = new TempFileFactory(null!).GetTempDirectory();
        var root = directory.DirectoryPath;
        Touch(root, "a/f1.txt");
        Touch(root, "a/f22.txt");
        Touch(root, "a/b/f3.txt");

        var found = PathEx.EnumerateFiles(Path.Combine(root, "a", "f?.txt"));
        var star = PathEx.EnumerateFiles(Path.Combine(root, "*", "*.txt"));

        CollectionAssert.AreEqual(new[] { "a/f1.txt" }, Names(root, [.. found]));
        CollectionAssert.AreEqual(new[] { "a/f1.txt", "a/f22.txt" }, Names(root, [.. star]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EnumerateFiles_ExistingFileOrTrailingSeparator_ReturnsFileOrDirectoryContents()
    {
        using var directory = new TempFileFactory(null!).GetTempDirectory();
        var root = directory.DirectoryPath;
        var file = Touch(root, "only.txt");
        Touch(root, "sub/inner.txt");

        Assert.AreEqual(file, PathEx.EnumerateFiles(file).Single());
        CollectionAssert.AreEqual(new[] { "only.txt" }, Names(root, [.. PathEx.EnumerateFiles(root + Path.DirectorySeparatorChar)]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EnumerateFiles_MissingBlankOrWildcardFreePath_ReturnsNothing()
    {
        using var directory = new TempFileFactory(null!).GetTempDirectory();

        Assert.IsFalse(PathEx.EnumerateFiles(null).Any());
        Assert.IsFalse(PathEx.EnumerateFiles(" ").Any());
        Assert.IsFalse(PathEx.EnumerateFiles(Path.Combine(directory.DirectoryPath, "missing", "*.txt")).Any());
        Assert.IsFalse(PathEx.EnumerateFiles(Path.Combine(directory.DirectoryPath, "nothing.txt")).Any());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EnumerateFiles_RootAndPatternsWithNegation_ExcludesWhatTheNegationMatches()
    {
        using var directory = new TempFileFactory(null!).GetTempDirectory();
        var root = directory.DirectoryPath;
        Touch(root, "src/Keep.cs");
        Touch(root, "src/obj/Skip.cs");
        Touch(root, "src/deep/Also.cs");

        var found = PathEx.EnumerateFiles(root, ["**/*.cs", "!**/obj/**"]);

        CollectionAssert.AreEqual(new[] { "src/Keep.cs", "src/deep/Also.cs" }, Names(root, [.. found]));
        Assert.IsFalse(PathEx.EnumerateFiles(Path.Combine(root, "missing"), ["**/*"]).Any());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetBasePath_WildcardPath_ReturnsTheLiteralPrefix()
    {
        var expected = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "base"));

        Assert.AreEqual(expected, PathEx.GetBasePath(Path.Combine(expected, "**", "*.cs")));
        Assert.IsNull(PathEx.GetBasePath(" "));
    }
}
