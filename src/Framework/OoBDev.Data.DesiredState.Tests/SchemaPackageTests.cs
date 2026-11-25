using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public sealed class SchemaPackageTests
{
    public required TestContext TestContext { get; set; }

    private const string V1 = "CREATE TABLE \"Notes\" (\"Id\" integer NOT NULL PRIMARY KEY, \"Text\" text NOT NULL);";
    private const string V2 = "CREATE TABLE \"Notes\" (\"Id\" integer NOT NULL PRIMARY KEY, \"Body\" text NOT NULL);";
    private static readonly RefactorEntry Rename = new("2026-11-24-notes-text-to-body", "Notes", "ALTER TABLE \"Notes\" RENAME COLUMN \"Text\" TO \"Body\";");

    private static SchemaPackage Package(string schema, params RefactorEntry[] refactors) =>
        new("notes", "1.0", new DesiredSchema([new DesiredScript("010_notes.sql", schema), new DesiredScript("900_history.sql", AdoAppliedScriptStore.SchemaScript)]),
            new DesiredSchema([]), new DesiredSchema([new DesiredScript("010_seed.once.sql", "INSERT INTO \"Notes\" VALUES (99, 'seed');")]), refactors);

    private static SqliteConnection Open(DatabaseTarget t) => new($"Data Source={t.Database};Pooling=False");

    private static ServiceProvider Services() => new ServiceCollection()
        .AddDesiredStateDatabase()
        .AddSingleton<IScriptExecutor>(new AdoScriptExecutor(Open))
        .AddSingleton<IAppliedScriptStore>(new AdoAppliedScriptStore(Open))
        .BuildServiceProvider();

    [TestMethod]
    [TestCategory("Unit")]
    public void Format_RoundTripsThroughAZip_AndKeepsTheHash()
    {
        var original = Package(V1, Rename);
        using var stream = new MemoryStream();

        SchemaPackageFormat.Write(original, stream);
        stream.Position = 0;
        var read = SchemaPackageFormat.Read(stream);

        Assert.AreEqual("notes", read.Name);
        Assert.AreEqual(2, read.Schema.Scripts.Count);
        Assert.AreEqual(1, read.PostDeploy.Scripts.Count);
        Assert.AreEqual(Rename, read.Refactors.Single());
        Assert.AreEqual(original.Hash, read.Hash);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Hash_ChangesWhenAScriptChanges() =>
        Assert.AreNotEqual(Package(V1).Hash, Package(V2).Hash);

    [TestMethod]
    [TestCategory("Unit")]
    public void Read_WithoutManifest_Throws()
    {
        using var stream = new MemoryStream();
        using (var zip = new global::System.IO.Compression.ZipArchive(stream, global::System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
        {
            zip.CreateEntry("schema/a.sql");
        }

        stream.Position = 0;

        Assert.ThrowsExactly<InvalidDataException>(() => SchemaPackageFormat.Read(stream));
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Deploy_RenameScenario_KeepsTheDataAndARepeatAppliesNothing()
    {
        // Stage
        if (!ProviderTests.IsOnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var target = DatabaseTarget.File(Path.Combine(folder.DirectoryPath, "app.db"));
        using var services = Services();
        var deployer = services.GetRequiredService<SchemaPackageDeployer>();

        // Test
        var first = await deployer.DeployAsync(Package(V1), SqldefProvider.Sqlite, target, cancellationToken: TestContext.CancellationToken);
        await Exec(target, "INSERT INTO \"Notes\" VALUES (1, 'keep me');");
        var upgrade = await deployer.DeployAsync(Package(V2, Rename), SqldefProvider.Sqlite, target, cancellationToken: TestContext.CancellationToken);
        var repeat = await deployer.DeployAsync(Package(V2, Rename), SqldefProvider.Sqlite, target, cancellationToken: TestContext.CancellationToken);
        TestContext.WriteLine($"upgrade ran={string.Join(',', upgrade.RefactorsRun)} plan={upgrade.Plan.Statements.Count} repeat plan={repeat.Plan.Statements.Count}");

        // Assert
        Assert.IsTrue(first.FirstDeploy);
        CollectionAssert.AreEqual(new[] { Rename.Id }, upgrade.RefactorsRun.ToArray());
        Assert.AreEqual("keep me", await Scalar(target, "SELECT \"Body\" FROM \"Notes\" WHERE \"Id\" = 1"));
        Assert.AreEqual("1", await Scalar(target, "SELECT COUNT(*) FROM \"Notes\" WHERE \"Id\" = 99"));
        Assert.AreEqual(0, repeat.RefactorsRun.Count);
        Assert.IsTrue(repeat.Plan.IsEmpty);
        Assert.IsFalse(repeat.FirstDeploy);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Deploy_RefactorOnAFreshDatabase_IsRecordedWithoutRunning()
    {
        // Stage
        if (!ProviderTests.IsOnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var target = DatabaseTarget.File(Path.Combine(folder.DirectoryPath, "app.db"));
        using var services = Services();
        var deployer = services.GetRequiredService<SchemaPackageDeployer>();

        // Test
        var result = await deployer.DeployAsync(Package(V2, Rename), SqldefProvider.Sqlite, target, cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(0, result.RefactorsRun.Count);
        CollectionAssert.AreEqual(new[] { Rename.Id }, result.RefactorsRecorded.ToArray());
        Assert.AreEqual("1", await Scalar(target, "SELECT COUNT(*) FROM \"Notes\""));
    }

    private static async Task Exec(DatabaseTarget target, string sql)
    {
        await using var connection = Open(target);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<string> Scalar(DatabaseTarget target, string sql)
    {
        await using var connection = Open(target);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToString(await command.ExecuteScalarAsync(), global::System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
