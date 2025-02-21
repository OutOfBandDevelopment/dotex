using OoBDev.Azure.StorageAccount.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace OoBDev.Azure.StorageAccount.Tests.MessageQueueing;

[TestClass]
public class AzureStorageQueueMapperTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow(null, 1000)]
    [DataRow("100", 100)]
    [DataRow("5000", 5000)]
    public void WaitDelayTest(string? input, int expected)
    {
        var mapper = new AzureStorageQueueMapper();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"WaitDelay", input }
            })
            .Build();

        var result = mapper.WaitDelay(config);
        Assert.AreEqual(expected, result);
    }

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow(null, false)]
    [DataRow("false", false)]
    [DataRow("False", false)]
    [DataRow("True", true)]
    [DataRow("true", true)]
    public void EnsureQueueExistsTest(string? input, bool expected)
    {
        var mapper = new AzureStorageQueueMapper();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"EnsureQueueExists", input }
            })
            .Build();

        var result = mapper.EnsureQueueExists(config);
        Assert.AreEqual(expected, result);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void WrapTest()
    {
        var message = new TestMessage();

        var messageContext = new MessageContext()
        {
            CorrelationId = "HelloWorld!",
            Headers ={
                ["Name1"] = "Header1"
            }
        };

        var mapper = new AzureStorageQueueMapper();
        var result = mapper.Wrap(message, messageContext);

        var type = Type.GetType(result.PayloadType);

        Assert.IsTrue(type?.IsInstanceOfType(message) ?? false);
        Assert.AreEqual("HelloWorld!", result.CorrelationId);
        Assert.AreEqual("Header1", result.Properties["Name1"]);
    }

    public class MessageContext : IMessageContext
    {
        public object? this[string key]
        {
            get => Headers[key];
            set => Headers[key] = value;
        }

        public string? OriginMessageId { get; }
        public string? CorrelationId { get; set; }
        public string? RequestId { get; }
        public string? SentId { get; set; }
        public string? ChannelType { get; }
        public string? MessageType { get; }
        public DateTimeOffset? SentAt { get; }
        public string? SentBy { get; }
        public string? SentFrom { get; }
        public Dictionary<string, object?> Headers { get; } = [];
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IConfigurationSection Config { get; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class TestMessage
    {
    }
}
