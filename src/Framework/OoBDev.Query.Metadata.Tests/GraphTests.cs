using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class GraphTests
{
    public required TestContext TestContext { get; set; }

    private static QueryEngine Engine(FakeExecutor executor) =>
        new(Support.Registry, new TestDialect(), executor, Support.Codec(), Support.Options(), NullLogger<QueryEngine>.Instance);

    private static IReadOnlyDictionary<string, object?> Edge(string a, string b, double w) =>
        new Dictionary<string, object?> { ["source"] = a, ["target"] = b, ["weight"] = w };

    [TestMethod]
    public async Task QueryAsync_Graph_ReturnsEdgesAndTheNodesTheyTouchAsync()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([Edge("a", "b", 5), Edge("b", "c", 2)]);

        var result = await Engine(ex).QueryAsync<GraphEdge>(new QueryRequest { From = "links", Shape = "graph" });

        TestContext.WriteLine("SQL: {0}", ex.Calls[0].Sql);
        StringAssert.Contains(ex.Calls[0].Sql, "ORDER BY (l.shared) DESC");
        Assert.AreEqual(2, result.Rows.Count);
        Assert.AreEqual(5, result.Rows[0].Weight);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result.Nodes!.ToArray());
    }

    [TestMethod]
    public async Task QueryAsync_GraphWithRoot_WalksOneStatementPerStepAsync()
    {
        var ex = new FakeExecutor();
        ex.Results.Enqueue([Edge("a", "b", 5)]);
        ex.Results.Enqueue([Edge("a", "b", 5), Edge("b", "c", 2)]);

        var result = await Engine(ex).QueryAsync<GraphEdge>(new QueryRequest { From = "links", Shape = "graph", Root = "a", Depth = 3 });

        TestContext.WriteLine("Statements: {0}", ex.Calls.Count);
        Assert.AreEqual(3, ex.Calls.Count, "third step finds c's neighbours and nothing new");
        CollectionAssert.AreEqual(new[] { "a" }, (string[])ex.Calls[0].Parameters.Values.Last()!);
        CollectionAssert.AreEquivalent(new[] { "a", "b" }, ((string[])ex.Calls[1].Parameters.Values.Last()!).Concat(new[] { "a" }).Distinct().ToArray());
        Assert.AreEqual(2, result.Rows.Count, "an edge seen twice counts once");
    }

    [TestMethod]
    public async Task QueryAsync_GraphOnKindWithoutEdges_ThrowsAsync()
    {
        var ex = new FakeExecutor();

        var error = await Assert.ThrowsExactlyAsync<QueryException>(() =>
            Engine(ex).QueryAsync(new QueryRequest { From = "hinted", Shape = "graph" }));

        StringAssert.Contains(error.Message, "no graph");
    }

    [TestMethod]
    public void Parse_AsGraph_ReadsRootAndDepth()
    {
        var request = QueryText.Parse("links as graph from 7 depth 2", Support.Registry, "links");

        Assert.AreEqual("graph", request.Shape);
        Assert.AreEqual("7", request.Root);
        Assert.AreEqual(2, request.Depth);
    }

    [TestMethod]
    public void Parse_FromInSearchText_StaysSearchText()
    {
        var request = QueryText.Parse("links photos from paris", Support.Registry, "links");

        Assert.AreEqual("photos from paris", request.Search);
    }
}
