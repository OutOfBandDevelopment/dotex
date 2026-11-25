using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search;
using OoBDev.Search.Requests;
using OoBDev.TestUtilities;
using OpenSearch.Client;
using OpenSearch.Net;
using SearchRequest = OoBDev.Search.Requests.SearchRequest;

namespace OoBDev.OpenSearch.Tests;

[TestClass]
public class KeywordSearchTests
{
    private const string Response = """
        {"hits":{"total":{"value":5,"relation":"eq"},"hits":[
          {"_id":"a","_score":2.5,"_source":{"name":"x.pdf","size":10,"ratio":0.5,"ok":true,"tags":["t"]}},
          {"_id":"b","_score":null}
        ]}}
        """;

    private static KeywordSearchProvider Provider(string body, int status = 200, params string[] filters)
    {
        var connection = new InMemoryConnection(Encoding.UTF8.GetBytes(body), status);
        var client = new OpenSearchLowLevelClient(new ConnectionConfiguration(new SingleNodeConnectionPool(new Uri("http://127.0.0.1:9200")), connection));
        return new KeywordSearchProvider(
            client,
            Options.Create(new OpenSearchOptions { HostName = "h", Port = 9200, IndexName = "docs" }),
            Options.Create(new KeywordSearchOptions { TextFields = ["name", "body"], FilterFields = filters }));
    }

    private static JsonElement Body(SearchRequest request, int offset = 0) =>
        JsonDocument.Parse(KeywordQueryBuilder.Build(request, ["name", "body"], offset)).RootElement;

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_ParsesHitsScoresMetadataAndNextCursor()
    {
        var result = await Provider(Response).SearchAsync(new SearchRequest { Text = "cat", PageSize = 2 });

        Assert.AreEqual(2, result.Items.Count);
        Assert.AreEqual("a", result.Items[0].ItemId);
        Assert.AreEqual(2.5, result.Items[0].Score);
        Assert.AreEqual("keyword", result.Items[0].Explanation.Field);
        Assert.AreEqual("x.pdf", result.Items[0].MetaData!["name"]);
        Assert.AreEqual(10L, result.Items[0].MetaData!["size"]);
        Assert.AreEqual(0.5, result.Items[0].MetaData!["ratio"]);
        Assert.AreEqual(true, result.Items[0].MetaData!["ok"]);
        Assert.AreEqual("[\"t\"]", result.Items[0].MetaData!["tags"]);
        Assert.AreEqual(0, result.Items[1].Score);
        Assert.IsNull(result.Items[1].MetaData);
        Assert.AreEqual("2", result.NextCursor);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_LastPage_HasNoCursor()
    {
        var result = await Provider(Response).SearchAsync(new SearchRequest { Text = "cat", PageSize = 3, Cursor = "3" });

        Assert.IsNull(result.NextCursor);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_ServerError_Throws() =>
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => Provider("{}", 500).SearchAsync(new SearchRequest { Text = "x" }));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_UnknownFilterVectorAnchorOrBadCursor_Refused()
    {
        var provider = Provider(Response, 200, "ext");

        var bad = new SearchRequest { Text = "x", Filters = [new FieldFilter("nope", FilterComparator.Equals, "1")] };
        Assert.AreEqual("nope", (await Assert.ThrowsExactlyAsync<UnsupportedFilterException>(() => provider.SearchAsync(bad))).Filter);
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.SearchAsync(new SearchRequest { Anchor = new SearchAnchor(LikeItemId: "i") }));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.SearchAsync(new SearchRequest { Anchor = new SearchAnchor(NearVector: new ReadOnlyMemory<float>([1f])) }));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => provider.SearchAsync(new SearchRequest { Text = "x", Cursor = "abc" }));
        CollectionAssert.AreEquivalent(new[] { "ext" }, provider.SupportedFilters.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_TextAndPaging_UsesSimpleQueryString()
    {
        var body = Body(new SearchRequest { Text = "red car", PageSize = 7 }, 14);

        Assert.AreEqual(14, body.GetProperty("from").GetInt32());
        Assert.AreEqual(7, body.GetProperty("size").GetInt32());
        var query = body.GetProperty("query").GetProperty("bool").GetProperty("must")[0].GetProperty("simple_query_string");
        Assert.AreEqual("red car", query.GetProperty("query").GetString());
        Assert.AreEqual("body", query.GetProperty("fields")[1].GetString());
        Assert.AreEqual("and", query.GetProperty("default_operator").GetString());
        Assert.IsFalse(body.TryGetProperty("sort", out _));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_NoText_MatchesAll() =>
        Assert.IsTrue(Body(new SearchRequest()).GetProperty("query").GetProperty("bool").GetProperty("must")[0].TryGetProperty("match_all", out _));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_FiltersSortAndNegation()
    {
        var request = new SearchRequest
        {
            Text = "x",
            Sort = ["-taken", "name"],
            Filters =
            [
                new FieldFilter("ext", FilterComparator.Equals, "pdf"),
                new FieldFilter("tag", FilterComparator.Contains, "a"),
                new FieldFilter("size", FilterComparator.Greater, "1"),
                new FieldFilter("size", FilterComparator.GreaterOrEqual, "2"),
                new FieldFilter("size", FilterComparator.Less, "3"),
                new FieldFilter("size", FilterComparator.LessOrEqual, "4"),
                new FieldFilter("tag", FilterComparator.Equals, "x", Negated: true),
            ],
        };

        var bool_ = Body(request).GetProperty("query").GetProperty("bool");
        var filter = bool_.GetProperty("filter");
        Assert.AreEqual("pdf", filter[0].GetProperty("term").GetProperty("ext").GetString());
        Assert.AreEqual("a", filter[1].GetProperty("match").GetProperty("tag").GetString());
        Assert.AreEqual("1", filter[2].GetProperty("range").GetProperty("size").GetProperty("gt").GetString());
        Assert.AreEqual("2", filter[3].GetProperty("range").GetProperty("size").GetProperty("gte").GetString());
        Assert.AreEqual("3", filter[4].GetProperty("range").GetProperty("size").GetProperty("lt").GetString());
        Assert.AreEqual("4", filter[5].GetProperty("range").GetProperty("size").GetProperty("lte").GetString());
        Assert.AreEqual("x", bool_.GetProperty("must_not")[0].GetProperty("term").GetProperty("tag").GetString());
        var sort = Body(request).GetProperty("sort");
        Assert.AreEqual("desc", sort[0].GetProperty("taken").GetString());
        Assert.AreEqual("asc", sort[1].GetProperty("name").GetString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Offset_NullIsZero_NumberIsParsed()
    {
        Assert.AreEqual(0, KeywordQueryBuilder.Offset(null));
        Assert.AreEqual(40, KeywordQueryBuilder.Offset("40"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Registration_ResolvesProviderUnderTheKey()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OpenSearchOptions:HostName"] = "h",
            ["OpenSearchOptions:Port"] = "9200",
            ["OpenSearchOptions:IndexName"] = "docs",
            ["OoBDev:OpenSearch:Keyword:FilterFields:0"] = "ext",
        }).Build();
        using var provider = new ServiceCollection().TryAddOpenSearchServices(configuration, "OpenSearchOptions").TryAddOpenSearchKeywordSearch(configuration).BuildServiceProvider();

        var search = provider.GetRequiredKeyedService<ISearchProvider>(KeywordRegistration.ProviderKey);

        Assert.IsInstanceOfType<KeywordSearchProvider>(search);
        CollectionAssert.AreEqual(new[] { "ext" }, search.SupportedFilters.ToArray());
    }
}
