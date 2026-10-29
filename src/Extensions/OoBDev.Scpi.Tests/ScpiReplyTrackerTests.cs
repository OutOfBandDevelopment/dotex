using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Scpi.Tests;

[TestClass]
public class ScpiReplyTrackerTests
{
    private static readonly ScpiRequest Query = new("MEAS?\n", true, ScpiResponseFormat.Text);
    private static readonly ScpiRequest Command = new("OUT 1\n", false, ScpiResponseFormat.Text);

    private static (ScpiReplyTracker Tracker, List<ScpiRequest> Sent) Tracker(bool accept = true, TimeSpan? timeout = null)
    {
        var sent = new List<ScpiRequest>();
        return (new ScpiReplyTracker(r =>
        {
            sent.Add(r);
            return Task.FromResult(accept);
        }, timeout), sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task QueryAsync_ReplyArrives_ReturnsItAsync()
    {
        // Stage
        var (tracker, sent) = Tracker();

        // Test
        var task = tracker.QueryAsync(Query);
        tracker.OnReply(new ScpiReply("1.5"));
        var reply = await task.ConfigureAwait(false);

        // Assert
        Assert.AreEqual("1.5", reply.Text);
        Assert.AreEqual(1, sent.Count);
        Assert.AreEqual(0, tracker.Open);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task QueryAsync_TwoQueries_RepliesPairInOrderAsync()
    {
        // Stage
        var (tracker, _) = Tracker();
        var first = tracker.QueryAsync(Query);
        var second = tracker.QueryAsync(Query);

        // Test
        tracker.OnReply(new ScpiReply("a"));
        tracker.OnReply(new ScpiReply("b"));

        // Assert
        Assert.AreEqual("a", (await first.ConfigureAwait(false)).Text);
        Assert.AreEqual("b", (await second.ConfigureAwait(false)).Text);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task QueryAsync_TimesOut_LateReplyIsDiscardedAndOrderHoldsAsync()
    {
        // Stage
        var (tracker, _) = Tracker(timeout: TimeSpan.FromMilliseconds(50));
        var unsolicited = new List<string>();
        tracker.Unsolicited += (_, r) => unsolicited.Add(r.Text);
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => tracker.QueryAsync(Query)).ConfigureAwait(false);
        var next = tracker.QueryAsync(Query);

        // Test
        tracker.OnReply(new ScpiReply("late"));
        tracker.OnReply(new ScpiReply("fresh"));

        // Assert
        Assert.AreEqual("fresh", (await next.ConfigureAwait(false)).Text);
        Assert.AreEqual(0, unsolicited.Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task QueryAsync_Cancelled_ThrowsAndDiscardsReplyAsync()
    {
        // Stage
        var (tracker, _) = Tracker();
        using var cancel = new CancellationTokenSource();
        var task = tracker.QueryAsync(Query, cancel.Token);

        // Test
        await cancel.CancelAsync().ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => task).ConfigureAwait(false);
        tracker.OnReply(new ScpiReply("late"));
        Assert.AreEqual(0, tracker.Open);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task QueryAsync_DeviceRefuses_ThrowsAndLeavesNothingOpenAsync()
    {
        // Stage
        var (tracker, _) = Tracker(accept: false);

        // Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => tracker.QueryAsync(Query)).ConfigureAwait(false);
        Assert.AreEqual(0, tracker.Open);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task QueryAsync_NotAQuery_ThrowsAsync()
    {
        // Stage
        var (tracker, _) = Tracker();

        // Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => tracker.QueryAsync(Command)).ConfigureAwait(false);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void OnReply_NothingOpen_RaisesUnsolicited()
    {
        // Stage
        var (tracker, _) = Tracker();
        ScpiReply? heard = null;
        tracker.Unsolicited += (_, r) => heard = r;

        // Test
        tracker.OnReply(new ScpiReply("boot"));

        // Assert
        Assert.AreEqual("boot", heard?.Text);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_Command_ForwardsWithoutWaitingAsync()
    {
        // Stage
        var (tracker, sent) = Tracker();

        // Test
        var accepted = await tracker.SendAsync(Command).ConfigureAwait(false);

        // Assert
        Assert.IsTrue(accepted);
        Assert.AreEqual(1, sent.Count);
        Assert.AreEqual(0, tracker.Open);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Constructor_NullSend_Throws()
    {
        // Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => new ScpiReplyTracker(null!));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task IdentifyAsync_KnownInstrument_ReturnsItsProfileAsync()
    {
        // Stage
        var (tracker, sent) = Tracker();

        // Test
        var task = ScpiAutoDetect.IdentifyAsync(tracker, ScpiProfileCatalog.Create(), "\r\n");
        tracker.OnReply(new ScpiReply("HEWLETT-PACKARD,34401A,0,10-5-2"));
        var (profile, idn) = await task.ConfigureAwait(false);

        // Assert
        Assert.AreEqual("*IDN?\r\n", sent[0].WireText);
        Assert.AreNotEqual(ScpiProfileCatalog.Generic.Name, profile.Name);
        Assert.AreEqual("HEWLETT-PACKARD,34401A,0,10-5-2", idn);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task IdentifyAsync_UnknownInstrument_FallsBackToGenericAsync()
    {
        // Stage
        var (tracker, sent) = Tracker();

        // Test
        var task = ScpiAutoDetect.IdentifyAsync(tracker, ScpiProfileCatalog.Create());
        tracker.OnReply(new ScpiReply("ACME,Widget,1,2"));
        var (profile, _) = await task.ConfigureAwait(false);

        // Assert
        Assert.AreEqual("*IDN?\n", sent[0].WireText);
        Assert.AreSame(ScpiProfileCatalog.Generic, profile);
    }
}
