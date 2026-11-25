using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class QueryLimitsTests
{
    public required TestContext TestContext { get; set; }

    private static QueryEngine Engine(QueryMetadataOptions options)
    {
        options.CursorKey = new string('k', 40);
        return new QueryEngine(Support.Registry, new TestDialect(), new FakeExecutor(), new CursorCodec(Microsoft.Extensions.Options.Options.Create(options)),
            Microsoft.Extensions.Options.Options.Create(options), NullLogger<QueryEngine>.Instance);
    }

    [TestMethod]
    public async Task QueryAsync_SearchTextTooLong_Is400()
    {
        var engine = Engine(new QueryMetadataOptions { MaxSearchLength = 10 });

        var ex = await Assert.ThrowsExactlyAsync<QueryException>(async () =>
            await engine.QueryAsync(new QueryRequest { From = "photos", Search = new string('a', 11) }, TestContext.CancellationToken));
        TestContext.WriteLine(ex.Message);

        Assert.AreEqual(400, ex.Status);
        StringAssert.Contains(ex.Message, "at most 10");
    }

    [TestMethod]
    public async Task QueryAsync_TooManyFilters_Is400()
    {
        var engine = Engine(new QueryMetadataOptions { MaxFilters = 2 });
        var filters = Support.Filters("""{"name":"a","id":1,"taken":"2024-01-01"}""");

        var ex = await Assert.ThrowsExactlyAsync<QueryException>(async () =>
            await engine.QueryAsync(new QueryRequest { From = "photos", Filters = filters }, TestContext.CancellationToken));
        TestContext.WriteLine(ex.Message);

        StringAssert.Contains(ex.Message, "3 filters");
    }

    [TestMethod]
    public async Task QueryAsync_TooManyValuesInOneFilter_Is400()
    {
        var engine = Engine(new QueryMetadataOptions { MaxFilterValues = 3 });
        var values = string.Join(',', Enumerable.Range(1, 4));
        var filters = Support.Filters($$"""{"id":[{{values}}]}""");

        var ex = await Assert.ThrowsExactlyAsync<QueryException>(async () =>
            await engine.QueryAsync(new QueryRequest { From = "photos", Filters = filters }, TestContext.CancellationToken));
        TestContext.WriteLine(ex.Message);

        StringAssert.Contains(ex.Message, "'id' lists 4 values");
    }

    [TestMethod]
    public async Task QueryAsync_WithinTheLimits_Runs()
    {
        var engine = Engine(new QueryMetadataOptions());

        var page = await engine.QueryAsync(new QueryRequest { From = "photos", Filters = Support.Filters("""{"name":"a"}"""), Search = "sunset" }, TestContext.CancellationToken);

        Assert.AreEqual(0, page.Rows.Count);
    }
}
