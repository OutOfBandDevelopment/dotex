using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Logging.Tests;

/// <summary>Checkpoint records: the recorder writes them, playback restores presenter state from them on a backward seek.</summary>
[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Logging)]
public sealed class PlaybackCheckpointTests
{
    private static readonly DateTimeOffset _t0 = new(2026, 11, 12, 12, 0, 0, TimeSpan.Zero);

    public TestContext TestContext { get; set; } = null!;

    private static SessionLogRecord Rx(double seconds, string text) =>
        new() { Kind = SessionLogRecordKind.Rx, Timestamp = _t0.AddSeconds(seconds), Data = Encoding.ASCII.GetBytes(text) };

    private static SessionLogRecord Checkpoint(double seconds) =>
        new() { Kind = SessionLogRecordKind.Checkpoint, Timestamp = _t0.AddSeconds(seconds) };

    // "ab" "c" [checkpoint] "d\r" "ef\r": the first line is split across the checkpoint, so only restored state yields "abcd".
    private static SessionLog Log() => new(
        new SessionLogHeader { Created = _t0, Presenters = ["ascii"] },
        [Rx(0, "ab"), Rx(1, "c"), Checkpoint(2), Rx(3, "d\r"), Rx(4, "ef\r")]);

    private static Pipeline Ascii() => new([new AsciiPresenter(Options.Create(new AsciiPresenterOptions()))]);

    private sealed class Counter
    {
        public int Renders { get; set; }
    }

    private sealed class CountingPresenter(Counter counter) : ISnapshotPresenter
    {
        private readonly AsciiPresenter _inner = new(Options.Create(new AsciiPresenterOptions()));

        public string Name => "ascii";

        public IReadOnlyList<string> Render(ReadOnlySequence<byte> data)
        {
            counter.Renders++;
            return _inner.Render(data);
        }

        public object? Snapshot() => _inner.Snapshot();

        public void Restore(object? snapshot) => _inner.Restore(snapshot);
    }

    private sealed class PlainPresenter(Counter counter) : IPresenter
    {
        private readonly AsciiPresenter _inner = new(Options.Create(new AsciiPresenterOptions()));

        public string Name => "ascii";

        public IReadOnlyList<string> Render(ReadOnlySequence<byte> data)
        {
            counter.Renders++;
            return _inner.Render(data);
        }
    }

    private static string[] Lines(PlaybackBatch batch) => [.. batch.Items.SelectMany(i => i.Outputs).Select(o => o.Text)];

    [TestMethod]
    public void BackwardSeek_RestartsFromTheCheckpoint_AndKeepsTheWholeTranscript()
    {
        var counter = new Counter();
        var engine = new PlaybackEngine(Log(), () => new Pipeline([new CountingPresenter(counter)]));
        engine.SkipToEnd();
        Assert.AreEqual(4, counter.Renders);

        var batch = engine.SeekTo(4);

        Assert.IsTrue(batch.Reset);
        Assert.AreEqual(4, engine.Position);
        CollectionAssert.AreEqual(new[] { "abcd" }, Lines(batch));
        Assert.HasCount(4, batch.Items);
        Assert.AreEqual(5, counter.Renders, "Only the record after the checkpoint is rendered again; the earlier ones come from the saved outputs.");

        CollectionAssert.AreEqual(new[] { "ef" }, Lines(engine.SeekTo(5)));
    }

    [TestMethod]
    public void BackwardSeek_BeforeAnyCheckpoint_ReplaysFromTheStart()
    {
        var counter = new Counter();
        var engine = new PlaybackEngine(Log(), () => new Pipeline([new CountingPresenter(counter)]));
        engine.SkipToEnd();

        var batch = engine.SeekTo(1);

        Assert.IsTrue(batch.Reset);
        Assert.AreEqual(1, engine.Position);
        Assert.IsEmpty(Lines(batch));
        Assert.AreEqual(5, counter.Renders);
    }

