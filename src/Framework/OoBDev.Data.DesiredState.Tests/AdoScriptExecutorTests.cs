using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public class AdoScriptExecutorTests
{
    public TestContext TestContext { get; set; } = null!;

    private static AdoScriptExecutor SqliteExecutor() => new(t => new SqliteConnection($"Data Source={t.Database};Pooling=False"));

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task EnsureRun_ManifestOnSqlite_CreatesTablesAndSecondRunChangesNothing()
    {
        // Stage
        using var directory = new TempFileFactory(null!).GetTempDirectory();
        var path = Path.Combine(directory.DirectoryPath, "ensure.db");
        TestContext.WriteLine($"database {path}");
        var target = DatabaseTarget.File(path);
        var runner = new EnsureScriptRunner(SqliteExecutor(), new InMemoryAppliedScriptStore());
        var scripts = ManifestCompiler.Compile(FeatureManifest.Parse("""{ "queues": ["orders"], "streams": ["events"] }"""), "sqlite");

        // Test
        await runner.RunAsync(scripts, target, TestContext.CancellationToken);
        await runner.RunAsync(scripts, target, TestContext.CancellationToken);

        // Assert
        await using var connection = new SqliteConnection($"Data Source={path};Pooling=False");
        await connection.OpenAsync(TestContext.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT count(*) FROM sqlite_master WHERE name IN ('queue_orders','stream_events')";
        Assert.AreEqual(2L, await command.ExecuteScalarAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Execute_BadSql_ThrowsNamingTheScript()
    {
        using var directory = new TempFileFactory(null!).GetTempDirectory();
        var target = DatabaseTarget.File(Path.Combine(directory.DirectoryPath, "ensure.db"));

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => SqliteExecutor().ExecuteAsync(target, "010_bad.sql", "NOT SQL", TestContext.CancellationToken));

        StringAssert.Contains(ex.Message, "010_bad.sql");
    }
}
