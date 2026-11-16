using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Query.Metadata;
using OoBDev.Search;
using OoBDev.Search.Requests;
using OoBDev.TestUtilities;

namespace OoBDev.PostgreSql.Query.Tests;

[TestClass]
public class SqlKindSearchTests
{
    private sealed class FakeExecutor : IQueryExecutor
    {
        public string? Sql { get; private set; }

        public IReadOnlyDictionary<string, object?>? Parameters { get; private set; }

        public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ReadAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken)
        {
            Sql = sql;
            Parameters = parameters;
            IReadOnlyList<IReadOnlyDictionary<string, object?>> rows =
            [
                new Dictionary<string, object?> { ["id"] = 7L, ["title"] = "one", ["tags"] = null, ["__s0"] = 1L, ["__s1"] = 7L },
            ];
            return Task.FromResult(rows);
        }
    }

    private static (ServiceProvider Services, FakeExecutor Executor) Build()
    {
        var executor = new FakeExecutor();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Query:Metadata:CursorKey"] = "0123456789abcdef0123456789abcdef-test-key",
            ["Query:Postgres:ConnectionString"] = "Host=localhost;Database=oobdev_test",
            ["OoBDev:Search:SqlKind:Kind"] = "items",
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddPostgresMetadataQuery(configuration, typeof(ItemRow).Assembly);
        services.AddSingleton<IQueryExecutor>(executor);
        services.TryAddPostgresSqlKindSearch(configuration);
        return (services.BuildServiceProvider(), executor);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_TextAndFilters_BecomeParametersAndHitsCarryTheId()
    {
        var (services, executor) = Build();
        using var _ = services;
        var provider = services.GetRequiredKeyedService<ISearchProvider>(ServiceCollectionExtensions.SqlKindProviderKey);
        var request = new SearchRequest
        {
            Text = "cat",
            PageSize = 1,
            Sort = ["score"],
            Filters =
            [
                new FieldFilter("score", FilterComparator.GreaterOrEqual, "5"),
                new FieldFilter("score", FilterComparator.Less, "9"),
                new FieldFilter("title", FilterComparator.Contains, "x", Negated: true),
            ],
        };

        var result = await provider.SearchAsync(request);

        Assert.AreEqual(1, result.Items.Count);
        Assert.AreEqual("7", result.Items[0].ItemId);
        Assert.AreEqual("sql", result.Items[0].Explanation.Field);
        Assert.AreEqual("one", result.Items[0].MetaData!["title"]);
        Assert.IsFalse(result.Items[0].MetaData!.ContainsKey("tags"));
        StringAssert.Contains(executor.Sql!, ">=");
        StringAssert.Contains(executor.Sql!, "NOT ILIKE");
        Assert.IsTrue(executor.Parameters!.Values.Any(v => v is double d && d == 5));
        Assert.IsNull(result.NextCursor);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_EqualsAndNegatedEquals_MapToEqAndNe()
    {
        var (services, executor) = Build();
        using var _ = services;
        var provider = services.GetRequiredService<SqlKindSearchProvider>();

        await provider.SearchAsync(new SearchRequest { Filters = [new FieldFilter("title", FilterComparator.Equals, "a"), new FieldFilter("id", FilterComparator.Equals, "3", Negated: true)] });

        StringAssert.Contains(executor.Sql!, "IS DISTINCT FROM");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_Unsupported_Refused()
    {
        var (services, _) = Build();
        using var _ = services;
        var provider = services.GetRequiredService<SqlKindSearchProvider>();

        Assert.AreEqual("nope", (await Assert.ThrowsExactlyAsync<UnsupportedFilterException>(
            () => provider.SearchAsync(new SearchRequest { Filters = [new FieldFilter("nope", FilterComparator.Equals, "1")] }))).Filter);
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.SearchAsync(new SearchRequest { Anchor = new SearchAnchor(LikeItemId: "i") }));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.SearchAsync(
            new SearchRequest { Filters = [new FieldFilter("score", FilterComparator.Greater, "1", Negated: true)] }));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.SearchAsync(
            new SearchRequest { Filters = [new FieldFilter("score", FilterComparator.Greater, "1"), new FieldFilter("score", FilterComparator.Greater, "2")] }));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => provider.SearchAsync(null!));
        CollectionAssert.IsSubsetOf(new[] { "id", "title", "score" }, provider.SupportedFilters.ToArray());
    }
}
