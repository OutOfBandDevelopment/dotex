using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DataLoader.Cli.Tests;

[TestClass]
public sealed class LoaderCommandTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RunAsync_DryRun_ReportsWithoutWriting()
    {
        // Stage
        var path = Path.Combine(AppContext.BaseDirectory, $"rows_{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(path, "id,name\n1,Ann\n2,Bo\n1,Dup\n", TestContext.CancellationToken);
        try
        {
            using var output = new StringWriter();

            // Test
            var code = await LoaderCommand.RunAsync(["--input", path, "--key", "id"], output, TextWriter.Null, TestContext.CancellationToken);
            TestContext.WriteLine(output.ToString());

            // Assert
            Assert.AreEqual(0, code);
            Assert.Contains("dry run: read 3, written 2, replaced 0, skipped 1, errors 0", output.ToString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RunAsync_Sqlite_WritesRowsAndReplacesOnSecondRun()
    {
        // Stage
        var path = Path.Combine(AppContext.BaseDirectory, $"rows_{Guid.NewGuid():N}.csv");
        var db = Path.Combine(AppContext.BaseDirectory, $"store_{Guid.NewGuid():N}.db");
        await File.WriteAllTextAsync(path, "id,name\n1,Ann\n2,Bo\n", TestContext.CancellationToken);
        try
        {
            using var first = new StringWriter();
            using var second = new StringWriter();

            // Test
            var code1 = await LoaderCommand.RunAsync(["--input", path, "--key", "id", "--sqlite", db], first, TextWriter.Null, TestContext.CancellationToken);
            var code2 = await LoaderCommand.RunAsync(["--input", path, "--key", "id", "--sqlite", db, "--duplicates", "replace"], second, TextWriter.Null, TestContext.CancellationToken);
            TestContext.WriteLine($"{first}{second}");

            // Assert
            Assert.AreEqual(0, code1);
            Assert.AreEqual(0, code2);
            Assert.Contains("loaded: read 2, written 2", first.ToString());
            Assert.Contains("replaced 2", second.ToString());
        }
        finally
        {
            File.Delete(path);
            SqliteConnection.ClearAllPools();
            File.Delete(db);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RunAsync_DuplicateWithFailPolicy_ReportsTheLineWithExitCodeOne()
    {
        // Stage
        var path = Path.Combine(AppContext.BaseDirectory, $"rows_{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(path, "id,name\n1,Ann\n1,Dup\n", TestContext.CancellationToken);
        try
        {
            using var error = new StringWriter();

            // Test
            var code = await LoaderCommand.RunAsync(["--input", path, "--key", "id", "--duplicates", "fail"], TextWriter.Null, error, TestContext.CancellationToken);
            TestContext.WriteLine($"exit={code} {error}");

            // Assert
            Assert.AreEqual(1, code);
            Assert.Contains(":3:", error.ToString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RunAsync_NoArguments_PrintsUsage()
    {
        // Test
        using var error = new StringWriter();
        var code = await LoaderCommand.RunAsync([], TextWriter.Null, error, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(2, code);
        Assert.Contains("data-loader", error.ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RunAsync_MissingFile_ReportsTheErrorWithoutThrowing()
    {
        // Test
        using var error = new StringWriter();
        var code = await LoaderCommand.RunAsync(["--input", Path.Combine(AppContext.BaseDirectory, "none.csv"), "--key", "id"], TextWriter.Null, error, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(2, code);
        Assert.IsGreaterThan(0, error.ToString().Length);
    }
}
