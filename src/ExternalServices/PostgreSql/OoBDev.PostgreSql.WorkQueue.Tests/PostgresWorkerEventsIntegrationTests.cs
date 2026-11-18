using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;
using OoBDev.WorkQueue;

namespace OoBDev.PostgreSql.WorkQueue.Tests;

[TestClass]
[TestCategory(TestCategories.Integration)]
public class PostgresWorkerEventsIntegrationTests
{
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 17, 8, 0, 0, TimeSpan.Zero));
    private PostgresWorkQueueOptions _postgres = new();
    private PostgresWorkerEvents _events = null!;

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public async Task Init()
    {
        var connectionString = TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING");
        if (!connectionString.Contains("oobdev_test", StringComparison.Ordinal))
            Assert.Inconclusive("The connection string is not a test database.");

        _postgres = new PostgresWorkQueueOptions { ConnectionString = connectionString, Schema = $"we_{Guid.NewGuid():N}" };
        var retention = new WorkerEventsOptions { MaxRecords = 3, MaxAge = TimeSpan.FromHours(1) };
        _events = new PostgresWorkerEvents(Options.Create(retention), Options.Create(_postgres), _clock);
        await _events.MigrateAsync(_postgres);
        await _events.MigrateAsync(_postgres);
        TestContext.WriteLine("Schema {0}", _postgres.Schema);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_events is null)
            return;
        await using var connection = new NpgsqlConnection(_postgres.ConnectionString);
        await connection.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{_postgres.Schema}\" CASCADE", connection);
        await drop.ExecuteNonQueryAsync();
        await _events.DisposeAsync();
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Read_ResumesFromTheStoredMarkWithoutDuplicates()
    {
        foreach (var m in new[] { "a", "b", "c" })
            await _events.PublishAsync("hash", m);

        var first = await _events.ReadAsync("log", 2, fromStart: true);
        await _events.PublishAsync("hash", "d");
        var second = await _events.ReadAsync("log", 10, fromStart: true);

        TestContext.WriteLine("First {0}, second {1}", string.Join(",", first.Select(e => e.Message)), string.Join(",", second.Select(e => e.Message)));
        CollectionAssert.AreEqual(new[] { "a", "b" }, first.Select(e => e.Message).ToArray());
        CollectionAssert.AreEqual(new[] { "c", "d" }, second.Select(e => e.Message).ToArray());
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Read_NewReader_StartsAtTheEnd()
    {
        await _events.PublishAsync("hash", "old");

        var first = await _events.ReadAsync("tab", 10);
        await _events.PublishAsync("hash", "new");
        var second = await _events.ReadAsync("tab", 10);

        Assert.AreEqual(0, first.Count);
        Assert.AreEqual("new", second.Single().Message);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Trim_ByCountAndAge_DeletesOldEventsAndIdleReaders()
    {
        foreach (var m in new[] { "a", "b", "c", "d", "e" })
            await _events.PublishAsync("hash", m);
        await _events.ReadAsync("tab", 10);

        var byCount = await _events.TrimAsync();
        _clock.Advance(TimeSpan.FromHours(2));
        var byAge = await _events.TrimAsync();
        var returning = await _events.ReadAsync("tab", 10, fromStart: true);

        TestContext.WriteLine("Trimmed {0} by count, {1} by age", byCount, byAge);
        Assert.AreEqual(2, byCount);
        Assert.AreEqual(3, byAge);
        Assert.AreEqual(0, returning.Count);
    }
}
