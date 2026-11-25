using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Data.DesiredState;
using OoBDev.System.IO;

namespace OoBDev.DesiredState.Cli.Tests;

[TestClass]
public sealed class DesiredStateCommandTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Plan_PendingSafeChange_Exits3AndPrintsStatements()
    {
        using var folder = Schema();
        var (code, output, _, _) = await RunAsync(["plan", "--engine", "fake", "--scripts", folder.DirectoryPath, "--database", "app"], ["CREATE TABLE a (id int);"]);
        TestContext.WriteLine(output);

        Assert.AreEqual(3, code);
        StringAssert.Contains(output, "CREATE TABLE a");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Plan_NoChanges_Exits0()
    {
        using var folder = Schema();
        var (code, output, _, _) = await RunAsync(["plan", "--engine", "fake", "--scripts", folder.DirectoryPath, "--database", "app"], []);

        Assert.AreEqual(0, code);
        StringAssert.Contains(output, "No changes.");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Plan_WithHazard_Exits1()
    {
        using var folder = Schema();
        var (code, output, _, _) = await RunAsync(["plan", "--engine", "fake", "--scripts", folder.DirectoryPath, "--database", "app"], ["DROP TABLE a;"]);

        Assert.AreEqual(1, code);
        StringAssert.Contains(output, "HAZARD DataLoss");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Apply_DropWithoutFlag_IsRefusedWithExit1()
    {
        using var folder = Schema();
        var (code, _, error, fake) = await RunAsync(["apply", "--engine", "fake", "--scripts", folder.DirectoryPath, "--database", "app"], ["DROP TABLE a;"]);
        TestContext.WriteLine(error);

        Assert.AreEqual(1, code);
        Assert.IsFalse(fake.Applied);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Apply_DropWithFlag_Applies()
    {
        using var folder = Schema();
        var (code, _, _, fake) = await RunAsync(["apply", "--engine", "fake", "--scripts", folder.DirectoryPath, "--database", "app", "--allow-data-loss", "true"], ["DROP TABLE a;"]);

        Assert.AreEqual(0, code);
        Assert.IsTrue(fake.Applied);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Apply_PasswordComesFromTheEnvironmentVariable()
    {
        using var folder = Schema();
        var (_, _, _, fake) = await RunAsync(["apply", "--engine", "fake", "--scripts", folder.DirectoryPath, "--database", "app", "--password-env", "PW"], ["CREATE TABLE a (id int);"], name => name == "PW" ? "s3cret" : null);

        Assert.AreEqual("s3cret", fake.Target?.Password);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("")]
    [DataRow("bogus")]
    public async Task Run_BadVerb_Exits2WithUsage(string verb)
    {
        var (code, _, error, _) = await RunAsync(verb.Length == 0 ? [] : [verb], []);

        Assert.AreEqual(2, code);
        StringAssert.Contains(error, "desired-state plan");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Plan_MissingEngine_Exits2()
    {
        var (code, _, error, _) = await RunAsync(["plan", "--scripts", ".", "--database", "x"], []);

        Assert.AreEqual(2, code);
        StringAssert.Contains(error, "--engine");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Plan_UnknownEngine_Exits2()
    {
        using var folder = Schema();
        var (code, _, _, _) = await RunAsync(["plan", "--engine", "mongo", "--scripts", folder.DirectoryPath, "--database", "x"], []);

        Assert.AreEqual(2, code);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Pack_SourceFolder_WritesAZipThatReadsBack()
    {
        using var source = Schema();
        using var target = new TempDirectoryHandle();
        Directory.CreateDirectory(Path.Combine(source.DirectoryPath, "schema"));
        File.Move(Path.Combine(source.DirectoryPath, "001.sql"), Path.Combine(source.DirectoryPath, "schema", "001.sql"));
        File.WriteAllText(Path.Combine(source.DirectoryPath, "refactorlog.json"), """[{"id":"r1","table":"a","sql":"ALTER TABLE a RENAME COLUMN x TO y;"}]""");
        var zip = Path.Combine(target.DirectoryPath, "p.zip");

        var (code, output, _, _) = await RunAsync(["pack", "--source", source.DirectoryPath, "--name", "app", "--version", "1.0", "--out", zip], []);
        TestContext.WriteLine(output);
        using var stream = File.OpenRead(zip);
        var package = SchemaPackageFormat.Read(stream);

        Assert.AreEqual(0, code);
        Assert.AreEqual(1, package.Schema.Scripts.Count);
        Assert.AreEqual("r1", package.Refactors.Single().Id);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Deploy_PackageOnSqlite_AppliesItAndTheRepeatChangesNothing()
    {
        if (!(Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator).Any(d => d.Length > 0 && (File.Exists(Path.Combine(d, "sqlite3def")) || File.Exists(Path.Combine(d, "sqlite3def.exe")))))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var work = new TempDirectoryHandle();
        var zip = Path.Combine(work.DirectoryPath, "p.zip");
        var package = new SchemaPackage("app", "1.0", new DesiredSchema([new DesiredScript("001.sql", "CREATE TABLE \"a\" (\"id\" integer NOT NULL PRIMARY KEY);")]), new DesiredSchema([]), new DesiredSchema([]), []);
        await using (var stream = File.Create(zip))
        {
            SchemaPackageFormat.Write(package, stream);
        }

        var db = Path.Combine(work.DirectoryPath, "app.db");
        using var services = new ServiceCollection().AddDesiredStateDatabase().BuildServiceProvider();
        var factory = services.GetRequiredService<DesiredStateProviderFactory>();
        string[] args = ["deploy", "--package", zip, "--engine", "sqlite", "--database", db];
        static DbConnection Connect(string engine, DatabaseTarget t) => new SqliteConnection($"Data Source={t.Database};Pooling=False");

        using var first = new StringWriter();
        using var second = new StringWriter();
        var code1 = await DesiredStateCommand.RunAsync(args, factory, _ => null, first, TextWriter.Null, Connect, TestContext.CancellationToken);
        var code2 = await DesiredStateCommand.RunAsync(args, factory, _ => null, second, TextWriter.Null, Connect, TestContext.CancellationToken);
        TestContext.WriteLine(first + second.ToString());

        Assert.AreEqual(0, code1);
        Assert.AreEqual(0, code2);
        Assert.DoesNotContain("0 statements applied", first.ToString());
        StringAssert.Contains(second.ToString(), "0 statements applied");
    }

    private static TempDirectoryHandle Schema()
    {
        var folder = new TempDirectoryHandle();
        File.WriteAllText(Path.Combine(folder.DirectoryPath, "001.sql"), "CREATE TABLE a (id int);");
        return folder;
    }

    private static async Task<(int Code, string Output, string Error, FakeProvider Fake)> RunAsync(string[] args, string[] statements, Func<string, string?>? environment = null)
    {
        var fake = new FakeProvider(statements);
        using var provider = new ServiceCollection().AddKeyedSingleton<IDesiredStateProvider>("fake", fake).BuildServiceProvider();
        using var output = new StringWriter();
        using var error = new StringWriter();
        var code = await DesiredStateCommand.RunAsync(args, new DesiredStateProviderFactory(provider), environment ?? (_ => null), output, error);
        return (code, output.ToString(), error.ToString(), fake);
    }

    private sealed class FakeProvider(string[] statements) : IDesiredStateProvider
    {
        public string Engine => "fake";

        public bool Applied { get; private set; }

        public DatabaseTarget? Target { get; private set; }

        public Task<SchemaPlan> PlanAsync(DesiredSchema desired, DatabaseTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(new SchemaPlan(Engine, statements, PlanAnalyzer.Analyze(statements), []));

        public Task ApplyAsync(DesiredSchema desired, DatabaseTarget target, SchemaPlan plan, ApplyOptions options, CancellationToken cancellationToken = default)
        {
            var refused = new List<Hazard>();
            foreach (var hazard in plan.Hazards)
            {
                if (!options.Permits(hazard))
                {
                    refused.Add(hazard);
                }
            }

            if (refused.Count > 0)
            {
                throw new UnsafePlanException(refused);
            }

            Applied = true;
            Target = target;
            return Task.CompletedTask;
        }
    }
}
