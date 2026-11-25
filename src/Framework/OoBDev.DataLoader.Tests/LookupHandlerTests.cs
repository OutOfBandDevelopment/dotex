using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DataLoader.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class LookupHandlerTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly Dictionary<string, string> Countries = new() { ["US"] = "1", ["DE"] = "2" };

    private static MemoryStream Text(string s) => new(Encoding.UTF8.GetBytes(s));

    [TestMethod]
    public async Task Load_RequiredLookup_MapsKnownAndRejectsUnknownOrEmpty()
    {
        var sink = new InMemoryRowSink("id");
        var loader = new Loader([new CsvRowReader()], [new LookupHandler(1, new DictionaryLookupSource(Countries), "country", "countryId")], sink);

        var report = await loader.LoadAsync(Text("id,country\n1,US\n2,FR\n3,\n4,DE\n"), "csv", new LoadOptions(), CancellationToken.None);
        TestContext.WriteLine($"written {report.Written}; errors {string.Join(" | ", report.Errors)}");

        Assert.AreEqual(2, report.Written);
        Assert.AreEqual(2, report.Errors.Count);
        Assert.AreEqual("1", sink.Rows["1"]["countryId"]);
        Assert.AreEqual("2", sink.Rows["4"]["countryId"]);
        StringAssert.Contains(report.Errors[0].Message, "FR");
    }

    [TestMethod]
    public async Task Handle_OptionalLookup_PassesUnknownAndEmptyUnchanged()
    {
        var handler = new LookupHandler(1, new DictionaryLookupSource(Countries), "country", "countryId", required: false);
        var unknown = new RowContext(1, new Dictionary<string, string?> { ["country"] = "FR" });
        var empty = new RowContext(2, new Dictionary<string, string?> { ["country"] = " " });

        Assert.IsTrue((await handler.HandleAsync(unknown, CancellationToken.None)).Accepted);
        Assert.IsTrue((await handler.HandleAsync(empty, CancellationToken.None)).Accepted);
        Assert.IsFalse(unknown.Values.ContainsKey("countryId"));
    }

    [TestMethod]
    public async Task Handle_DelegateQuery_ReceivesTheKeyAndTheToken()
    {
        string? seen = null;
        var handler = new LookupHandler(5, new DelegateLookupSource((key, _) => { seen = key; return ValueTask.FromResult<string?>(key.ToUpperInvariant()); }), "code", "upper");
        var row = new RowContext(1, new Dictionary<string, string?> { ["code"] = "ab" });

        var outcome = await handler.HandleAsync(row, CancellationToken.None);

        Assert.IsTrue(outcome.Accepted);
        Assert.AreEqual("ab", seen);
        Assert.AreEqual("AB", row.Values["upper"]);
        Assert.AreEqual(5, handler.Priority);
    }

    [TestMethod]
    public async Task Dictionary_CaseInsensitiveComparer_MatchesAnyCase()
    {
        var source = new DictionaryLookupSource(Countries, StringComparer.OrdinalIgnoreCase);

        Assert.AreEqual("1", await source.FindAsync("us", CancellationToken.None));
        Assert.IsNull(await source.FindAsync("xx", CancellationToken.None));
    }
}
