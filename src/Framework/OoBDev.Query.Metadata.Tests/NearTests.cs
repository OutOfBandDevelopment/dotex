using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class NearTests
{
    public required TestContext TestContext { get; set; }

    private static QueryEngine Engine(FakeExecutor executor) =>
        new(Support.Registry, new TestDialect(), executor, Support.Codec(), Support.Options(), NullLogger<QueryEngine>.Instance);

    [TestMethod]
    public async Task QueryAsync_NearGeo_SortsByDistanceAndBindsThePositionAsync()
    {
        var ex = new FakeExecutor();

        await Engine(ex).QueryAsync<SpaceRow>(new QueryRequest { From = "spots", Near = new NearClause("geo", Lat: 51.5, Lon: -0.1, Max: 25) });

        var sql = ex.Calls[0].Sql;
        TestContext.WriteLine("SQL: {0}", sql);
        StringAssert.Contains(sql, "HAV(s.lat,s.lon,@p0,@p1) AS \"distance\"");
        StringAssert.Contains(sql, "s.lat IS NOT NULL AND s.lon IS NOT NULL");
        StringAssert.Contains(sql, "<= @p2");
        StringAssert.Contains(sql, "ORDER BY HAV(s.lat,s.lon,@p0,@p1) ASC");
        Assert.AreEqual(51.5, ex.Calls[0].Parameters["p0"]);
        Assert.AreEqual(25.0, ex.Calls[0].Parameters["p2"]);
    }

    [TestMethod]
    public async Task QueryAsync_NearVector_JoinsTheTableAndBindsTheAnchorRefAsync()
    {
        var ex = new FakeExecutor();

        await Engine(ex).QueryAsync<SpaceRow>(new QueryRequest { From = "spots", Near = new NearClause("similar", Ref: "42") });

        var sql = ex.Calls[0].Sql;
        TestContext.WriteLine("SQL: {0}", sql);
        StringAssert.Contains(sql, "JOIN spot_vectors sv ON sv.spot_id = s.id");
        StringAssert.Contains(sql, "CAST(@p0 AS bigint)");
        Assert.AreEqual("42", ex.Calls[0].Parameters["p0"]);
        Assert.AreEqual(0.5, ex.Calls[0].Parameters["p1"], "the declared default maximum applies");
        Assert.IsFalse(sql.Contains("'42'"), "the anchor is bound");
    }

    [TestMethod]
    public async Task QueryAsync_NearVectorWithCandidates_TakesTheNearestFromTheTableFirstAsync()
    {
        var ex = new FakeExecutor();

        await Engine(ex).QueryAsync<IndexedSpaceRow>(new QueryRequest { From = "indexed", Near = new NearClause("similar", Ref: "7"), Filters = Support.Filters("{\"name\":\"a\"}") });

        var sql = ex.Calls[0].Sql;
        TestContext.WriteLine("SQL: {0}", sql);
        StringAssert.Contains(sql, "JOIN (SELECT x.spot_id AS id, VEC(x.vec,");
        StringAssert.Contains(sql, " FROM spot_vectors x ORDER BY VEC(x.vec,");
        StringAssert.Contains(sql, "LIMIT 200) sv ON sv.id = s.id");
        StringAssert.Contains(sql, "sv.d AS \"distance\"");
        StringAssert.Contains(sql, "ORDER BY sv.d ASC");
    }

    [TestMethod]
    public async Task QueryAsync_NearThenPage_CursorCarriesTheDistanceAsync()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([
            new Dictionary<string, object?> { ["name"] = "a", ["distance"] = 1.5, ["__s0"] = 1.5, ["__s1"] = 1L },
            new Dictionary<string, object?> { ["name"] = "b", ["distance"] = 2.5, ["__s0"] = 2.5, ["__s1"] = 2L },
        ]);
        var near = new NearClause("geo", Lat: 1, Lon: 1);

        var first = await Engine(ex).QueryAsync<SpaceRow>(new QueryRequest { From = "spots", Near = near, Top = 1 });
        await Engine(ex).QueryAsync<SpaceRow>(new QueryRequest { From = "spots", Near = near, Top = 1, After = first.Next });

        Assert.IsNotNull(first.Next);
        Assert.AreEqual(1.5, first.Rows[0].Distance);
        StringAssert.Contains(ex.Calls[1].Sql, "HAV(s.lat,s.lon,@p0,@p1) > @");
    }

    [TestMethod]
    public async Task QueryAsync_NearErrors_AreQueryExceptionsAsync()
    {
        var ex = new FakeExecutor();

        await Assert.ThrowsExactlyAsync<QueryException>(() => Engine(ex).QueryAsync(new QueryRequest { From = "spots", Near = new NearClause("nope") }));
        await Assert.ThrowsExactlyAsync<QueryException>(() => Engine(ex).QueryAsync(new QueryRequest { From = "spots", Near = new NearClause("geo") }));
        await Assert.ThrowsExactlyAsync<QueryException>(() => Engine(ex).QueryAsync(new QueryRequest { From = "spots", Near = new NearClause("similar") }));
        await Assert.ThrowsExactlyAsync<QueryException>(() => Engine(ex).QueryAsync(new QueryRequest { From = "spots", Sort = ["distance"] }));
    }

    [TestMethod]
    public void Parse_Near_ReadsPositionReferenceAndMax()
    {
        var geo = QueryText.Parse("spots near geo:51.5,-0.1 max 25", Support.Registry, "spots");
        var vec = QueryText.Parse("spots near similar:42 top 5", Support.Registry, "spots");

        Assert.AreEqual(new NearClause("geo", null, 51.5, -0.1, 25), geo.Near);
        Assert.AreEqual(new NearClause("similar", "42"), vec.Near);
        Assert.AreEqual(5, vec.Top);
        Assert.ThrowsExactly<QueryException>(() => QueryText.Parse("spots near geo", Support.Registry, "spots"));
    }

    [TestMethod]
    public void Describe_ListsTheSpaces()
    {
        CollectionAssert.AreEqual(new[] { "geo", "similar" }, Describer.Describe(Support.Registry.Get("spots")).Spaces.ToArray());
    }
}
