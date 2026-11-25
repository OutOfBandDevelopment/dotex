using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class TextAnchorTests
{
    public required TestContext TestContext { get; set; }

    private sealed class FixedEmbedder : IEmbeddingProvider
    {
        public int Length => 3;

        public string? Last { get; private set; }

        public Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string content, string? model, CancellationToken cancellationToken)
        {
            Last = content;
            return Task.FromResult<ReadOnlyMemory<float>>(new float[] { 0.25f, -1f, 2.5f });
        }
    }

    private static QueryEngine Engine(FakeExecutor executor, IEmbeddingProvider? embedder) =>
        new(Support.Registry, new TestDialect(), executor, Support.Codec(), Support.Options(), NullLogger<QueryEngine>.Instance, null, embedder);

    [TestMethod]
    public async Task QueryAsync_NearText_EmbedsTheWordsAndBindsTheVectorAsync()
    {
        var ex = new FakeExecutor();
        var embedder = new FixedEmbedder();

        await Engine(ex, embedder).QueryAsync<SpaceRow>(new QueryRequest { From = "indexed", Near = new NearClause("similar", Text: "red barn at sunset") }, TestContext.CancellationToken);

        var call = ex.Calls[0];
        TestContext.WriteLine("SQL: {0}; parameters: {1}", call.Sql, string.Join(", ", call.Parameters.Select(p => $"{p.Key}={p.Value}")));
        Assert.AreEqual("red barn at sunset", embedder.Last);
        Assert.IsTrue(call.Parameters.Values.Contains("[0.25,-1,2.5]"), "the embedded vector is bound as vector text");
        Assert.IsFalse(call.Sql.Contains("barn", StringComparison.Ordinal), "the words never reach the SQL");
        StringAssert.Contains(call.Sql, "sv.d AS \"distance\"");
    }

    [TestMethod]
    public async Task QueryAsync_NearTextWithoutAnEmbedder_Is400Async()
    {
        var ex = new FakeExecutor();

        var error = await Assert.ThrowsExactlyAsync<QueryException>(() =>
            Engine(ex, null).QueryAsync<SpaceRow>(new QueryRequest { From = "indexed", Near = new NearClause("similar", Text: "barn") }, TestContext.CancellationToken));

        TestContext.WriteLine(error.Message);
        Assert.AreEqual(400, error.Status);
        Assert.AreEqual(0, ex.Calls.Count);
    }

    [TestMethod]
    public void Parse_NearAbout_ReadsTheQuotedWordsAndMax()
    {
        var r = QueryText.Parse("indexed near similar about \"red barn\" max 0.4", Support.Registry, "indexed");

        Assert.AreEqual("similar", r.Near!.Space);
        Assert.AreEqual("red barn", r.Near.Text);
        Assert.AreEqual(0.4, r.Near.Max);
    }
}
