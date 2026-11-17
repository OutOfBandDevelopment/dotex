using Microsoft.Extensions.Configuration;
using OoBDev.TestUtilities.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.MessageQueueing.Tests;
using OoBDev.PostgreSql.Pgmq.MessageQueueing;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.PostgreSql.Pgmq.Tests;

[TestClass]
[MessageQueue(QueueConfig)]
public class PgmqIntegrationTests
{
    public const string QueueConfig = "pgmq-test";

    public required TestContext TestContext { get; set; }

    private string _queue = "";
    private string _connectionString = "";

    [TestInitialize]
    public void Init()
    {
        _queue = $"test_{Guid.NewGuid():N}";
        _connectionString = TestContext.GetRequiredProperty<string>("POSTGRES_CONNECTION_STRING");
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_queue.Length == 0)
            return;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        foreach (var name in new[] { _queue, $"{_queue}_dlq" })
        {
            await using var drop = new NpgsqlCommand("SELECT pgmq.drop_queue(@q)", connection);
            drop.Parameters.AddWithValue("q", name);
            try
            {
                await drop.ExecuteNonQueryAsync();
            }
            catch (PostgresException)
            {
                // the dead-letter queue exists only when MaxDeliveries is on
            }
        }
    }

    private IConfigurationRoot Config(string maxDeliveries = "5") => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        [$"MessageQueue:{QueueConfig}:Provider"] = PgmqGlobals.MessageProviderKey,
        [$"MessageQueue:{QueueConfig}:Config:ConnectionString"] = _connectionString,
        [$"MessageQueue:{QueueConfig}:Config:QueueName"] = _queue,
        [$"MessageQueue:{QueueConfig}:Config:VisibilityTimeoutSeconds"] = "1",
        [$"MessageQueue:{QueueConfig}:Config:PollIntervalMilliseconds"] = "50",
        [$"MessageQueue:{QueueConfig}:Config:MaxDeliveries"] = maxDeliveries,
    }).Build();

    [IntegrationTestMethod("oobd-test-postgres")]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsync_PutsOneMessageOnTheQueue()
    {
        // Stage
        var config = Config();
        var service = MessageSenderTests.GetServiceProvider(TestContext, config, s => s.TryAddPgmqQueueServices());
        var sender = service.GetRequiredService<IMessageQueueSender<PgmqIntegrationTests>>();

        // Test
        var correlationId = await sender.SendAsync(new { hello = "world" });

        // Assert
        Assert.IsNotNull(correlationId);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var count = new NpgsqlCommand($"SELECT count(*) FROM pgmq.q_{_queue}", connection);
        Assert.AreEqual(1L, await count.ExecuteScalarAsync());
    }

    [IntegrationTestMethod("oobd-test-postgres")]
    [TestCategory(TestCategories.Integration)]
    public async Task RunAsync_DeliversToHandlerAndArchives()
    {
        // Stage
        var config = Config();
        var service = MessageSenderTests.GetServiceProvider(TestContext, config, s => s.TryAddPgmqQueueServices());
        var sender = service.GetRequiredService<IMessageQueueSender<PgmqIntegrationTests>>();
        var correlationId = await sender.SendAsync(new { hello = "world" });

        var handler = new RecordingHandler(config.GetSection($"MessageQueue:{QueueConfig}:Config"));
        var receiver = service.GetRequiredService<IMessageReceiverProvider>().SetHandlerProvider(handler);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        // Test
        var run = receiver.RunAsync(cts.Token);
        await handler.Received.Task.WaitAsync(TimeSpan.FromSeconds(15));
        await cts.CancelAsync();
        await run;

        // Assert
        Assert.AreEqual(correlationId, handler.MessageId);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var count = new NpgsqlCommand($"SELECT count(*) FROM pgmq.q_{_queue}", connection);
        Assert.AreEqual(0L, await count.ExecuteScalarAsync());
        await using var archived = new NpgsqlCommand($"SELECT count(*) FROM pgmq.a_{_queue}", connection);
        Assert.AreEqual(1L, await archived.ExecuteScalarAsync());
    }

    [IntegrationTestMethod("oobd-test-postgres")]
    [TestCategory(TestCategories.Integration)]
    public async Task RunAsync_FailedHandler_RedeliversAfterVisibilityTimeout()
    {
        // Stage
        var config = Config();
        var service = MessageSenderTests.GetServiceProvider(TestContext, config, s => s.TryAddPgmqQueueServices());
        await service.GetRequiredService<IMessageQueueSender<PgmqIntegrationTests>>().SendAsync(new { hello = "again" });

        var handler = new RecordingHandler(config.GetSection($"MessageQueue:{QueueConfig}:Config"), failFirst: true);
        var receiver = service.GetRequiredService<IMessageReceiverProvider>().SetHandlerProvider(handler);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Test
        var run = receiver.RunAsync(cts.Token);
        await handler.Received.Task.WaitAsync(TimeSpan.FromSeconds(25));
        await cts.CancelAsync();
        await run;

        // Assert
        Assert.AreEqual(2, handler.Calls);
    }

    [IntegrationTestMethod("oobd-test-postgres")]
    [TestCategory(TestCategories.Integration)]
    public async Task RunAsync_AlwaysFailing_MovesMessageToDeadLetterQueue()
    {
        // Stage
        var config = Config(maxDeliveries: "2");
        var service = MessageSenderTests.GetServiceProvider(TestContext, config, s => s.TryAddPgmqQueueServices());
        await service.GetRequiredService<IMessageQueueSender<PgmqIntegrationTests>>().SendAsync(new { hello = "poison" });

        var handler = new RecordingHandler(config.GetSection($"MessageQueue:{QueueConfig}:Config"), failAlways: true);
        var receiver = service.GetRequiredService<IMessageReceiverProvider>().SetHandlerProvider(handler);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));

        // Test
        var run = receiver.RunAsync(cts.Token);
        long dead = 0;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        while (dead == 0 && !cts.IsCancellationRequested)
        {
            await Task.Delay(250);
            await using var count = new NpgsqlCommand($"SELECT count(*) FROM pgmq.q_{_queue}_dlq", connection);
            dead = (long)(await count.ExecuteScalarAsync())!;
        }

        await cts.CancelAsync();
        await run;

        // Assert
        Assert.AreEqual(1L, dead);
        Assert.AreEqual(2, handler.Calls);
        await using var main = new NpgsqlCommand($"SELECT count(*) FROM pgmq.q_{_queue}", connection);
        Assert.AreEqual(0L, await main.ExecuteScalarAsync());
    }

    private sealed class RecordingHandler(IConfigurationSection config, bool failFirst = false, bool failAlways = false) : IMessageHandlerProvider
    {
        public IConfigurationSection Config { get; } = config;
        public TaskCompletionSource Received { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string? MessageId { get; private set; }
        public int Calls { get; private set; }

        public Task HandleAsync(IQueueMessage message, string messageId)
        {
            Calls++;
            if (failAlways || (failFirst && Calls == 1))
                throw new InvalidOperationException("delivery fails");
            MessageId = messageId;
            Received.TrySetResult();
            return Task.CompletedTask;
        }
    }
}
