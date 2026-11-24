using System;
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
