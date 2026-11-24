using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public class PostgresDefIntegrationTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Sqldef_PsqldefOnPostgres_PlansACreateAndThenNothing()
    {
        // Stage
        if (!OnPath("psqldef"))
        {
            Assert.Inconclusive("psqldef (sqldef, MIT) is not on PATH; build it with go install github.com/sqldef/sqldef/v3/cmd/psqldef@latest.");
        }

        var admin = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING"));
        var builder = new NpgsqlConnectionStringBuilder(admin);
        var database = $"def_{Guid.NewGuid():N}";
        TestContext.WriteLine($"database {database} on {builder.Host}:{builder.Port}");
        await ExecuteAsync(admin, $"CREATE DATABASE \"{database}\"");
        try
        {
            var provider = SqldefProvider.ForPostgres(new ProcessToolRunner());
            var desired = new DesiredSchema([new DesiredScript("001.sql", "CREATE TABLE t (id integer PRIMARY KEY, name text);")]);
            var target = new DatabaseTarget(builder.Host!, builder.Port, database, builder.Username, builder.Password);

            // Test
            var plan = await provider.PlanAsync(desired, target, TestContext.CancellationToken);
            await provider.ApplyAsync(desired, target, plan, new ApplyOptions(), TestContext.CancellationToken);
            var again = await provider.PlanAsync(desired, target, TestContext.CancellationToken);
            TestContext.WriteLine($"first={plan.Statements.Count} second={again.Statements.Count}");

            // Assert
            Assert.IsFalse(plan.IsEmpty);
            Assert.IsTrue(again.IsEmpty);
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(admin, $"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)");
        }
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task EnsureRun_ManifestOnPostgres_CreatesTablesAndSecondRunChangesNothing()
    {
        // Stage
        var admin = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING"));
        var database = $"ensure_{Guid.NewGuid():N}";
        TestContext.WriteLine($"database {database}");
        await ExecuteAsync(admin, $"CREATE DATABASE \"{database}\"");
        try
        {
            var connectionString = new NpgsqlConnectionStringBuilder(admin) { Database = database }.ConnectionString;
            var runner = new EnsureScriptRunner(new AdoScriptExecutor(_ => new NpgsqlConnection(connectionString)), new InMemoryAppliedScriptStore());
            var target = new DatabaseTarget("postgres", 5432, database);
            var scripts = ManifestCompiler.Compile(FeatureManifest.Parse("""{ "queues": ["orders"], "streams": ["events"] }"""), "postgres");

            // Test
            await runner.RunAsync(scripts, target, TestContext.CancellationToken);
            var second = await runner.RunAsync(scripts, target, TestContext.CancellationToken);
            TestContext.WriteLine($"second run re-ran {second.Ran.Count} idempotent scripts without error");

            // Assert
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(TestContext.CancellationToken);
            await using var command = new NpgsqlCommand("SELECT COUNT(*) FROM information_schema.tables WHERE table_name IN ('queue_orders','stream_events')", connection);
            Assert.AreEqual(2L, await command.ExecuteScalarAsync(TestContext.CancellationToken));
            Assert.AreEqual(2, second.Ran.Count);
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(admin, $"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)");
        }
    }

    private async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(TestContext.CancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(TestContext.CancellationToken);
    }

    private static bool OnPath(string tool) =>
        (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator)
            .Any(d => d.Length > 0 && (File.Exists(Path.Combine(d, tool)) || File.Exists(Path.Combine(d, tool + ".exe"))));
}
