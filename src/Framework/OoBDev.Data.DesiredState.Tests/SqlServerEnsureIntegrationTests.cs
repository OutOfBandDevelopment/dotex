using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public class SqlServerEnsureIntegrationTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-sqlserver")]
    public async Task EnsureRun_ManifestOnSqlServer_CreatesTablesAndSecondRunChangesNothing()
    {
        // Stage
        var master = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING"));
        var database = $"Ensure_{Guid.NewGuid():N}";
        TestContext.WriteLine($"database {database}");
        await ExecuteAsync(master, $"CREATE DATABASE [{database}]");
        try
        {
            var builder = new SqlConnectionStringBuilder(master) { InitialCatalog = database };
            var executor = new AdoScriptExecutor(_ => new SqlConnection(builder.ConnectionString));
            var runner = new EnsureScriptRunner(executor, new InMemoryAppliedScriptStore());
            var target = new DatabaseTarget("sqlserver", 1433, database);
            var scripts = ManifestCompiler.Compile(FeatureManifest.Parse("""{ "queues": ["orders"], "streams": ["events"] }"""), "sqlserver");

            // Test
            await runner.RunAsync(scripts, target, TestContext.CancellationToken);
            var second = await runner.RunAsync(scripts, target, TestContext.CancellationToken);
            TestContext.WriteLine($"second run executed {second.Ran.Count} scripts");

            // Assert
            var tables = await ScalarAsync(builder.ConnectionString, "SELECT COUNT(*) FROM sys.tables WHERE name IN ('queue_orders','stream_events')");
            Assert.AreEqual(2, tables);
        }
        finally
        {
            SqlConnection.ClearAllPools();
            await ExecuteAsync(master, $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END");
        }
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-sqlserver")]
    public async Task Sqldef_MssqldefOnSqlServer_PlansACreateAndThenNothing()
    {
        // Stage
        if (!OnPath("mssqldef"))
        {
            Assert.Inconclusive("mssqldef (sqldef, MIT) is not on PATH.");
        }

        var master = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING"));
        var builder = new SqlConnectionStringBuilder(master);
        var source = builder.DataSource.Replace("tcp:", string.Empty, StringComparison.OrdinalIgnoreCase).Split(',');
        var host = source[0];
        var port = source.Length > 1 ? int.Parse(source[1], global::System.Globalization.CultureInfo.InvariantCulture) : 1433;
        var database = $"Def_{Guid.NewGuid():N}";
        TestContext.WriteLine($"database {database} on {host}:{port}");
        await ExecuteAsync(master, $"CREATE DATABASE [{database}]");
        try
        {
            var provider = SqldefProvider.ForSqlServer(new ProcessToolRunner());
            var desired = new DesiredSchema([new DesiredScript("001.sql", "CREATE TABLE t (id int NOT NULL PRIMARY KEY, name nvarchar(50));")]);
            var target = new DatabaseTarget(host, port, database, builder.UserID, builder.Password);

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
            SqlConnection.ClearAllPools();
            await ExecuteAsync(master, $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END");
        }
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-sqlserver")]
    public async Task AppliedStore_OnSqlServer_RecordsAndFindsAScriptAndRunsGoBatches()
    {
        // Stage
        var master = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING"));
        var database = $"Store_{Guid.NewGuid():N}";
        await ExecuteAsync(master, $"CREATE DATABASE [{database}]");
        try
        {
            var builder = new SqlConnectionStringBuilder(master) { InitialCatalog = database };
            SqlConnection Connect(DatabaseTarget _) => new(builder.ConnectionString);
            var store = new AdoAppliedScriptStore(Connect);
            var target = new DatabaseTarget("sqlserver", 1433, database);

            // Test
            var before = await store.HasAppliedAsync(target, "s1", "h1", TestContext.CancellationToken);
            await store.RecordAsync(target, "s1", "h1", TestContext.CancellationToken);
            var after = await store.HasAppliedAsync(target, "s1", "h1", TestContext.CancellationToken);
            await new AdoScriptExecutor(Connect).ExecuteAsync(target, "go", "CREATE TABLE t1 (id int)\nGO\nINSERT INTO t1 VALUES (1)\nGO", TestContext.CancellationToken);
            TestContext.WriteLine($"before={before} after={after}");

            // Assert
            Assert.IsFalse(before);
            Assert.IsTrue(after);
            Assert.AreEqual(1, await ScalarAsync(builder.ConnectionString, "SELECT COUNT(*) FROM t1"));
        }
        finally
        {
            SqlConnection.ClearAllPools();
            await ExecuteAsync(master, $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END");
        }
    }

    private static bool OnPath(string tool) =>
        (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(global::System.IO.Path.PathSeparator)
            .Any(d => d.Length > 0 && (global::System.IO.File.Exists(global::System.IO.Path.Combine(d, tool)) || global::System.IO.File.Exists(global::System.IO.Path.Combine(d, tool + ".exe"))));

    private async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(TestContext.CancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(TestContext.CancellationToken);
    }

    private async Task<int> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(TestContext.CancellationToken);
        await using var command = new SqlCommand(sql, connection);
        return (int)(await command.ExecuteScalarAsync(TestContext.CancellationToken))!;
    }
}
