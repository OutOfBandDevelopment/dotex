using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OoBDev.MigrationHelper.Cli.Tests;

[TestClass]
public class PrefixMigratorTests
{
    private string _folder = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _folder = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_folder, "Old.App", "Old.Inner"));
        File.WriteAllText(Path.Combine(_folder, "Old.App", "Old.Inner", "Old.Thing.cs"), "namespace Old.Thing;");
        File.WriteAllText(Path.Combine(_folder, "notes.md"), "see Old.App");
        File.WriteAllText(Path.Combine(_folder, "clean.txt"), "nothing here");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    [TestCategory("Unit")]
    [TestMethod]
    public void Run_NestedPrefixedFolders_RenamesAllLevelsAndRewritesText()
    {
        // Stage
        using var output = new StringWriter();
        using var error = new StringWriter();

        // Test
        var summary = PrefixMigrator.Run(_folder, "Old", "New", dryRun: false, output, error);

        // Assert
        var renamed = Path.Combine(_folder, "New.App", "New.Inner", "New.Thing.cs");
        Assert.IsTrue(File.Exists(renamed));
        Assert.AreEqual("namespace New.Thing;", File.ReadAllText(renamed));
        Assert.AreEqual("see New.App", File.ReadAllText(Path.Combine(_folder, "notes.md")));
        Assert.AreEqual(new MigrationSummary(2, 1, 2, 0), summary);
        Assert.AreEqual(string.Empty, error.ToString());
    }

    [TestCategory("Unit")]
    [TestMethod]
    public void Run_DryRun_ChangesNothingAndReportsCounts()
    {
        // Stage
        using var output = new StringWriter();
        using var error = new StringWriter();

        // Test
        var summary = PrefixMigrator.Run(_folder, "Old", "New", dryRun: true, output, error);

        // Assert
        Assert.IsTrue(File.Exists(Path.Combine(_folder, "Old.App", "Old.Inner", "Old.Thing.cs")));
        Assert.AreEqual("see Old.App", File.ReadAllText(Path.Combine(_folder, "notes.md")));
        Assert.AreEqual(new MigrationSummary(2, 1, 2, 0), summary);
        StringAssert.Contains(output.ToString(), "Would rename: Old.App");
    }

    [TestCategory("Unit")]
    [TestMethod]
    public void Run_BinaryFileWithPrefix_IsSkippedAndLeftUnchanged()
    {
        // Stage
        var binary = Path.Combine(_folder, "data.bin");
        File.WriteAllText(binary, "Old\0Old");
        using var output = new StringWriter();
        using var error = new StringWriter();

        // Test
        var summary = PrefixMigrator.Run(_folder, "Old", "New", dryRun: false, output, error);

        // Assert
        Assert.AreEqual("Old\0Old", File.ReadAllText(binary));
        Assert.AreEqual(1, summary.FilesSkipped);
        StringAssert.Contains(output.ToString(), "Skip:");
    }
}
