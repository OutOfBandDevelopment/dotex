using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Tests;

/// <summary>Reply correlation of the line-reply base presenter.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class LineReplyPresenterTests
{
    private sealed class TestPresenter : LineReplyPresenter
    {
        public override string Name => "test";

        public Dictionary<string, string> Patterns { get; } = new();

        protected override ReplyMatch Classify(string replyId, string line) =>
            !Patterns.TryGetValue(replyId, out var prefix) ? ReplyMatch.NextLine
            : line.StartsWith(prefix, global::System.StringComparison.Ordinal) ? ReplyMatch.Matches : ReplyMatch.Rejects;
    }

    private static (TestPresenter Presenter, List<IReadOnlyDictionary<string, string>> Values) Create()
    {
        var presenter = new TestPresenter();
        var values = new List<IReadOnlyDictionary<string, string>>();
        presenter.ValuesChanged += (_, v) => values.Add(v);
        return (presenter, values);
    }

    private static IReadOnlyList<string> Feed(TestPresenter presenter, string text) =>
        presenter.Render(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes(text)));

    [TestMethod]
    public void Render_CrLf_YieldsOneLine()
    {
        var (presenter, _) = Create();

        var lines = Feed(presenter, "abc\r\ndef\r\n");

        CollectionAssert.AreEqual(new[] { "abc", "def" }, new List<string>(lines));
    }

    [TestMethod]
    public void Render_LfCr_YieldsOneLine()
    {
        var (presenter, _) = Create();

        var lines = Feed(presenter, "abc\n\rdef\n\r");

        CollectionAssert.AreEqual(new[] { "abc", "def" }, new List<string>(lines));
    }

    [TestMethod]
    public void Render_PendingQueries_AnswerInOrder()
    {
        var (presenter, values) = Create();
        presenter.QuerySent("a");
        presenter.QuerySent("b");

        Feed(presenter, "1\r\n2\r\n");

        Assert.AreEqual("1", values[0]["a"]);
        Assert.AreEqual("2", values[1]["b"]);
    }

    [TestMethod]
    public void Render_UnsolicitedLine_PublishesNothing()
    {
        var (presenter, values) = Create();

        Feed(presenter, "noise\r\n");

        Assert.AreEqual(0, values.Count);
    }

    [TestMethod]
    public void Render_MatchingPattern_SkipsAnOlderQuery()
    {
        var (presenter, values) = Create();
        presenter.Patterns["a"] = "A=";
        presenter.Patterns["b"] = "B=";
        presenter.QuerySent("a");
        presenter.QuerySent("b");

        Feed(presenter, "B=2\r\n");

        Assert.AreEqual("B=2", values[0]["b"]);
        Assert.IsFalse(values[0].ContainsKey("a"));
    }

    [TestMethod]
    public void Cancel_PendingQuery_LeavesTheNextLineUnsolicited()
    {
        var (presenter, values) = Create();
        presenter.QuerySent("a");
        presenter.Cancel("a");

        Feed(presenter, "x\r\n");

        Assert.AreEqual(0, values.Count);
    }

    [TestMethod]
    public void Reset_PendingQueryAndPartialLine_AreDropped()
    {
        var (presenter, values) = Create();
        presenter.QuerySent("a");
        Feed(presenter, "par");

        presenter.Reset();
        var lines = Feed(presenter, "tial\r\n");

        CollectionAssert.AreEqual(new[] { "tial" }, new List<string>(lines));
        Assert.AreEqual(0, values.Count);
    }

    [TestMethod]
    public void Render_TerminatorlessLink_CompletesEachChunk()
    {
        var (presenter, _) = Create();
        presenter.ConfigureTerminator(string.Empty);

        var lines = Feed(presenter, "OK");

        CollectionAssert.AreEqual(new[] { "OK" }, new List<string>(lines));
    }

    [TestMethod]
    public void Render_OverlongLine_FlushesWithoutConsumingTheQuery()
    {
        var (presenter, values) = Create();
        presenter.QuerySent("a");

        var lines = Feed(presenter, new string('x', 4096));
        Feed(presenter, "end\r\n");

        Assert.AreEqual(1, lines.Count);
        Assert.AreEqual("end", values[0]["a"]);
    }
}