    [TestMethod]
    public void BackwardSeek_WithAPresenterThatCannotRestore_ReplaysFromTheStart()
    {
        var counter = new Counter();
        var engine = new PlaybackEngine(Log(), () => new Pipeline([new PlainPresenter(counter)]));
        engine.SkipToEnd();

        var batch = engine.SeekTo(4);

        CollectionAssert.AreEqual(new[] { "abcd" }, Lines(batch));
        Assert.AreEqual(4 + 3, counter.Renders);
    }

    [TestMethod]
    public void ANoteInsertedBeforeALaterCheckpoint_DoesNotLeaveAStaleSnapshot()
    {
        var engine = new PlaybackEngine(Log(), Ascii);
        engine.SkipToEnd();
        engine.SeekTo(1);
        engine.AddNote("look here");

        var batch = engine.SeekTo(5);

        Assert.AreEqual(5, engine.Position);
        CollectionAssert.AreEqual(new[] { "abcd" }, Lines(batch));
        CollectionAssert.AreEqual(new[] { "ef" }, Lines(engine.SeekTo(6)));
        CollectionAssert.AreEqual(new[] { "abcd" }, Lines(engine.SeekTo(5)));
    }

    [TestMethod]
    public void ARestoredPipeline_MatchesAFullReplay_ForEveryPosition()
    {
        var engine = new PlaybackEngine(Log(), Ascii);
        engine.SkipToEnd();

        for (var position = engine.Count - 1; position >= 0; position--)
        {
            var restored = engine.SeekTo(position);
            var fresh = new PlaybackEngine(Log(), Ascii);

            CollectionAssert.AreEqual(Lines(fresh.SeekTo(position)), Lines(restored), $"position {position}");
        }
    }

    [TestMethod]
    public void ACheckpointRecord_RoundTripsThroughTheFileFormat()
    {
        var line = SessionLogFormat.WriteRecord(Checkpoint(2) with { Sequence = 7 });

        Assert.Contains("\"type\":\"checkpoint\"", line);
        var record = SessionLogFormat.ReadRecord(line);
        Assert.AreEqual(SessionLogRecordKind.Checkpoint, record.Kind);
        Assert.AreEqual(7L, record.Sequence);
    }

    [TestMethod]
    public void ACheckpointIsInvisibleInPlaybackText()
    {
        var engine = new PlaybackEngine(Log(), () => new Pipeline([]));

        Assert.IsEmpty(PlaybackText.Lines(engine.SeekTo(3)).Where(l => l.Text.Contains("heckpoint")).ToList());
    }

    [TestMethod]
    public async Task TheRecorder_WritesACheckpointAfterEveryNthReceivedChunk()
    {
        using var file = new TempFileHandle();
        var transport = new FakeTransport();
        await using var session = new Session(transport, new Pipeline([]));
        using (var logger = SessionLogger.Start(file.FilePath, new SessionLogHeader { Created = DateTimeOffset.MinValue, Presenters = ["ascii"] }, checkpointEvery: 2))
        {
            logger.Attach(session, "loopback://");
            await session.OpenAsync(TestContext.CancellationToken);
            await transport.PushIncomingAsync([0x78]);
            await WaitForAsync(() => logger.RecordCount == 3);
            await transport.PushIncomingAsync([0x78]);
            await WaitForAsync(() => logger.RecordCount == 5);
            await transport.PushIncomingAsync([0x78]);
            await WaitForAsync(() => logger.RecordCount == 6);
        }

        var log = SessionLog.Load(file.FilePath);
        Assert.AreEqual("session,open,rx,rx,checkpoint,rx", string.Join(",", log.Records.Select(r => SessionLogFormat.TypeName(r.Kind))));
    }

    [TestMethod]
    public void TheRecorder_RejectsANegativeInterval()
    {
        using var stream = new MemoryStream();
        using var writer = new SessionLogWriter(stream, new SessionLogHeader { Created = DateTimeOffset.MinValue });

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SessionLogger(writer, null, -1));
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                Assert.Fail("Timed out waiting for the logger.");
            }

            await Task.Delay(10);
        }
    }
}
