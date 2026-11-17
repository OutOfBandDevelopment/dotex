using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Docs.Tests;

[TestClass]
public class ApiReferenceTests
{
    private const string _doc = "<doc><assembly><name>OoBDev.A</name></assembly><members><member name=\"T:OoBDev.A.Thing\"><summary>A thing.</summary></member></members></doc>";

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Write_RepositoryTree_WritesPagesToBuildOutput()
    {
        // Stage
        var root = RepoRoot();
        var folder = Path.Combine(AppContext.BaseDirectory, "api-reference");

        // Test
        ApiReference.Write(root, folder);

        // Assert
        Assert.IsEmpty(ApiReference.Write(root, folder), "a second run writes nothing");
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CLAUDE.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root (CLAUDE.md) not found.");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Write_ProjectWithXmlDoc_WritesPageRemovesStaleAndIsStable()
    {
        // Stage
        var root = Directory.CreateTempSubdirectory("apiref-");
        try
        {
            var project = Path.Combine(root.FullName, "src", "Framework", "OoBDev.A");
            Directory.CreateDirectory(Path.Combine(project, "bin", "Debug", "net10.0"));
            File.WriteAllText(Path.Combine(project, "OoBDev.A.csproj"), "<Project />");
            File.WriteAllText(Path.Combine(project, "bin", "Debug", "net10.0", "OoBDev.A.xml"), _doc);
            var folder = Path.Combine(root.FullName, "out");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "OoBDev.Removed.md"), "old");

            // Test
            var first = ApiReference.Write(root.FullName, folder);
            var second = ApiReference.Write(root.FullName, folder);

            // Assert
            CollectionAssert.AreEquivalent(new[] { "OoBDev.Removed.md", "OoBDev.A.md" }, first.ToArray());
            Assert.IsEmpty(second);
            StringAssert.Contains(File.ReadAllText(Path.Combine(folder, "OoBDev.A.md")), "## OoBDev.A.Thing");
            Assert.IsFalse(File.Exists(Path.Combine(folder, "OoBDev.Removed.md")));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
