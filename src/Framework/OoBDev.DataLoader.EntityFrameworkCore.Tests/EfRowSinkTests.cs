using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DataLoader.EntityFrameworkCore.Tests;

[TestCategory("Simulate")]
[TestClass]
public sealed class EfRowSinkTests
{
    public TestContext TestContext { get; set; } = null!;

    private SqliteRowsDatabase database = null!;

    [TestInitialize]
    public void Setup() => database = new SqliteRowsDatabase();

    [TestCleanup]
    public async Task Cleanup() => await database.DisposeAsync();

    private static RowContext Row(string id, string name) => new(1, new global::System.Collections.Generic.Dictionary<string, string?> { ["id"] = id, ["name"] = name });

    [TestMethod]
    public async Task Write_NewThenDuplicate_FollowsThePolicy()
    {
        var sink = new EfRowSink(database.Factory, "people", "id");

        Assert.AreEqual(WriteResult.Written, await sink.WriteAsync(Row("1", "Ann"), DuplicatePolicy.Skip, CancellationToken.None));
        Assert.AreEqual(WriteResult.Skipped, await sink.WriteAsync(Row("1", "Bo"), DuplicatePolicy.Skip, CancellationToken.None));
        Assert.AreEqual("Ann", (await sink.GetAsync("1", CancellationToken.None))!["name"]);
        Assert.AreEqual(WriteResult.Duplicate, await sink.WriteAsync(Row("1", "Bo"), DuplicatePolicy.Fail, CancellationToken.None));
        Assert.AreEqual(WriteResult.Replaced, await sink.WriteAsync(Row("1", "Bo"), DuplicatePolicy.Replace, CancellationToken.None));
        Assert.AreEqual("Bo", (await sink.GetAsync("1", CancellationToken.None))!["name"]);
    }

    [TestMethod]
    public async Task Write_SameKeyInDifferentDatasets_AreSeparateRows()
    {
        var a = new EfRowSink(database.Factory, "a", "id");
        var b = new EfRowSink(database.Factory, "b", "id");

        Assert.AreEqual(WriteResult.Written, await a.WriteAsync(Row("1", "Ann"), DuplicatePolicy.Fail, CancellationToken.None));
        Assert.AreEqual(WriteResult.Written, await b.WriteAsync(Row("1", "Bo"), DuplicatePolicy.Fail, CancellationToken.None));
        Assert.IsNull(await a.GetAsync("2", CancellationToken.None));
    }

    [TestMethod]
    public async Task Load_CsvThroughTheLoaderIntoTheDatabase()
    {
        var services = new ServiceCollection();
        services.TryAddDataLoader();
        services.AddSingleton<IRowSink>(new EfRowSink(database.Factory, "people", "id"));
        using var provider = services.BuildServiceProvider();
        using var csv = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Ann\n2,Bo\n1,Dup\n"));

        var report = await provider.GetRequiredService<ILoader>().LoadAsync(csv, "csv", new LoadOptions(), CancellationToken.None);
        TestContext.WriteLine($"read={report.Read} written={report.Written} skipped={report.Skipped}");

        Assert.AreEqual(2, report.Written);
        Assert.AreEqual(1, report.Skipped);
    }

    [TestMethod]
    public async Task Load_BatchedCsv_MatchesRowByRowAndWritesInBatches()
    {
        var services = new ServiceCollection();
        services.TryAddDataLoader();
        var sink = new EfRowSink(database.Factory, "people", "id");
        services.AddSingleton<IRowSink>(sink);
        using var provider = services.BuildServiceProvider();
        using var csv = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Ann\n2,Bo\n1,Dup\n3,Cy\n4,Di\n"));

        var report = await provider.GetRequiredService<ILoader>().LoadAsync(csv, "csv", new LoadOptions { BatchSize = 2 }, CancellationToken.None);
        TestContext.WriteLine($"read={report.Read} written={report.Written} skipped={report.Skipped}");

        Assert.AreEqual(5, report.Read);
        Assert.AreEqual(4, report.Written);
        Assert.AreEqual(1, report.Skipped);
        Assert.AreEqual("Ann", (await sink.GetAsync("1", CancellationToken.None))!["name"]);
        Assert.AreEqual("Di", (await sink.GetAsync("4", CancellationToken.None))!["name"]);
    }

    [TestMethod]
    public async Task WriteBatch_ReplaceWithRepeatInBatch_LastRowWins()
    {
        var sink = new EfRowSink(database.Factory, "people", "id");
        await sink.WriteAsync(Row("1", "Ann"), DuplicatePolicy.Skip, CancellationToken.None);

        var results = await sink.WriteBatchAsync([Row("1", "Bo"), Row("2", "Cy"), Row("2", "Di")], DuplicatePolicy.Replace, CancellationToken.None);

        CollectionAssert.AreEqual(new[] { WriteResult.Replaced, WriteResult.Written, WriteResult.Replaced }, results.ToArray());
        Assert.AreEqual("Bo", (await sink.GetAsync("1", CancellationToken.None))!["name"]);
        Assert.AreEqual("Di", (await sink.GetAsync("2", CancellationToken.None))!["name"]);
    }
}
