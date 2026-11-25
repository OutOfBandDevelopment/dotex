using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public sealed class EfDesiredStateDeployerTests
{
    public required TestContext TestContext { get; set; }

    private static DbContextOptions<T> Sqlite<T>(string connectionString)
        where T : DbContext => new DbContextOptionsBuilder<T>().UseSqlite(connectionString).Options;

    private static string ConnectionString(string file) => $"Data Source={file};Pooling=False";

    [TestMethod]
    [TestCategory("Unit")]
    public void EngineOf_SqliteContext_MapsToTheEngineKey()
    {
        using var context = new NoteContext(Sqlite<NoteContext>("Data Source=a.db"));

        Assert.AreEqual(SqldefProvider.Sqlite, DbContextTarget.EngineOf(context));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TargetOf_SqliteFile_UsesTheFilePath()
    {
        using var context = new NoteContext(Sqlite<NoteContext>("Data Source=app.db"));

        var target = DbContextTarget.TargetOf(context, SqldefProvider.Sqlite);

        Assert.AreEqual("app.db", target.Database);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("Data Source=:memory:")]
    [DataRow("Data Source=x;Mode=Memory;Cache=Shared")]
    public void TargetOf_SqliteInMemory_IsRefused(string connectionString)
    {
        using var context = new NoteContext(Sqlite<NoteContext>(connectionString));

        Assert.ThrowsExactly<NotSupportedException>(() => DbContextTarget.TargetOf(context, SqldefProvider.Sqlite));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ScriptOf_ContextWithSeedData_IsRefused()
    {
        using var context = new SeededContext(Sqlite<SeededContext>("Data Source=a.db"));

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => DbContextTarget.ScriptOf(context));

        StringAssert.Contains(ex.Message, "once.sql");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ScriptOf_Context_ReturnsTheCreateScript()
    {
        using var context = new NoteContext(Sqlite<NoteContext>("Data Source=a.db"));

        var script = DbContextTarget.ScriptOf(context);

        StringAssert.Contains(script, "CREATE TABLE \"Notes\"");
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task EnsureAsync_SqliteFile_CreatesTheTablesAndTheSecondRunChangesNothing()
    {
        // Stage
        if (!ProviderTests.IsOnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var connectionString = ConnectionString(Path.Combine(folder.DirectoryPath, "app.db"));
        using var services = new ServiceCollection().AddDesiredStateDatabase().BuildServiceProvider();
        var deployer = services.GetRequiredService<EfDesiredStateDeployer>();
        await using var context = new NoteContext(Sqlite<NoteContext>(connectionString));

        // Test
        var first = await deployer.EnsureAsync(context, cancellationToken: TestContext.CancellationToken);
        var second = await deployer.EnsureAsync(context, cancellationToken: TestContext.CancellationToken);
        context.Notes.Add(new Note { Id = 1, Text = "hello" });
        await context.SaveChangesAsync(TestContext.CancellationToken);
        TestContext.WriteLine($"first={first.Plan.Statements.Count} applied={first.Applied} second={second.Plan.Statements.Count}");

        // Assert
        Assert.IsTrue(first.Applied);
        Assert.IsFalse(second.Applied);
        Assert.AreEqual(1, await context.Notes.CountAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task EnsureAsync_ModelDropsAColumn_IsRefusedUntilDataLossIsPermitted()
    {
        // Stage
        if (!ProviderTests.IsOnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var connectionString = ConnectionString(Path.Combine(folder.DirectoryPath, "app.db"));
        using var services = new ServiceCollection().AddDesiredStateDatabase().BuildServiceProvider();
        var deployer = services.GetRequiredService<EfDesiredStateDeployer>();
        await using (var wide = new NoteContext(Sqlite<NoteContext>(connectionString)))
        {
            await deployer.EnsureAsync(wide, cancellationToken: TestContext.CancellationToken);
        }

        await using var narrow = new NarrowNoteContext(Sqlite<NarrowNoteContext>(connectionString));

        // Test and Assert
        var refused = await deployer.EnsureAsync(narrow, cancellationToken: TestContext.CancellationToken);
        Assert.IsFalse(refused.Applied);
        Assert.AreEqual(1, refused.Plan.Skipped.Count);
        var allowed = await deployer.EnsureAsync(narrow, new ApplyOptions(AllowDataLoss: true), cancellationToken: TestContext.CancellationToken);
        Assert.IsTrue(allowed.Applied);
        var after = await deployer.EnsureAsync(narrow, cancellationToken: TestContext.CancellationToken);
        TestContext.WriteLine($"skipped={refused.Plan.Skipped.Count} after-skipped={after.Plan.Skipped.Count}");
        Assert.AreEqual(0, after.Plan.Skipped.Count);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task EnsureAsync_OnceScript_RunsAfterTheSchemaOnlyOnce()
    {
        // Stage
        if (!ProviderTests.IsOnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var connectionString = ConnectionString(Path.Combine(folder.DirectoryPath, "app.db"));
        using var services = new ServiceCollection()
            .AddDesiredStateDatabase()
            .AddSingleton<IScriptExecutor>(new AdoScriptExecutor(t => new SqliteConnection($"Data Source={t.Database};Pooling=False")))
            .BuildServiceProvider();
        var deployer = services.GetRequiredService<EfDesiredStateDeployer>();
        var scripts = new DesiredSchema([new DesiredScript("010_seed.once.sql", "INSERT INTO Notes (Id, Text) VALUES (7, 'seed');")]);
        await using var context = new NoteContext(Sqlite<NoteContext>(connectionString));

        // Test
        var first = await deployer.EnsureAsync(context, ensureScripts: scripts, cancellationToken: TestContext.CancellationToken);
        var second = await deployer.EnsureAsync(context, ensureScripts: scripts, cancellationToken: TestContext.CancellationToken);

        // Assert
        CollectionAssert.AreEqual(new[] { "010_seed.once.sql" }, first.Ensure!.Ran.ToArray());
        CollectionAssert.AreEqual(new[] { "010_seed.once.sql" }, second.Ensure!.Skipped.ToArray());
        Assert.AreEqual(1, await context.Notes.CountAsync(TestContext.CancellationToken));
    }

    private sealed class Note
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public string? Extra { get; set; }
    }

    private sealed class NoteContext(DbContextOptions<NoteContext> options) : DbContext(options)
    {
        public DbSet<Note> Notes => Set<Note>();
    }

    private sealed class NarrowNoteContext(DbContextOptions<NarrowNoteContext> options) : DbContext(options)
    {
        public DbSet<Note> Notes => Set<Note>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Note>().Ignore(n => n.Extra);
    }

    private sealed class SeededContext(DbContextOptions<SeededContext> options) : DbContext(options)
    {
        public DbSet<Note> Notes => Set<Note>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Note>().HasData(new Note { Id = 1, Text = "x" });
    }
}
