using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class QueryEngineTests
{
    private sealed class Allow(bool allowed) : IQueryAuthorizer
    {
        public bool IsAllowed(string right) => allowed;
    }

    private static QueryEngine Engine(FakeExecutor executor, IQueryAuthorizer? authorizer = null) =>
        new(Support.Registry, new TestDialect(), executor, Support.Codec(), Support.Options(), NullLogger<QueryEngine>.Instance, authorizer);

    private static IReadOnlyDictionary<string, object?> Row(string name, DateTime taken, int id) =>
        new Dictionary<string, object?> { ["name"] = name, ["taken"] = taken, ["__s0"] = taken, ["__s1"] = id };

    [TestMethod]
    public async Task QueryAsync_MoreRowsThanTop_ReturnsNextCursorAndStripsSortColumns()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([Row("a", new DateTime(2024, 1, 3), 3), Row("b", new DateTime(2024, 1, 2), 2), Row("c", new DateTime(2024, 1, 1), 1)]);

        var page = await Engine(ex).QueryAsync(new QueryRequest { From = "photos", Top = 2 });

        Assert.AreEqual(2, page.Rows.Count);
        Assert.IsNotNull(page.Next);
        Assert.IsFalse(page.Rows[0].ContainsKey("__s0"));
    }

    [TestMethod]
    public async Task QueryAsync_NextCursor_ContinuesWithKeysetCondition()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([Row("a", new DateTime(2024, 1, 3), 3), Row("b", new DateTime(2024, 1, 2), 2), Row("c", new DateTime(2024, 1, 1), 1)]);
        var engine = Engine(ex);
        var first = await engine.QueryAsync(new QueryRequest { From = "photos", Top = 2 });

        await engine.QueryAsync(new QueryRequest { From = "photos", Top = 2, After = first.Next });

        Assert.AreNotEqual(ex.Calls[0].Sql, ex.Calls[1].Sql);
    }

    [TestMethod]
    public async Task QueryAsync_LastPage_HasNoCursor()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([Row("a", new DateTime(2024, 1, 3), 3)]);

        var page = await Engine(ex).QueryAsync(new QueryRequest { From = "photos", Top = 5 });

        Assert.IsNull(page.Next);
    }

    [TestMethod]
    public async Task QueryAsync_Count_RunsCountQuery()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([]);
        ex.Results.Enqueue([new Dictionary<string, object?> { ["count"] = 42L }]);

        var page = await Engine(ex).QueryAsync(new QueryRequest { From = "photos", Count = true });

        Assert.AreEqual(42, page.Count);
        Assert.AreEqual(2, ex.Calls.Count);
    }

    [TestMethod]
    public async Task QueryAsync_KindWithRightAndNoAuthorizer_Is403()
    {
        var ex = await Assert.ThrowsExactlyAsync<QueryException>(() => Engine(new FakeExecutor()).QueryAsync(new QueryRequest { From = "admin" }));

        Assert.AreEqual(403, ex.Status);
    }

    [TestMethod]
    public async Task QueryAsync_KindWithRightDenied_Is403AndNothingRuns()
    {
        var executor = new FakeExecutor();

        await Assert.ThrowsExactlyAsync<QueryException>(() => Engine(executor, new Allow(false)).QueryAsync(new QueryRequest { From = "admin" }));

        Assert.AreEqual(0, executor.Calls.Count);
    }

    [TestMethod]
    public async Task QueryAsync_KindWithRightAllowed_Runs()
    {
        var executor = new FakeExecutor();

        await Engine(executor, new Allow(true)).QueryAsync(new QueryRequest { From = "admin" });

        Assert.AreEqual(1, executor.Calls.Count);
    }

    [TestMethod]
    public async Task QueryAsync_Typed_MapsRowsToRowClass()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([new Dictionary<string, object?> { ["name"] = "a", ["size"] = 5, ["taken"] = new DateTime(2024, 1, 1), ["rating"] = 3L, ["__s0"] = 1, ["__s1"] = 1 }]);

        var page = await Engine(ex).QueryAsync<PhotoRow>(new QueryRequest { From = "photos" });

        Assert.AreEqual("a", page.Rows[0].Name);
        Assert.AreEqual(5L, page.Rows[0].Size);
        Assert.AreEqual(3, page.Rows[0].Rating);
        Assert.AreEqual(new DateTime(2024, 1, 1), page.Rows[0].Taken);
    }

    [TestMethod]
    public async Task QueryAsync_NullRequest_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => Engine(new FakeExecutor()).QueryAsync(null!));
    }

    [TestMethod]
    public async Task QueryAsync_FacetsShape_ReturnsRowsWithoutCursor()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([new Dictionary<string, object?> { ["value"] = "x", ["count"] = 2L }]);

        var page = await Engine(ex).QueryAsync<FacetCount>(new QueryRequest { From = "photos", Shape = "facets", By = ["album"] });

        Assert.AreEqual(1, page.Rows.Count);
        Assert.IsNull(page.Next);
        Assert.AreEqual(2, page.Rows.Single().Count);
    }
}
