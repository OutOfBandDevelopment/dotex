using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;
using OoBDev.System.IO;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public sealed class InstanceScopeTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task PlanAsync_PostgresWithASchema_PassesTargetSchema()
    {
        // Stage
        var runner = new Recorder();
        var target = new DatabaseTarget("db.test", 5432, "app", "me").ForInstance(schema: "tickets");

        // Test
        await SqldefProvider.ForPostgres(runner).PlanAsync(new DesiredSchema([]), target, TestContext.CancellationToken);
        TestContext.WriteLine($"args={string.Join(' ', runner.Arguments)}");

        // Assert
        CollectionAssert.AreEqual(new[] { "-h", "db.test", "-p", "5432", "-U", "me", "--config-inline", "target_schema: tickets", "--dry-run", "app" }, runner.Arguments.ToArray());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task PlanAsync_SqliteWithAPrefix_PassesAnEscapedTargetTables()
    {
        // Stage
        var runner = new Recorder();
        var target = DatabaseTarget.File("app.db").ForInstance(tablePrefix: "ticket_");

        // Test
        await SqldefProvider.ForSqlite(runner).PlanAsync(new DesiredSchema([]), target, TestContext.CancellationToken);

        // Assert
        CollectionAssert.AreEqual(new[] { "--config-inline", "target_tables: ticket_.*", "--dry-run", "app.db" }, runner.Arguments.ToArray());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task PlanAsync_WithoutAScope_PassesNoConfig()
    {
        // Stage
        var runner = new Recorder();

        // Test
        await SqldefProvider.ForSqlite(runner).PlanAsync(new DesiredSchema([]), DatabaseTarget.File("app.db"), TestContext.CancellationToken);

        // Assert
        Assert.IsFalse(runner.Arguments.Contains("--config-inline"));
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Sqlite_TwoPrefixedInstancesInOneFile_NeverPlanEachOthersDrops()
    {
        // Stage
        if (!ProviderTests.IsOnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var provider = SqldefProvider.ForSqlite(new ProcessToolRunner());
        var file = DatabaseTarget.File(Path.Combine(folder.DirectoryPath, "app.db"));
        var a = file.ForInstance(tablePrefix: "a_");
        var b = file.ForInstance(tablePrefix: "b_");
        var schemaA = new DesiredSchema([new DesiredScript("a.sql", "CREATE TABLE a_notes (id integer primary key, body text);")]);
        var schemaB = new DesiredSchema([new DesiredScript("b.sql", "CREATE TABLE b_notes (id integer primary key, body text);")]);

        // Test
        await provider.ApplyAsync(schemaA, a, await provider.PlanAsync(schemaA, a, TestContext.CancellationToken), new ApplyOptions(), TestContext.CancellationToken);
        await provider.ApplyAsync(schemaB, b, await provider.PlanAsync(schemaB, b, TestContext.CancellationToken), new ApplyOptions(), TestContext.CancellationToken);
        var againA = await provider.PlanAsync(schemaA, a, TestContext.CancellationToken);
        var againB = await provider.PlanAsync(schemaB, b, TestContext.CancellationToken);
        var unscopedA = await provider.PlanAsync(schemaA, file, TestContext.CancellationToken);
        TestContext.WriteLine($"scoped A={againA.Statements.Count}/{againA.Skipped.Count} B={againB.Statements.Count}/{againB.Skipped.Count}; unscoped A skipped={unscopedA.Skipped.Count}");

        // Assert
        Assert.IsTrue(againA.IsEmpty);
        Assert.IsEmpty(againA.Skipped);
        Assert.IsTrue(againB.IsEmpty);
        Assert.IsEmpty(againB.Skipped);
        Assert.IsNotEmpty(unscopedA.Skipped, "Without a scope the other instance's table looks like a drop; the scope is what hides it.");
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Postgres_TwoSchemasInOneDatabase_NeverPlanEachOthersDrops()
    {
        // Stage
        if (!ProviderTests.IsOnPath("psqldef"))
        {
            Assert.Inconclusive("psqldef (sqldef, MIT) is not on PATH.");
        }

        var admin = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING"));
        var builder = new NpgsqlConnectionStringBuilder(admin);
        var database = $"inst_{Guid.NewGuid():N}";
        await PgExec(admin, $"CREATE DATABASE \"{database}\"");
        try
        {
            var dbAdmin = new NpgsqlConnectionStringBuilder(admin) { Database = database, Pooling = false }.ConnectionString;
            await PgExec(dbAdmin, "CREATE SCHEMA a; CREATE SCHEMA b;");
            var provider = SqldefProvider.ForPostgres(new ProcessToolRunner());
            var root = new DatabaseTarget(builder.Host!, builder.Port, database, builder.Username, builder.Password);
            var a = root.ForInstance(schema: "a");
            var b = root.ForInstance(schema: "b");
            var schemaA = new DesiredSchema([new DesiredScript("a.sql", "CREATE TABLE a.notes (id integer primary key, body text);")]);
            var schemaB = new DesiredSchema([new DesiredScript("b.sql", "CREATE TABLE b.notes (id integer primary key, body text);")]);

            // Test
            await provider.ApplyAsync(schemaA, a, await provider.PlanAsync(schemaA, a, TestContext.CancellationToken), new ApplyOptions(), TestContext.CancellationToken);
            await provider.ApplyAsync(schemaB, b, await provider.PlanAsync(schemaB, b, TestContext.CancellationToken), new ApplyOptions(), TestContext.CancellationToken);
            var againA = await provider.PlanAsync(schemaA, a, TestContext.CancellationToken);
            var againB = await provider.PlanAsync(schemaB, b, TestContext.CancellationToken);
            var dropA = await provider.PlanAsync(new DesiredSchema([]), a, TestContext.CancellationToken);
            TestContext.WriteLine($"scoped A={againA.Statements.Count}/{againA.Skipped.Count} B={againB.Statements.Count}/{againB.Skipped.Count}; emptying A skips {dropA.Skipped.Count}");

            // Assert
            Assert.IsTrue(againA.IsEmpty);
            Assert.IsEmpty(againA.Skipped);
            Assert.IsTrue(againB.IsEmpty);
            Assert.IsEmpty(againB.Skipped);
            Assert.IsTrue(dropA.Skipped.All(s => !s.Contains("b.", StringComparison.Ordinal) && !s.Contains("\"b\"", StringComparison.Ordinal)), "A plan scoped to schema a must not mention schema b.");
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await PgExec(admin, $"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)");
        }
    }

    private static async Task PgExec(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed class Recorder : IToolRunner
    {
        public IReadOnlyList<string> Arguments { get; private set; } = [];

        public Task<ToolResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string standardInput, IReadOnlyDictionary<string, string> environment, CancellationToken cancellationToken = default)
        {
            Arguments = arguments;
            return Task.FromResult(new ToolResult(0, string.Empty, string.Empty));
        }
    }
}
