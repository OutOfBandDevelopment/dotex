using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.OpenSearch;
using OoBDev.PostgreSql.Query;
using OoBDev.Search.Requests;
using OoBDev.TestUtilities;
using OpenSearch.Net;

namespace OoBDev.Search.Parity.Tests;

/// <summary>The same filter requests run against the keyword (OpenSearch) and SQL-kind (PostgreSQL) providers over equal data.</summary>
[TestClass]
[TestCategory(TestCategories.Integration)]
public class FilterParityTests
{
    private sealed record Doc(long Id, string Name, string Ext, int Size);

    private static readonly Doc[] Docs =
    [
        new(1, "red car", "pdf", 1),
        new(2, "blue car", "pdf", 2),
        new(3, "red bike", "jpg", 3),
        new(4, "green bike", "jpg", 4),
        new(5, "red boat", "txt", 5),
        new(6, "blue boat", "txt", 6),
        new(7, "green car", "pdf", 7),
        new(8, "red plane", "jpg", 8),
        new(9, "blue plane", "txt", 9),
        new(10, "green plane", "pdf", 10),
    ];

    private string _schema = "";
    private string _pgBase = "";
    private string _index = "";
    private OpenSearchLowLevelClient? _client;
    private ServiceProvider? _services;
    private ISearchProvider _keyword = null!;
    private ISearchProvider _sql = null!;

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public async Task Init()
    {
        _pgBase = TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING");
        if (!_pgBase.Contains("oobdev_test", StringComparison.Ordinal))
        {
            Assert.Inconclusive("The connection string is not a test database.");
        }

        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OPENSEARCH_URL"));
        _client = new OpenSearchLowLevelClient(new ConnectionConfiguration(new Uri(url))
            .ServerCertificateValidationCallback((_, _, _, _) => true)
            .BasicAuthentication(TestContext.GetRequiredProperty<string>("OPENSEARCH_USERNAME"), TestContext.GetRequiredProperty<string>("OPENSEARCH_PASSWORD"))
            .ThrowExceptions(true));

        _index = $"parity_{Guid.NewGuid():N}";
        await _client.Indices.CreateAsync<StringResponse>(_index, PostData.String(
            """{"mappings":{"properties":{"id":{"type":"long"},"name":{"type":"text"},"ext":{"type":"keyword"},"size":{"type":"integer"}}}}"""));
        foreach (var doc in Docs)
        {
            await _client.IndexAsync<StringResponse>(_index, doc.Id.ToString(CultureInfo.InvariantCulture), PostData.Serializable(new { id = doc.Id, name = doc.Name, ext = doc.Ext, size = doc.Size }));
        }

        await _client.Indices.RefreshAsync<StringResponse>(_index);

        _schema = $"p_{Guid.NewGuid():N}";
        await using (var connection = new NpgsqlConnection(_pgBase))
        {
            await connection.OpenAsync();
            await Run(connection, $"CREATE SCHEMA \"{_schema}\"");
            await Run(connection, $"CREATE TABLE \"{_schema}\".docs (id bigint PRIMARY KEY, name text NOT NULL, ext text NOT NULL, size int NOT NULL)");
            foreach (var doc in Docs)
            {
                await Run(connection, $"INSERT INTO \"{_schema}\".docs (id, name, ext, size) VALUES ({doc.Id}, '{doc.Name}', '{doc.Ext}', {doc.Size})");
            }
        }

        var uri = new Uri(url);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Query:Metadata:CursorKey"] = "0123456789abcdef0123456789abcdef-test-key",
            ["Query:Postgres:ConnectionString"] = _pgBase.TrimEnd(';') + $";Search Path={_schema}",
            ["OoBDev:Search:SqlKind:Kind"] = "docs",
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddPostgresMetadataQuery(configuration, typeof(ParityDoc).Assembly);
        services.TryAddPostgresSqlKindSearch(configuration);
        _services = services.BuildServiceProvider();
        _sql = _services.GetRequiredKeyedService<ISearchProvider>(PostgreSql.Query.ServiceCollectionExtensions.SqlKindProviderKey);
        _keyword = new KeywordSearchProvider(
            _client,
            Options.Create(new OpenSearchOptions { HostName = uri.Host, Port = uri.Port, IndexName = _index }),
            Options.Create(new KeywordSearchOptions { TextFields = ["name"], FilterFields = ["ext", "size"] }));
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_services is not null)
        {
            await _services.DisposeAsync();
        }

        if (_client is not null && _index.Length > 0)
        {
            await _client.Indices.DeleteAsync<StringResponse>(_index);
        }

        if (_schema.Length > 0)
        {
            await using var connection = new NpgsqlConnection(_pgBase);
            await connection.OpenAsync();
            await Run(connection, $"DROP SCHEMA IF EXISTS \"{_schema}\" CASCADE");
        }
    }

    private static async Task Run(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task AssertParity(SearchRequest request, Func<Doc, bool> expected)
    {
        var keyword = (await _keyword.SearchAsync(request)).Items.Select(i => i.ItemId).Order(StringComparer.Ordinal).ToArray();
        var sql = (await _sql.SearchAsync(request)).Items.Select(i => i.ItemId).Order(StringComparer.Ordinal).ToArray();
        var want = Docs.Where(expected).Select(d => d.Id.ToString(CultureInfo.InvariantCulture)).Order(StringComparer.Ordinal).ToArray();

        Assert.AreNotEqual(0, want.Length, "The case expects no hits, so it proves nothing.");
        CollectionAssert.AreEqual(want, keyword, "keyword: " + JsonSerializer.Serialize(keyword));
        CollectionAssert.AreEqual(want, sql, "sql: " + JsonSerializer.Serialize(sql));
    }

    private static SearchRequest Request(string text = "", params FieldFilter[] filters) =>
        new() { Text = text, Filters = filters, PageSize = 50 };

    [TestMethod]
    public Task Equals_OnKeyword_MatchesBothProviders() =>
        AssertParity(Request(filters: new FieldFilter("ext", FilterComparator.Equals, "pdf")), d => d.Ext == "pdf");

    [TestMethod]
    public Task NegatedEquals_ExcludesInBothProviders() =>
        AssertParity(Request(filters: new FieldFilter("ext", FilterComparator.Equals, "pdf", Negated: true)), d => d.Ext != "pdf");

    [TestMethod]
    public Task Range_BothBounds_MatchesBothProviders() =>
        AssertParity(Request(filters: [new FieldFilter("size", FilterComparator.GreaterOrEqual, "3"), new FieldFilter("size", FilterComparator.Less, "8")]), d => d.Size is >= 3 and < 8);

    [TestMethod]
    public Task Range_StrictBounds_MatchesBothProviders() =>
        AssertParity(Request(filters: [new FieldFilter("size", FilterComparator.Greater, "2"), new FieldFilter("size", FilterComparator.LessOrEqual, "6")]), d => d.Size is > 2 and <= 6);

    [TestMethod]
    public Task Text_WholeWord_MatchesBothProviders() =>
        AssertParity(Request("red"), d => d.Name.Contains("red", StringComparison.Ordinal));

    [TestMethod]
    public Task TextAndFilter_Combine_InBothProviders() =>
        AssertParity(Request("car", new FieldFilter("ext", FilterComparator.Equals, "pdf"), new FieldFilter("size", FilterComparator.Greater, "1")),
            d => d.Name.Contains("car", StringComparison.Ordinal) && d.Ext == "pdf" && d.Size > 1);
}
