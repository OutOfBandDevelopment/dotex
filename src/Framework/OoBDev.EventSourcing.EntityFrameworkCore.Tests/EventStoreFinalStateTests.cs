using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.Data.DesiredState;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

/// <summary>
/// Final-state scripts for PostgreSQL and SQLite (build plan phase A): the committed script equals the model, and sqldef takes an
/// empty database to that script and then plans nothing. Scripts live in <c>Schema/{engine}</c> until the schema project is renamed.
/// </summary>
[TestClass]
public class EventStoreFinalStateTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EventStore_PostgresScriptMatchesTheModel() => AssertMatches("postgres", "EventStore", GeneratePostgres());

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EventStore_SqliteScriptMatchesTheModel() => AssertMatches("sqlite", "EventStore", GenerateSqlite());

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task EventStore_SqliteEmptyToFinalState_ThenPlansNothing() => await SqliteRoundTripAsync("EventStore");

    private async Task SqliteRoundTripAsync(string name)
    {
        // Stage
        if (!OnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var provider = SqldefProvider.ForSqlite(new ProcessToolRunner());
        var desired = new DesiredSchema([new DesiredScript($"{name}.sql", File.ReadAllText(ScriptPath("sqlite", name)))]);
        var target = DatabaseTarget.File(Path.Combine(folder.DirectoryPath, "app.db"));

        // Test
        var plan = await provider.PlanAsync(desired, target, TestContext.CancellationToken);
        await provider.ApplyAsync(desired, target, plan, new ApplyOptions(), TestContext.CancellationToken);
        var again = await provider.PlanAsync(desired, target, TestContext.CancellationToken);
        TestContext.WriteLine($"first={plan.Statements.Count} second={again.Statements.Count}");

        // Assert
        Assert.IsFalse(plan.IsEmpty);
        Assert.IsTrue(again.IsEmpty, string.Join("\n", again.Statements));
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task EventStore_PostgresEmptyToFinalState_ThenPlansNothing() => await PostgresRoundTripAsync("EventStore");

    private async Task PostgresRoundTripAsync(string name)
    {
        // Stage
        if (!OnPath("psqldef"))
        {
            Assert.Inconclusive("psqldef (sqldef, MIT) is not on PATH.");
        }

        var admin = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING"));
        var builder = new NpgsqlConnectionStringBuilder(admin);
        var database = $"final_{Guid.NewGuid():N}";
        await ExecuteAsync(admin, $"CREATE DATABASE \"{database}\"");
        try
        {
            var provider = SqldefProvider.ForPostgres(new ProcessToolRunner());
            var desired = new DesiredSchema([new DesiredScript($"{name}.sql", File.ReadAllText(ScriptPath("postgres", name)))]);
            var target = new DatabaseTarget(builder.Host!, builder.Port, database, builder.Username, builder.Password);

            // Test
            var plan = await provider.PlanAsync(desired, target, TestContext.CancellationToken);
            await provider.ApplyAsync(desired, target, plan, new ApplyOptions(), TestContext.CancellationToken);
            var again = await provider.PlanAsync(desired, target, TestContext.CancellationToken);
            TestContext.WriteLine($"first={plan.Statements.Count} second={again.Statements.Count}");

            // Assert
            Assert.IsFalse(plan.IsEmpty);
            Assert.IsTrue(again.IsEmpty, string.Join("\n", again.Statements));
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(admin, $"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)");
        }
    }

    private static string GeneratePostgres() => GeneratePostgres<EventSourcingDbContext>();

    private static string GeneratePostgres<T>() where T : DbContext
    {
        using var context = Create<T>(new DbContextOptionsBuilder<T>().UseNpgsql("Host=unused;Database=unused").Options);
        return context.Database.GenerateCreateScript();
    }

    private static string GenerateSqlite() => GenerateSqlite<EventSourcingDbContext>();

    private static string GenerateSqlite<T>() where T : DbContext
    {
        using var context = Create<T>(new DbContextOptionsBuilder<T>().UseSqlite("Data Source=unused.db").Options);
        return context.Database.GenerateCreateScript();
    }

    private static T Create<T>(DbContextOptions<T> options) where T : DbContext => (T)Activator.CreateInstance(typeof(T), options)!;

    private static void AssertMatches(string engine, string name, string generated)
    {
        var text = Normalize(generated);
        var file = ScriptPath(engine, name);
        if (Environment.GetEnvironmentVariable("REGENERATE_SQL_SCHEMA") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, text);
        }

        Assert.IsTrue(File.Exists(file), $"Missing {file}; run with REGENERATE_SQL_SCHEMA=1.");
        Assert.AreEqual(text, Normalize(File.ReadAllText(file)), $"{engine}/{name}.sql is out of date; run with REGENERATE_SQL_SCHEMA=1.");
    }

    private static string Normalize(string sql)
    {
        var text = sql.Replace("\r\n", "\n", StringComparison.Ordinal);
        while (text.Contains("\n\n\n", StringComparison.Ordinal))
        {
            text = text.Replace("\n\n\n", "\n\n", StringComparison.Ordinal);
        }

        return text.Trim() + "\n";
    }

    private static string ScriptPath(string engine, string name) => Path.Combine(FindProject(), "Schema", engine, $"{name}.sql");

    private static string FindProject()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "OoBDev.EventSourcing.EntityFrameworkCore.Tests.csproj")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Test project folder not found.");
    }

    internal static bool OnPath(string tool) =>
        (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator)
            .Any(d => d.Length > 0 && (File.Exists(Path.Combine(d, tool)) || File.Exists(Path.Combine(d, tool + ".exe"))));

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
