using System;
using OoBDev.TestUtilities.Containers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.Query.Metadata;
using OoBDev.TestUtilities;

namespace OoBDev.PostgreSql.Query.Tests;

[TestClass]
[TestCategory(TestCategories.Integration)]
public class PostgresQueryIntegrationTests
{
    private string _schema = "";
    private string _connectionString = "";
    private ServiceProvider? _provider;
    private QueryEngine _engine = null!;

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public async Task Init()
    {
        var baseConnection = TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING");
        if (!baseConnection.Contains("oobdev_test", StringComparison.Ordinal))
            Assert.Inconclusive("The connection string is not a test database.");

        _schema = $"q_{Guid.NewGuid():N}";
        _connectionString = baseConnection.TrimEnd(';') + $";Search Path={_schema}";
        await using (var connection = new NpgsqlConnection(baseConnection))
        {
            await connection.OpenAsync();
            await Run(connection, $"CREATE SCHEMA \"{_schema}\"");
            await Run(connection, $"CREATE TABLE \"{_schema}\".items (id bigint PRIMARY KEY, title text NOT NULL, created timestamp NULL, tags text[] NULL, score int NOT NULL, removed boolean NOT NULL DEFAULT false)");
            await Run(connection, $"CREATE TABLE \"{_schema}\".places (id bigint PRIMARY KEY, name text NOT NULL, lat double precision NULL, lon double precision NULL, sha text NULL)");
            await Run(connection, $"INSERT INTO \"{_schema}\".places VALUES (1, 'a', 10.1, 20.1, 'sha-b'), (2, 'b', 10.2, 20.2, 'sha-a'), (3, 'c', 50.5, 60.5, 'sha-c'), (4, 'd', NULL, NULL, 'sha-d')");
            for (var i = 1; i <= 25; i++)
            {
                var suffix = i % 5 == 0 ? "50%_off" : "plain";
                var tag = i % 2 == 0 ? "even" : "odd";
                var day = (i % 9) + 1;
                var removed = i == 3 ? "true" : "false";
                await Run(connection, $"INSERT INTO \"{_schema}\".items (id, title, created, tags, score, removed) VALUES ({i}, 'item {i} {suffix}', '2024-01-0{day}', ARRAY['{tag}'], {i % 4}, {removed})");
            }
        }

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Query:Metadata:CursorKey"] = "0123456789abcdef0123456789abcdef-test-key",
            ["Query:Postgres:ConnectionString"] = _connectionString,
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddPostgresMetadataQuery(config, typeof(ItemRow).Assembly);
        _provider = services.BuildServiceProvider();
        _engine = _provider.GetRequiredService<QueryEngine>();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_provider is null)
            return;
        await _provider.DisposeAsync();
        var baseConnection = TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING");
        await using var connection = new NpgsqlConnection(baseConnection);
        await connection.OpenAsync();
        await Run(connection, $"DROP SCHEMA IF EXISTS \"{_schema}\" CASCADE");
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Query_FiltersAndCount_RunAgainstPostgres()
    {
        var page = await _engine.QueryAsync<ItemRow>(new QueryRequest { From = "items", Search = "tag:even", Filters = Filters("""{"live":"yes"}"""), Top = 100, Count = true });

        Assert.AreEqual(12, page.Rows.Count);
        Assert.AreEqual(12, page.Count);
        Assert.IsTrue(page.Rows.All(r => r.Id % 2 == 0));
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Query_WildcardInSearch_IsLiteral()
    {
        var page = await _engine.QueryAsync<ItemRow>(new QueryRequest { From = "items", Search = "\"50%_off\"", Top = 100 });

        Assert.AreEqual(5, page.Rows.Count);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Query_InjectionText_IsHarmless()
    {
        var page = await _engine.QueryAsync<ItemRow>(new QueryRequest { From = "items", Filters = Filters("""{"title":"x'; DROP TABLE items;--"}""") });

        Assert.AreEqual(0, page.Rows.Count);
        Assert.AreEqual(25, (await _engine.QueryAsync<ItemRow>(new QueryRequest { From = "items", Top = 100 })).Rows.Count);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Query_Paging_NeverRepeatsOrSkipsWhenARowIsInsertedBetweenPages()
    {
        var seen = new List<long>();
        string? after = null;
        var inserted = false;
        do
        {
            var page = await _engine.QueryAsync<ItemRow>(new QueryRequest { From = "items", Top = 7, After = after });
            seen.AddRange(page.Rows.Select(r => r.Id));
            after = page.Next;
            if (!inserted)
            {
                inserted = true;
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                await Run(connection, "INSERT INTO items (id, title, created, tags, score) VALUES (1000, 'late', '2099-01-01', ARRAY['new'], 1)");
            }
        }
        while (after is not null);

        Assert.AreEqual(seen.Count, seen.Distinct().Count());
        Assert.AreEqual(25, seen.Count(i => i < 1000));
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Query_FacetsShape_CountsByValue()
    {
        var page = await _engine.QueryAsync<FacetCount>(new QueryRequest { From = "items", Shape = "facets", By = ["score"] });

        Assert.AreEqual(25, page.Rows.Sum(r => r.Count));
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Query_BucketsShape_GroupsIntoGridCellsAndSkipsRowsWithoutPosition()
    {
        var page = await _engine.QueryAsync<Bucket>(new QueryRequest { From = "places", Shape = "buckets", By = ["lat", "lon"], Cell = 1 });

        TestContext.WriteLine("Buckets: {0}", string.Join("; ", page.Rows.Select(r => $"{r.A:0.##},{r.B:0.##} x{r.Count} {r.Sample}")));
        Assert.AreEqual(2, page.Rows.Count);
        Assert.AreEqual(2, page.Rows[0].Count);
        Assert.AreEqual(10.15, page.Rows[0].A, 0.0001);
        Assert.AreEqual("sha-a", page.Rows[0].Sample);
        Assert.AreEqual(1, page.Rows[1].Count);
    }

    private static Dictionary<string, JsonElement> Filters(string json) => JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

    private static async Task Run(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
