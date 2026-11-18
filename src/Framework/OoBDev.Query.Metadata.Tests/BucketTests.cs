using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[QueryKind("places", Base = "places p", Key = "p.id", DefaultSort = "name", Sample = "min(p.sha)")]
public sealed class PlaceRow
{
    [Facet("p.name")]
    public string Name { get; set; } = "";

    [Facet("p.lat")]
    public double Lat { get; set; }

    [Facet("p.lon")]
    public double Lon { get; set; }
}

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class BucketTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void Compile_Buckets_GroupsByGridCellAndBindsTheCell()
    {
        var q = Support.Compile(new QueryRequest { From = "places", Shape = "buckets", By = ["lat", "lon"], Cell = 0.5 });

        TestContext.WriteLine("SQL: {0}", q.Sql);
        StringAssert.Contains(q.Sql, "avg(p.lat)");
        StringAssert.Contains(q.Sql, "min(p.sha) AS \"sample\"");
        StringAssert.Contains(q.Sql, "p.lat IS NOT NULL AND p.lon IS NOT NULL");
        StringAssert.Contains(q.Sql, "GROUP BY floor((p.lat) / @p0), floor((p.lon) / @p0)");
        Assert.AreEqual(0.5, q.Parameters["p0"]);
    }

    [TestMethod]
    public void Compile_BucketsWithoutCell_UsesOneAndClampsTooSmall()
    {
        var whole = Support.Compile(new QueryRequest { From = "places", Shape = "buckets", By = ["lat", "lon"] });
        var tiny = Support.Compile(new QueryRequest { From = "places", Shape = "buckets", By = ["lat", "lon"], Cell = 0 });

        Assert.AreEqual(1.0, whole.Parameters["p0"]);
        Assert.AreEqual(0.0001, tiny.Parameters["p0"]);
    }

    [TestMethod]
    public void Compile_BucketsKindWithoutSample_SelectsNull()
    {
        var q = Support.Compile(new QueryRequest { From = "photos", Shape = "buckets", By = ["size", "rating"] });

        StringAssert.Contains(q.Sql, "NULL AS \"sample\"");
    }

    [TestMethod]
    public void Compile_BucketsBadRequests_Are400()
    {
        var bad = new[]
        {
            new QueryRequest { From = "places", Shape = "buckets" },
            new QueryRequest { From = "places", Shape = "buckets", By = ["lat"] },
            new QueryRequest { From = "places", Shape = "buckets", By = ["lat", "name"] },
            new QueryRequest { From = "places", Shape = "buckets", By = ["lat", "nope"] },
            new QueryRequest { From = "places", Shape = "buckets", By = ["lat", "lon"], After = "x" },
        };
        foreach (var request in bad)
            Assert.ThrowsExactly<QueryException>(() => Support.Compile(request));
    }

    [TestMethod]
    public void Text_AsBucketsByCell_SetsShapeAxesAndCell()
    {
        var r = QueryText.Parse("places as buckets by lat,lon cell 0.25", Support.Registry, "places");

        Assert.AreEqual("buckets", r.Shape);
        CollectionAssert.AreEqual(new[] { "lat", "lon" }, r.By);
        Assert.AreEqual(0.25, r.Cell);
    }

    [TestMethod]
    public void Text_BucketsBadCell_Is400()
    {
        Assert.ThrowsExactly<QueryException>(() => QueryText.Parse("places as buckets by lat,lon cell x", Support.Registry, "places"));
    }

    [TestMethod]
    public async Task QueryAsync_Buckets_MapsRowsToBucketsAsync()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([new Dictionary<string, object?> { ["a"] = 52.1, ["b"] = 4.3, ["count"] = 7L, ["sample"] = "abc" }]);
        var engine = new QueryEngine(Support.Registry, new TestDialect(), ex, Support.Codec(), Support.Options(), NullLogger<QueryEngine>.Instance);

        var result = await engine.QueryAsync<Bucket>(new QueryRequest { From = "places", Shape = "buckets", By = ["lat", "lon"] });

        Assert.AreEqual("buckets", result.Shape);
        Assert.AreEqual(7L, result.Rows[0].Count);
        Assert.AreEqual(52.1, result.Rows[0].A);
        Assert.AreEqual("abc", result.Rows[0].Sample);
        Assert.IsNull(result.Next);
    }
}
