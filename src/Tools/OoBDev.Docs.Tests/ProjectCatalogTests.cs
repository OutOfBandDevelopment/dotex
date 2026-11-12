using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Docs.Tests;

[TestClass]
public class ProjectCatalogTests
{
    private static void Write(string root, string relative, string text)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Build_SmallTree_ListsProjectsWithPurposeFlagsAndNavigation()
    {
        // Stage
        var root = Directory.CreateTempSubdirectory("catalog-");
        try
        {
            Write(root.FullName, "src/Common/OoBDev.A/OoBDev.A.csproj", "<Project />");
            Write(root.FullName, "src/Common/OoBDev.A/README.A.md", "# A\n\nDoes the first thing well. Then more.\n");
            Write(root.FullName, "src/Framework/OoBDev.B/OoBDev.B.csproj", "<Project />");
            Write(root.FullName, "src/Framework/OoBDev.B.Tests/OoBDev.B.Tests.csproj", "<Project />");
            Write(root.FullName, "src/Framework/OoBDev.C/OoBDev.C.csproj", "<Project />");
            Write(root.FullName, "src/Framework/OoBDev.C/bin/Ignored/Ignored.csproj", "<Project />");
            Write(root.FullName, "src/Applications/OoBDev.App/OoBDev.App.csproj", "<Project />");

            // Test
            var files = ProjectCatalog.Build(root.FullName);

            // Assert
            CollectionAssert.AreEqual(new[] { "01-common.md", "02-framework.md", "03-applications.md", "README.md" }, files.Keys.ToList());
            StringAssert.Contains(files["01-common.md"], "| `OoBDev.A` | Does the first thing well. |   |");
            StringAssert.Contains(files["01-common.md"], "[← Index](./README.md) · [Framework Layer →](./02-framework.md)");
            StringAssert.Contains(files["02-framework.md"], "| `OoBDev.B` |   | no readme |");
            StringAssert.Contains(files["02-framework.md"], "| `OoBDev.C` |   | no readme, no tests project |");
            StringAssert.Contains(files["02-framework.md"], "2 projects (test projects excluded).");
            Assert.DoesNotContain("Ignored", files["02-framework.md"]);
            StringAssert.Contains(files["03-applications.md"], "[Index →](./README.md)");
            StringAssert.Contains(files["README.md"], "3. [Table 3 — Applications projects](./03-applications.md)");
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Write_RepositoryTree_CatalogIsCurrent()
    {
        // Stage
        var root = RepoRoot();
        var folder = Path.Combine(root, "docs", "patterns-discovery", "07-project-catalog");

        // Test
        var written = ProjectCatalog.Write(root, folder);

        // Assert
        Assert.IsTrue(File.Exists(Path.Combine(folder, "README.md")));
        Assert.IsTrue(File.Exists(Path.Combine(folder, "05-hardwaredevices.md")), "every src layer has a catalog file");
        Assert.IsEmpty(ProjectCatalog.Write(root, folder), $"a second run writes nothing (first run wrote {written.Count})");
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
}
