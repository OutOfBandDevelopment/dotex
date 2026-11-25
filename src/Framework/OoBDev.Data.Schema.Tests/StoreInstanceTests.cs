using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.Communications.EntityFrameworkCore;
using OoBDev.Data.DesiredState;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.Data.Schema.Tests;

/// <summary>One store deployed twice into one database (design: store instances): the model follows the instance and sqldef scopes each plan.</summary>
[TestClass]
public class StoreInstanceTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Suppressions_PostgresSchemaInstance_PlacesTheTableInThatSchema()
    {
        // Test
        var a = Script(b => b.UseNpgsql("Host=unused;Database=unused").UseSuppressionsInstance(schema: "a"));
        var other = Script(b => b.UseNpgsql("Host=unused;Database=unused").UseSuppressionsInstance(schema: "b"));
        var plain = Script(b => b.UseNpgsql("Host=unused;Database=unused"));
        TestContext.WriteLine(a);

        // Assert
        StringAssert.Contains(a, "a.\"Suppressions\"");
        StringAssert.Contains(other, "b.\"Suppressions\"");
        Assert.DoesNotContain("a.\"", other);
        Assert.DoesNotContain("a.\"Suppressions", plain);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Suppressions_SqlitePrefixInstance_PrefixesTableAndConstraintNames()
    {
        // Test
        var a = Script(b => b.UseSqlite("Data Source=unused.db").UseSuppressionsInstance(tablePrefix: "a_"));
        var other = Script(b => b.UseSqlite("Data Source=unused.db").UseSuppressionsInstance(tablePrefix: "b_"));
        TestContext.WriteLine(a);

        // Assert
        StringAssert.Contains(a, "\"a_Suppressions\"");
        StringAssert.Contains(a, "PK_a_Suppressions");
        Assert.DoesNotContain("a_Suppressions", other);
        StringAssert.Contains(other, "\"b_Suppressions\"");
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Suppressions_TwoSqlitePrefixInstancesInOneFile_DeployAndPlanNothing()
    {
        // Stage
        if (!FinalStateScriptTests.OnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var file = DatabaseTarget.File(Path.Combine(folder.DirectoryPath, "app.db"));
        var provider = SqldefProvider.ForSqlite(new ProcessToolRunner());
        var schemas = new[] { "a_", "b_" };

        // Test
        foreach (var prefix in schemas)
        {
            var desired = new DesiredSchema([new DesiredScript(prefix + ".sql", Script(b => b.UseSqlite("Data Source=unused.db").UseSuppressionsInstance(tablePrefix: prefix)))]);
            var target = file.ForInstance(tablePrefix: prefix);
            await provider.ApplyAsync(desired, target, await provider.PlanAsync(desired, target, TestContext.CancellationToken), new ApplyOptions(), TestContext.CancellationToken);
        }

        // Assert
        foreach (var prefix in schemas)
        {
            var desired = new DesiredSchema([new DesiredScript(prefix + ".sql", Script(b => b.UseSqlite("Data Source=unused.db").UseSuppressionsInstance(tablePrefix: prefix)))]);
            var again = await provider.PlanAsync(desired, file.ForInstance(tablePrefix: prefix), TestContext.CancellationToken);
            TestContext.WriteLine($"{prefix}: statements={again.Statements.Count} skipped={again.Skipped.Count}");
            Assert.IsTrue(again.IsEmpty, string.Join("\n", again.Statements));
            Assert.IsEmpty(again.Skipped);
        }
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    [TestCategory(TestCategories.Integration)]
    public async Task Suppressions_TwoPostgresSchemaInstances_DeployAndPlanNothing()
    {
        // Stage
        if (!FinalStateScriptTests.OnPath("psqldef"))
        {
            Assert.Inconclusive("psqldef (sqldef, MIT) is not on PATH.");
        }

        var admin = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING"));
        var builder = new NpgsqlConnectionStringBuilder(admin);
        var database = $"inst_{Guid.NewGuid():N}";
        await ExecuteAsync(admin, $"CREATE DATABASE \"{database}\"");
        try
        {
            var provider = SqldefProvider.ForPostgres(new ProcessToolRunner());
            var root = new DatabaseTarget(builder.Host!, builder.Port, database, builder.Username, builder.Password);
            var dbAdmin = new NpgsqlConnectionStringBuilder(admin) { Database = database, Pooling = false }.ConnectionString;
            await ExecuteAsync(dbAdmin, "CREATE SCHEMA a; CREATE SCHEMA b;");

            // Test
            foreach (var schema in new[] { "a", "b" })
            {
                var desired = new DesiredSchema([new DesiredScript(schema + ".sql", Script(b => b.UseNpgsql("Host=unused;Database=unused").UseSuppressionsInstance(schema: schema)))]);
                var target = root.ForInstance(schema: schema);
                await provider.ApplyAsync(desired, target, await provider.PlanAsync(desired, target, TestContext.CancellationToken), new ApplyOptions(), TestContext.CancellationToken);
            }

            // Assert
            foreach (var schema in new[] { "a", "b" })
            {
                var desired = new DesiredSchema([new DesiredScript(schema + ".sql", Script(b => b.UseNpgsql("Host=unused;Database=unused").UseSuppressionsInstance(schema: schema)))]);
                var again = await provider.PlanAsync(desired, root.ForInstance(schema: schema), TestContext.CancellationToken);
                TestContext.WriteLine($"{schema}: statements={again.Statements.Count} skipped={again.Skipped.Count}");
                Assert.IsTrue(again.IsEmpty, string.Join("\n", again.Statements));
                Assert.IsEmpty(again.Skipped);
            }
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(admin, $"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)");
        }
    }

    private static string Script(Action<DbContextOptionsBuilder<SuppressionsDbContext>> configure)
    {
        var builder = new DbContextOptionsBuilder<SuppressionsDbContext>();
        configure(builder);
        using var context = new SuppressionsDbContext(builder.Options);
        var script = context.Database.GenerateCreateScript().Replace("\r\n", "\n", StringComparison.Ordinal);
        // EF emits a DO block that creates the schema; sqldef cannot parse it, so the schema is created by an ensure step instead.
        return Regex.Replace(script, @"DO \$EF\$.*?END \$EF\$;\s*", string.Empty, RegexOptions.Singleline);
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
