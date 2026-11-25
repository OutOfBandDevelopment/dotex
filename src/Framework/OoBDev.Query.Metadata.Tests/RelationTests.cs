using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class RelationTests
{
    public required TestContext TestContext { get; set; }

    private static QueryEngine Engine(FakeExecutor executor) =>
        new(Support.Registry, new TestDialect(), executor, Support.Codec(), Support.Options(), NullLogger<QueryEngine>.Instance);

    [TestMethod]
    public async Task QueryAsync_Related_AddsTheRelationWithTheAnchorBoundAsync()
    {
        var ex = new FakeExecutor();

        await Engine(ex).QueryAsync<HintedRow>(new QueryRequest { From = "hinted", Related = new RelatedClause("copies", "42") });

        TestContext.WriteLine("SQL: {0}", ex.Calls[0].Sql);
        StringAssert.Contains(ex.Calls[0].Sql, "CAST(@p0 AS bigint)");
        Assert.AreEqual("42", ex.Calls[0].Parameters["p0"]);
        Assert.IsFalse(ex.Calls[0].Sql.Contains("42"), "the anchor is bound, never written into the statement");
    }

    [TestMethod]
    public async Task QueryAsync_UnknownRelation_ThrowsAsync()
    {
        var ex = new FakeExecutor();

        var error = await Assert.ThrowsExactlyAsync<QueryException>(() =>
            Engine(ex).QueryAsync<HintedRow>(new QueryRequest { From = "hinted", Related = new RelatedClause("nope", "1") }));

        StringAssert.Contains(error.Message, "copies");
    }

    [TestMethod]
    public void Parse_Of_ReadsRelationAndAnchor()
    {
        var request = QueryText.Parse("hinted of copies:42 top 5", Support.Registry, "hinted");

        Assert.AreEqual(new RelatedClause("copies", "42"), request.Related);
        Assert.AreEqual(5, request.Top);
    }

    [TestMethod]
    public void Parse_OfWithoutAnchor_Throws()
    {
        Assert.ThrowsExactly<QueryException>(() => QueryText.Parse("of copies", Support.Registry, "hinted"));
    }

    [TestMethod]
    public void Describe_ListsTheRelations()
    {
        CollectionAssert.AreEqual(new[] { "copies" }, Describer.Describe(Support.Registry.Get("hinted")).Relations);
    }
}
