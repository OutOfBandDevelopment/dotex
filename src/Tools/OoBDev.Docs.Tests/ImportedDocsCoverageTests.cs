using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Docs.Tests;

[TestClass]
public class ImportedDocsCoverageTests
{
    private static void Write(string root, string relative, string text)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FindOrphans_MixedFiles_ReportsOnlyTheUnreachedOne()
    {
        // Stage
        var root = Directory.CreateTempSubdirectory("docs-coverage-");
        try
        {
            Write(root.FullName, "docs/applications/app/README.md", "[a](a.md) [bugs](bugs/)");
            Write(root.FullName, "docs/applications/app/a.md", "[deeper](deep/b.md)");
            Write(root.FullName, "docs/applications/app/deep/b.md", "reached through a.md");
            Write(root.FullName, "docs/applications/app/bugs/001.md", "covered by the folder link");
            Write(root.FullName, "docs/applications/app/mapped/c.md", "named in the migration map");
            Write(root.FullName, "docs/applications/app/lost.md", "nobody links this");
            Write(root.FullName, "docs/migration/map.md", "`docs/applications/app/mapped/` moves later");

            // Test
            var orphans = ImportedDocsCoverage.FindOrphans(root.FullName, Path.Combine(root.FullName, "docs", "applications", "app")).ToList();

            // Assert
            Assert.HasCount(1, orphans);
            StringAssert.StartsWith(orphans[0], "docs/applications/app/lost.md");
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FindOrphans_NoReadme_ReportsIt()
    {
        var root = Directory.CreateTempSubdirectory("docs-coverage-");
        try
        {
            Write(root.FullName, "docs/applications/app/a.md", "x");

            var orphans = ImportedDocsCoverage.FindOrphans(root.FullName, Path.Combine(root.FullName, "docs", "applications", "app")).ToList();

            Assert.HasCount(1, orphans);
            StringAssert.Contains(orphans[0], "no README.md");
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
