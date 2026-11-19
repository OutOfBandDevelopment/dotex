using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class HintTests
{
    public required TestContext TestContext { get; set; }

    private static QueryEngine Engine(FakeExecutor executor) =>
        new(Support.Registry, new TestDialect(), executor, Support.Codec(), Support.Options(), NullLogger<QueryEngine>.Instance);

    private static IReadOnlyDictionary<string, object?> Row(string name, long id) =>
        new Dictionary<string, object?> { ["name"] = name, ["__s0"] = name, ["__s1"] = id };

    [TestMethod]
    public async Task QueryAsync_SelectHas_RunsOneBatchedStatementForThePageKeysAsync()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([Row("a", 1), Row("b", 2), Row("c", 3)]);
        ex.Results.Enqueue([
            new Dictionary<string, object?> { ["__key"] = 1L, ["copies"] = true, ["burst"] = false },
            new Dictionary<string, object?> { ["__key"] = 2L, ["copies"] = false, ["burst"] = true },
        ]);

        var page = await Engine(ex).QueryAsync<HintedRow>(new QueryRequest { From = "hinted", Select = ["has"], Top = 2 });

        TestContext.WriteLine("Statements: {0}; hint SQL: {1}", ex.Calls.Count, ex.Calls[1].Sql);
        Assert.AreEqual(2, ex.Calls.Count);
        StringAssert.Contains(ex.Calls[1].Sql, "c.id <> f.id");
        CollectionAssert.AreEqual(new long[] { 1, 2 }, (long[])ex.Calls[1].Parameters.Values.Single()!, "only the keys of the page, not the look-ahead row");
        Assert.IsTrue(page.Rows[0].Has!.Copies);
        Assert.IsFalse(page.Rows[0].Has!.Burst);
        Assert.IsTrue(page.Rows[1].Has!.Burst);
    }

    [TestMethod]
    public async Task QueryAsync_RowWithoutHintResult_GetsAllFalseAsync()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([Row("a", 1)]);

        var page = await Engine(ex).QueryAsync<HintedRow>(new QueryRequest { From = "hinted", Select = ["has"] });

        Assert.IsFalse(page.Rows[0].Has!.Copies);
        Assert.IsFalse(page.Rows[0].Has!.Burst);
    }

    [TestMethod]
    public async Task QueryAsync_WithoutSelectHas_RunsNoHintStatementAsync()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([Row("a", 1)]);

        var page = await Engine(ex).QueryAsync<HintedRow>(new QueryRequest { From = "hinted" });

        Assert.AreEqual(1, ex.Calls.Count);
        Assert.IsNull(page.Rows[0].Has);
    }

    [TestMethod]
    public void Describe_ListsTheHints()
    {
        var kind = Describer.Describe(Support.Registry.Get("hinted"));

        CollectionAssert.AreEqual(new[] { "burst", "copies" }, kind.Hints);
    }
}
