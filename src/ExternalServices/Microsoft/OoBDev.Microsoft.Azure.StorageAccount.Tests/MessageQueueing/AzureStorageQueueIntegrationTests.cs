using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Microsoft.Azure.StorageAccount.MessageQueueing;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.StorageAccount.Tests.MessageQueueing;

/// <summary>
/// Integration tests for <see cref="QueueClientFactory"/> against the Azurite emulator in the Docker test stack.
/// </summary>
[TestClass]
public class AzureStorageQueueIntegrationTests
{
    private const string DefaultAzuriteConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;";

    private QueueClient? _queue;

    public required TestContext TestContext { get; set; }

    private QueueClient CreateQueue()
    {
        var connectionString = TestContext.GetPropertyOrDefault("AZURITE_CONNECTION_STRING", DefaultAzuriteConnectionString);
        var section = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Queue:ConnectionString"] = connectionString,
                ["Queue:QueueName"] = $"test-{Guid.NewGuid():N}",
            })
            .Build()
            .GetSection("Queue");

        _queue = new QueueClientFactory().Create(section);
        return _queue;
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (_queue != null)
            _ = await _queue.DeleteIfExistsAsync();
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendMessage_Azurite_ReceivesSameText()
    {
        // Stage
        var queue = CreateQueue();
        _ = await queue.CreateIfNotExistsAsync();
        var text = $"hello-{Guid.NewGuid():N}";

        // Test
        _ = await queue.SendMessageAsync(text);
        var received = await queue.ReceiveMessageAsync();

        // Assert
        Assert.IsNotNull(received.Value);
        Assert.AreEqual(text, received.Value.MessageText);

        // Verify
        _ = await queue.DeleteMessageAsync(received.Value.MessageId, received.Value.PopReceipt);
        var after = await queue.PeekMessageAsync();
        Assert.IsNull(after.Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task ReceiveMessage_EmptyQueue_ReturnsNothing()
    {
        // Stage
        var queue = CreateQueue();
        _ = await queue.CreateIfNotExistsAsync();

        // Test
        var received = await queue.ReceiveMessageAsync();

        // Assert
        Assert.IsNull(received.Value);
    }
}
