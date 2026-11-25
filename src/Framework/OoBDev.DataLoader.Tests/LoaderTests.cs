using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DataLoader.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class LoaderTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class Required(string column, int priority = 0) : IPipelineHandler
    {
        public int Priority => priority;

        public ValueTask<RowOutcome> HandleAsync(RowContext row, CancellationToken cancellationToken)
            => ValueTask.FromResult(string.IsNullOrWhiteSpace((row.Values.TryGetValue(column, out var v) ? v : null)) ? RowOutcome.Reject($"{column} is required") : RowOutcome.Accept);
    }

    private sealed class Recorder(List<int> order, int priority) : IPipelineHandler
    {
        public int Priority => priority;

        public ValueTask<RowOutcome> HandleAsync(RowContext row, CancellationToken cancellationToken)
        {
            order.Add(priority);
            return ValueTask.FromResult(RowOutcome.Accept);
        }
    }

    private static MemoryStream Text(string s) => new(Encoding.UTF8.GetBytes(s));

    private static Loader Create(InMemoryRowSink sink, params IPipelineHandler[] handlers)
        => new([new CsvRowReader(), new JsonRowReader()], handlers, sink);

    [TestMethod]
    public async Task Load_CsvWithDuplicate_SkipReplaceAndFailEachApplied()
    {
        const string csv = "id,name\n1,Ann\n2,Bob\n1,Anna\n";
        foreach (var (policy, written, replaced, skipped, errors, name) in new[]
        {
            (DuplicatePolicy.Skip, 2, 0, 1, 0, "Ann"),
            (DuplicatePolicy.Replace, 2, 1, 0, 0, "Anna"),
            (DuplicatePolicy.Fail, 2, 0, 0, 1, "Ann"),
        })
        {
            var sink = new InMemoryRowSink("id");
            var report = await Create(sink).LoadAsync(Text(csv), "csv", new LoadOptions { Duplicates = policy }, CancellationToken.None);
            TestContext.WriteLine($"{policy}: written {report.Written}, replaced {report.Replaced}, skipped {report.Skipped}, errors {report.Errors.Count}");

            Assert.AreEqual(written, report.Written, policy.ToString());
            Assert.AreEqual(replaced, report.Replaced, policy.ToString());
            Assert.AreEqual(skipped, report.Skipped, policy.ToString());
            Assert.AreEqual(errors, report.Errors.Count, policy.ToString());
            Assert.AreEqual(name, sink.Rows["1"]["name"], policy.ToString());
        }
    }

    [TestMethod]
    public async Task Load_JsonArray_ReadsRows()
    {
        var sink = new InMemoryRowSink("id");

        var report = await Create(sink).LoadAsync(Text("[{\"id\":1,\"name\":\"Ann\"},{\"id\":2,\"name\":null}]"), "json", new LoadOptions(), CancellationToken.None);

        Assert.AreEqual(2, report.Written);
        Assert.IsNull(sink.Rows["2"]["name"]);
    }

    [TestMethod]
    public async Task Load_InvalidRow_ReportedWithLineAndRestLoaded()
    {
        var sink = new InMemoryRowSink("id");

        var report = await Create(sink, new Required("name")).LoadAsync(Text("id,name\n1,Ann\n2,\n3,Cy\n"), "csv", new LoadOptions(), CancellationToken.None);
        TestContext.WriteLine($"errors: {string.Join(";", report.Errors.Select(e => $"{e.Line}:{e.Message}"))}");

        Assert.AreEqual(2, report.Written);
        Assert.AreEqual(1, report.Errors.Count);
        Assert.AreEqual(3, report.Errors[0].Line);
    }

    [TestMethod]
    public async Task Load_StopOnError_StopsAtFirstRejectedRow()
    {
        var sink = new InMemoryRowSink("id");

        var report = await Create(sink, new Required("name")).LoadAsync(Text("id,name\n1,\n2,Bob\n"), "csv", new LoadOptions { StopOnError = true }, CancellationToken.None);

        Assert.AreEqual(0, report.Written);
        Assert.AreEqual(1, report.Errors.Count);
    }

    [TestMethod]
    public async Task Load_Handlers_RunInAscendingPriority()
    {
        var order = new List<int>();
        var sink = new InMemoryRowSink("id");

        await Create(sink, new Recorder(order, 30), new Recorder(order, 10), new Recorder(order, 20))
            .LoadAsync(Text("id\n1\n"), "csv", new LoadOptions(), CancellationToken.None);

        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, order);
    }

    [TestMethod]
    public async Task Load_QuotedCsvFields_CommasQuotesAndLineBreaks()
    {
        var sink = new InMemoryRowSink("id");

        await Create(sink).LoadAsync(Text("id,note\n1,\"a, b\"\n2,\"say \"\"hi\"\"\"\n3,\"two\nlines\"\n"), "csv", new LoadOptions(), CancellationToken.None);

        Assert.AreEqual("a, b", sink.Rows["1"]["note"]);
        Assert.AreEqual("say \"hi\"", sink.Rows["2"]["note"]);
        Assert.AreEqual("two\nlines", sink.Rows["3"]["note"]);
    }

    [TestMethod]
    public async Task Load_UnknownFormat_Throws()
    {
        await Assert.ThrowsExactlyAsync<global::System.NotSupportedException>(async () =>
            await Create(new InMemoryRowSink("id")).LoadAsync(Text(""), "xml", new LoadOptions(), CancellationToken.None));
    }
}
