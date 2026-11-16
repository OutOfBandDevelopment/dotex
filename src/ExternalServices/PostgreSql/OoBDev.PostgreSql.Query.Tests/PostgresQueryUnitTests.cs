using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Query.Metadata;
using OoBDev.TestUtilities;

namespace OoBDev.PostgreSql.Query.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class PostgresQueryUnitTests
{
    private readonly PostgresQueryDialect _dialect = new();

    [TestMethod]
    public void Quote_EmbeddedQuote_IsDoubled() => Assert.AreEqual("\"a\"\"b\"", _dialect.Quote("a\"b"));

    [TestMethod]
    public void Like_UsesIlikeWithEscape() => StringAssert.Contains(_dialect.Like("x", "@p0"), "ILIKE @p0 ESCAPE");

    [TestMethod]
    public void NotLike_UsesNotIlike() => StringAssert.Contains(_dialect.NotLike("x", "@p0"), "NOT ILIKE");

    [TestMethod]
    public void In_UsesAny() => Assert.AreEqual("x = ANY(@p0)", _dialect.In("x", "@p0"));

    [TestMethod]
    public void ArrayContains_PutsParameterFirst() => Assert.AreEqual("@p0 = ANY(x)", _dialect.ArrayContains("@p0", "x"));

    [TestMethod]
    public void Direction_SortsNullsLast()
    {
        StringAssert.Contains(_dialect.Direction(true), "DESC NULLS LAST");
        StringAssert.Contains(_dialect.Direction(false), "ASC NULLS LAST");
    }

    [TestMethod]
    public void Limit_IsInvariant() => Assert.AreEqual("LIMIT 61", _dialect.Limit(61));

    [TestMethod]
    public void DatePart_KnownAndUnknown()
    {
        StringAssert.Contains(_dialect.DatePart("month", "x")!, "YYYY-MM");
        StringAssert.Contains(_dialect.DatePart("year", "x")!, "YYYY");
        StringAssert.Contains(_dialect.DatePart("day", "x")!, "DD");
        Assert.IsNull(_dialect.DatePart("week", "x"));
    }

    [TestMethod]
    public void ContainsPattern_EscapesWildcards() => Assert.AreEqual("%a\\%b\\_c\\\\%", _dialect.ContainsPattern("a%b_c\\"));

    [TestMethod]
    public async Task TryAddPostgresMetadataQuery_ResolvesEngine()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Query:Metadata:CursorKey"] = "0123456789abcdef0123456789abcdef-test-key",
            ["Query:Postgres:ConnectionString"] = "Host=localhost;Database=oobdev_test",
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddPostgresMetadataQuery(config, typeof(ItemRow).Assembly);

        await using var provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<PostgresQueryDialect>(provider.GetRequiredService<IQueryDialect>());
        Assert.IsNotNull(provider.GetRequiredService<QueryEngine>());
    }
}
