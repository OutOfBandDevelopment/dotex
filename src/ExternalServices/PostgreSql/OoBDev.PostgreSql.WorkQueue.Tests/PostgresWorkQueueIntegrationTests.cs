using System;
using OoBDev.TestUtilities.Containers;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.TestUtilities;
using OoBDev.WorkQueue;

namespace OoBDev.PostgreSql.WorkQueue.Tests;

[TestClass]
[TestCategory(TestCategories.Integration)]
public class PostgresWorkQueueIntegrationTests
{
    private const string Stage = "hash";

    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 16, 8, 0, 0, TimeSpan.Zero));
    private readonly WorkQueueOptions _queueOptions = new() { BackoffBase = TimeSpan.FromSeconds(30) };
    private PostgresWorkQueueOptions _postgres = new();
    private PostgresWorkQueue _queue = null!;

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public async Task Init()
    {
        var connectionString = TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING");
        if (!connectionString.Contains("oobdev_test", StringComparison.Ordinal))
            Assert.Inconclusive("The connection string is not a test database.");

        _postgres = new PostgresWorkQueueOptions { ConnectionString = connectionString, Schema = $"wq_{Guid.NewGuid():N}" };
        _queue = new PostgresWorkQueue(Options.Create(_queueOptions), Options.Create(_postgres), new OldestFirstClaimOrder(), _clock);
        await _queue.MigrateAsync(_postgres);
        await _queue.MigrateAsync(_postgres);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_queue is null)
            return;
        await using var connection = new NpgsqlConnection(_postgres.ConnectionString);
        await connection.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{_postgres.Schema}\" CASCADE", connection);
        await drop.ExecuteNonQueryAsync();
        await _queue.DisposeAsync();
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Enqueue_IsIdempotent()
    {
        Assert.IsTrue(await _queue.EnqueueAsync(Stage, "a"));
        Assert.IsFalse(await _queue.EnqueueAsync(Stage, "a"));
        Assert.AreEqual(1, (await _queue.StatsAsync(Stage)).Claimable);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Claim_ConcurrentWorkers_EachItemGoesToExactlyOne()
    {
        for (var i = 0; i < 20; i++)
            await _queue.EnqueueAsync(Stage, $"k{i:00}");

        var batches = await Task.WhenAll(Enumerable.Range(0, 5).Select(w => _queue.ClaimAsync(Stage, $"w{w}", 8)));

        var keys = batches.SelectMany(b => b).Select(i => i.Key).ToList();
        Assert.AreEqual(20, keys.Count);
        Assert.AreEqual(20, keys.Distinct().Count());
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Claim_TakesOldestFirstUpToBatch()
    {
        await _queue.EnqueueAsync(Stage, "b");
        _clock.Advance(TimeSpan.FromSeconds(1));
        await _queue.EnqueueAsync(Stage, "a");
        _clock.Advance(TimeSpan.FromSeconds(1));
        await _queue.EnqueueAsync(Stage, "c");

        var items = await _queue.ClaimAsync(Stage, "w", 2);

        CollectionAssert.AreEquivalent(new[] { "b", "a" }, items.Select(i => i.Key).ToArray());
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Complete_FromAWorkerWhoseLeaseExpired_IsRejected()
    {
        await _queue.EnqueueAsync(Stage, "a");
        var old = (await _queue.ClaimAsync(Stage, "w1", 1))[0];

        _clock.Advance(_queueOptions.Default.Lease + TimeSpan.FromSeconds(1));
        Assert.AreEqual(1, (await _queue.StatsAsync(Stage)).LeaseExpired);
        var fresh = (await _queue.ClaimAsync(Stage, "w2", 1))[0];

        Assert.AreEqual(2, fresh.Attempt);
        Assert.IsFalse(await _queue.CompleteAsync(old));
        Assert.IsNull(await _queue.FailAsync(old, "late"));
        Assert.IsFalse(await _queue.HeartbeatAsync(old));
        Assert.IsTrue(await _queue.HeartbeatAsync(fresh));
        Assert.IsTrue(await _queue.CompleteAsync(fresh, WorkStatus.Skipped, "nothing to do"));
        Assert.AreEqual(0, (await _queue.StatsAsync(Stage)).Running);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Fail_Retryable_WaitsForTheBackoff_ThenDiesOnTheLastAttempt()
    {
        await _queue.EnqueueAsync(Stage, "a");

        for (var attempt = 1; attempt <= _queueOptions.Default.MaxAttempts; attempt++)
        {
            var item = (await _queue.ClaimAsync(Stage, "w", 1))[0];
            Assert.AreEqual(attempt, item.Attempt);
            var status = await _queue.FailAsync(item, new string('x', 600));
            Assert.AreEqual(attempt == _queueOptions.Default.MaxAttempts ? WorkStatus.Dead : WorkStatus.Pending, status);
            if (status == WorkStatus.Pending)
            {
                Assert.AreEqual(0, (await _queue.StatsAsync(Stage)).Claimable);
                Assert.AreEqual(0, (await _queue.ClaimAsync(Stage, "w", 1)).Count);
                _clock.Advance(_queueOptions.Backoff(attempt));
            }
        }

        var stats = await _queue.StatsAsync(Stage);
        Assert.AreEqual(1, stats.Dead);
        var letter = (await _queue.DeadLettersAsync(Stage)).Single();
        Assert.AreEqual("attempts exhausted", letter.Reason);
        Assert.AreEqual(500, letter.LastError!.Length);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Fail_NonRetryable_IsDeadAtOnce()
    {
        await _queue.EnqueueAsync(Stage, "a");
        var item = (await _queue.ClaimAsync(Stage, "w", 1))[0];

        Assert.AreEqual(WorkStatus.Dead, await _queue.FailAsync(item, "bad", retryable: false));

        Assert.AreEqual("non-retryable", (await _queue.DeadLettersAsync(Stage)).Single().Reason);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Claim_ParksAnItemThatKeptHanging()
    {
        await _queue.EnqueueAsync(Stage, "a");
        for (var attempt = 0; attempt < _queueOptions.Default.MaxAttempts; attempt++)
        {
            Assert.AreEqual(1, (await _queue.ClaimAsync(Stage, "w", 1)).Count);
            _clock.Advance(_queueOptions.Default.Lease + TimeSpan.FromSeconds(1));
        }

        Assert.AreEqual(0, (await _queue.ClaimAsync(Stage, "w", 1)).Count);

        Assert.AreEqual(1, (await _queue.StatsAsync(Stage)).Parked);
        Assert.AreEqual("lease expired", (await _queue.DeadLettersAsync(Stage)).Single().Reason);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Redrive_ReturnsDeadAndParkedWithAFreshBudget()
    {
        await _queue.EnqueueAsync(Stage, "a");
        await _queue.EnqueueAsync(Stage, "b");
        foreach (var item in await _queue.ClaimAsync(Stage, "w", 2))
            await _queue.FailAsync(item, "x", retryable: false);

        Assert.AreEqual(1, await _queue.RedriveAsync(Stage, "a"));
        Assert.AreEqual(1, await _queue.RedriveAsync(Stage));
        Assert.AreEqual(0, await _queue.RedriveAsync(Stage));

        var again = await _queue.ClaimAsync(Stage, "w", 5);
        Assert.AreEqual(2, again.Count);
        Assert.IsTrue(again.All(i => i.Attempt == 1));
        Assert.IsTrue((await _queue.DeadLettersAsync(Stage)).All(d => d.RedrivenAt is not null));
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Complete_RejectsAnUnknownFinalStatus()
    {
        await _queue.EnqueueAsync(Stage, "a");
        var item = (await _queue.ClaimAsync(Stage, "w", 1))[0];

        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => _queue.CompleteAsync(item, WorkStatus.Dead));
    }
}
