using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public sealed class ProviderTests
{
    private static readonly DesiredSchema _desired = new([new DesiredScript("001.sql", "CREATE TABLE a (id int);"), new DesiredScript("002.sql", "CREATE TABLE b (id int);")]);
    private static readonly DatabaseTarget _target = new("db.test", 5432, "app", "me", "secret");

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task PlanAsync_Postgres_RunsDryRunWithPasswordInTheEnvironmentOnly()
    {
        // Stage
        var runner = new FakeRunner("CREATE TABLE a (id int);\n");
        var provider = SqldefProvider.ForPostgres(runner);

        // Test
        var plan = await provider.PlanAsync(_desired, _target, TestContext.CancellationToken);
        TestContext.WriteLine($"args={string.Join(' ', runner.Calls[0].Arguments)}");

        // Assert
        Assert.AreEqual("psqldef", runner.Calls[0].FileName);
        CollectionAssert.AreEqual(new[] { "-h", "db.test", "-p", "5432", "-U", "me", "--dry-run", "app" }, runner.Calls[0].Arguments.ToArray());
        Assert.AreEqual("secret", runner.Calls[0].Environment["PGPASSWORD"]);
        Assert.IsFalse(runner.Calls[0].Arguments.Contains("secret"));
        StringAssert.Contains(runner.Calls[0].Input, "CREATE TABLE b");
        Assert.HasCount(1, plan.Statements);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task PlanAsync_Sqlite_PassesTheFileAndNoHost()
    {
        var runner = new FakeRunner("");
        var provider = SqldefProvider.ForSqlite(runner);

        var plan = await provider.PlanAsync(_desired, DatabaseTarget.File("app.db"), TestContext.CancellationToken);

        CollectionAssert.AreEqual(new[] { "--dry-run", "app.db" }, runner.Calls[0].Arguments.ToArray());
        Assert.IsTrue(plan.IsEmpty);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ApplyAsync_PlanWithADrop_IsRefusedAndRunsNothing()
    {
        var runner = new FakeRunner("ALTER TABLE a DROP COLUMN n;\n");
        var provider = SqldefProvider.ForSqlServer(runner);
        var plan = await provider.PlanAsync(_desired, _target, TestContext.CancellationToken);
        runner.Calls.Clear();

        var ex = await Assert.ThrowsExactlyAsync<UnsafePlanException>(() => provider.ApplyAsync(_desired, _target, plan, new ApplyOptions(), TestContext.CancellationToken));
        TestContext.WriteLine(ex.Message);

        Assert.IsEmpty(runner.Calls);
        Assert.AreEqual(HazardKind.DataLoss, ex.Hazards.Single().Kind);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ApplyAsync_TypeChange_NeedsItsOwnFlag()
    {
        var runner = new FakeRunner("ALTER TABLE a ALTER COLUMN n TYPE varchar(5);\n");
        var provider = SqldefProvider.ForPostgres(runner);
        var plan = await provider.PlanAsync(_desired, _target, TestContext.CancellationToken);

        await Assert.ThrowsExactlyAsync<UnsafePlanException>(() => provider.ApplyAsync(_desired, _target, plan, new ApplyOptions(AllowDataLoss: true), TestContext.CancellationToken));
        await provider.ApplyAsync(_desired, _target, plan, new ApplyOptions(AllowTypeChange: true), TestContext.CancellationToken);

        Assert.IsFalse(runner.Calls.Last().Arguments.Contains("--enable-drop"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ApplyAsync_PermittedDrop_PassesEnableDrop()
    {
        var runner = new FakeRunner("DROP TABLE old;\n");
        var provider = SqldefProvider.ForPostgres(runner);
        var plan = await provider.PlanAsync(_desired, _target, TestContext.CancellationToken);

        await provider.ApplyAsync(_desired, _target, plan, new ApplyOptions(AllowDataLoss: true), TestContext.CancellationToken);

        CollectionAssert.Contains(runner.Calls.Last().Arguments.ToList(), "--enable-drop");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ApplyAsync_EmptyPlan_RunsNothing()
    {
        var runner = new FakeRunner("");
        var provider = SqldefProvider.ForPostgres(runner);
        var plan = await provider.PlanAsync(_desired, _target, TestContext.CancellationToken);
        runner.Calls.Clear();

        await provider.ApplyAsync(_desired, _target, plan, new ApplyOptions(), TestContext.CancellationToken);

        Assert.IsEmpty(runner.Calls);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task PlanAsync_ToolFails_ThrowsWithTheErrorText()
    {
        var runner = new FakeRunner("") { ExitCode = 1, Error = "syntax error" };
        var provider = SqldefProvider.ForPostgres(runner);

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => provider.PlanAsync(_desired, _target, TestContext.CancellationToken));

        StringAssert.Contains(ex.Message, "syntax error");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Factory_ResolvesByEngineKey_AndRefusesUnknownEngines()
    {
        var services = new ServiceCollection().AddDesiredStateDatabase();
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<DesiredStateProviderFactory>();

        Assert.AreEqual("postgres", factory.Create("Postgres").Engine);
        Assert.AreEqual("sqlserver", factory.Create("sqlserver").Engine);
        Assert.AreEqual("sqlite", factory.Create("sqlite").Engine);
        Assert.ThrowsExactly<NotSupportedException>(() => factory.Create("oracle"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void FromDirectory_ReadsSqlFilesInNameOrder()
    {
        using var folder = new TempDirectoryHandle();
        File.WriteAllText(Path.Combine(folder.DirectoryPath, "020_b.sql"), "B");
        File.WriteAllText(Path.Combine(folder.DirectoryPath, "001_a.sql"), "A");
        File.WriteAllText(Path.Combine(folder.DirectoryPath, "notes.txt"), "x");

        var desired = DesiredSchema.FromDirectory(folder.DirectoryPath);

        CollectionAssert.AreEqual(new[] { "001_a.sql", "020_b.sql" }, desired.Scripts.Select(s => s.Name).ToArray());
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task Sqlite_WithTheRealTool_PlansACreateAndThenNothing()
    {
        if (!IsOnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var provider = SqldefProvider.ForSqlite(new ProcessToolRunner());
        var desired = new DesiredSchema([new DesiredScript("001.sql", "CREATE TABLE t (id integer primary key, name text);")]);
        var target = DatabaseTarget.File(Path.Combine(folder.DirectoryPath, "app.db"));

        var plan = await provider.PlanAsync(desired, target, TestContext.CancellationToken);
        await provider.ApplyAsync(desired, target, plan, new ApplyOptions(), TestContext.CancellationToken);
        var again = await provider.PlanAsync(desired, target, TestContext.CancellationToken);
        TestContext.WriteLine($"first={plan.Statements.Count} second={again.Statements.Count}");

        Assert.IsFalse(plan.IsEmpty);
        Assert.IsTrue(again.IsEmpty);
    }

    private static bool IsOnPath(string tool) =>
        (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator)
            .Any(d => d.Length > 0 && (File.Exists(Path.Combine(d, tool)) || File.Exists(Path.Combine(d, tool + ".exe"))));

    private sealed record Call(string FileName, IReadOnlyList<string> Arguments, string Input, IReadOnlyDictionary<string, string> Environment);

    private sealed class FakeRunner(string output) : IToolRunner
    {
        public List<Call> Calls { get; } = [];

        public int ExitCode { get; init; }

        public string Error { get; init; } = string.Empty;

        public Task<ToolResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string standardInput, IReadOnlyDictionary<string, string> environment, CancellationToken cancellationToken = default)
        {
            Calls.Add(new Call(fileName, arguments, standardInput, environment));
            return Task.FromResult(new ToolResult(ExitCode, output, Error));
        }
    }

}
