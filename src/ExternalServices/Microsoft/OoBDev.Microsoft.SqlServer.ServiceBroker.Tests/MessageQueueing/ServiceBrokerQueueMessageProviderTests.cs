using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;
using OoBDev.Microsoft.SqlServer.ServiceBroker.Tests.TestItems;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.Tests.MessageQueueing;

[TestClass]
public class ServiceBrokerQueueMessageProviderTests
{
    private string _database = "";
    private string _admin = "";

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public async Task Initialize()
    {
        _admin = TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING");
        _database = $"Test_{Guid.NewGuid():N}";

        await using var connection = new SqlConnection(_admin);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"CREATE DATABASE [{_database}]; ALTER DATABASE [{_database}] SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;", connection);
        await command.ExecuteNonQueryAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        SqlConnection.ClearAllPools();
        await using var connection = new SqlConnection(_admin);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"ALTER DATABASE [{_database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_database}];", connection);
        await command.ExecuteNonQueryAsync();
    }

    private IConfigurationSection Config(string? queue = null, string maxRetries = "3", string? connection = null) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["q:ConnectionString"] = connection ?? new SqlConnectionStringBuilder(_admin) { InitialCatalog = _database }.ConnectionString,
            ["q:QueueName"] = queue ?? "orders",
            ["q:ServiceName"] = "//test/orders",
            ["q:MaxRetries"] = maxRetries,
            ["q:ReceiveTimeoutSeconds"] = "1",
            ["q:EnsureObjects"] = "true",
        }).Build().GetSection("q");

    private static ServiceBrokerQueueMessageProvider Provider() =>
        new(new DefaultJsonSerializer(), new BrokerConnectionFactory(), NullLogger<ServiceBrokerQueueMessageProvider>.Instance);

    private async Task<int> QueueDepthAsync(string queue = "orders")
    {
        await using var connection = new SqlConnection(new SqlConnectionStringBuilder(_admin) { InitialCatalog = _database }.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"SELECT COUNT(*) FROM [{queue}] WITH (NOLOCK)", connection);
        return (int)(await command.ExecuteScalarAsync())!;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(100);
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsync_ThenReceive_DeliversTheBodyOnce()
    {
        // Stage
        var config = Config();
        var provider = Provider();
        var handler = new TestHandlerProvider(config);
        using var cts = new CancellationTokenSource();

        // Test
        var correlationId = await provider.SendAsync(new { hello = "world" }, new TestMessageContext(config) { CorrelationId = "c-1" });
        provider.SetHandlerProvider(handler);
        var run = provider.RunAsync(cts.Token);
        await WaitUntilAsync(() => !handler.Received.IsEmpty);
        await Task.Delay(2500);
        await cts.CancelAsync();
        await run;

        // Assert
        Assert.AreEqual("c-1", correlationId);
        Assert.AreEqual(1, handler.Calls);
        StringAssert.Contains(handler.Received.ToArray()[0], "world");
        Assert.AreEqual(0, await QueueDepthAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task RunAsync_HandlerFailsTwice_RetriesAndThenSucceeds()
    {
        // Stage
        var config = Config();
        var provider = Provider();
        var handler = new TestHandlerProvider(config, failFirst: 2);
        using var cts = new CancellationTokenSource();
        await provider.SendAsync(new { retry = "me" }, new TestMessageContext(config) { CorrelationId = "c-2" });

        // Test
        provider.SetHandlerProvider(handler);
        var run = provider.RunAsync(cts.Token);
        await WaitUntilAsync(() => !handler.Received.IsEmpty);
        await cts.CancelAsync();
        await run;

        // Assert
        Assert.AreEqual(3, handler.Calls);
        Assert.AreEqual(1, handler.Received.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task RunAsync_HandlerAlwaysFails_EndsTheConversationAndKeepsTheQueueEnabled()
    {
        // Stage
        var config = Config(maxRetries: "2");
        var provider = Provider();
        var handler = new TestHandlerProvider(config, failFirst: int.MaxValue);
        using var cts = new CancellationTokenSource();
        await provider.SendAsync(new { poison = true }, new TestMessageContext(config) { CorrelationId = "c-3" });

        // Test
        provider.SetHandlerProvider(handler);
        var run = provider.RunAsync(cts.Token);
        await WaitUntilAsync(() => handler.Calls >= 2);
        await Task.Delay(2500);
        await cts.CancelAsync();
        await run;

        // Assert
        Assert.AreEqual(2, handler.Calls);
        Assert.IsTrue(handler.Received.IsEmpty);

        await using var connection = new SqlConnection(new SqlConnectionStringBuilder(_admin) { InitialCatalog = _database }.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT is_receive_enabled FROM sys.service_queues WHERE name = N'orders'", connection);
        Assert.AreEqual(true, await command.ExecuteScalarAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task EnsureObjects_RunTwice_ChangesNothing()
    {
        // Stage
        var config = Config();
        var options = ServiceBrokerOptions.From(config);
        var factory = new BrokerConnectionFactory();

        // Test / Assert
        await using (await factory.OpenAsync(options)) { }
        await using (await factory.OpenAsync(options)) { }
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task OpenAsync_BrokerDisabled_ThrowsInvalidOperation()
    {
        // Stage
        await using (var connection = new SqlConnection(_admin))
        {
            await connection.OpenAsync();
            await using var command = new SqlCommand($"ALTER DATABASE [{_database}] SET DISABLE_BROKER WITH ROLLBACK IMMEDIATE;", connection);
            await command.ExecuteNonQueryAsync();
        }

        // Test / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => new BrokerConnectionFactory().OpenAsync(ServiceBrokerOptions.From(Config())));
    }
}
