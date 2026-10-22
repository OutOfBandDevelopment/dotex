using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Microsoft.Azure.ServiceBus.MessageQueueing;
using System.Collections.Generic;
using System.Text.Json;

namespace OoBDev.Microsoft.Azure.ServiceBus.Tests.MessageQueueing;

[TestClass]
public class ServiceBusAsyncApiContributorTests
{
    private static IConfigurationSection Section(params (string Key, string Value)[] values)
    {
        var data = new Dictionary<string, string?>();
        foreach (var (key, value) in values)
        {
            data["q:" + key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(data).Build().GetSection("q");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_ConnectionStringWithEndpoint_UsesOnlyTheHost()
    {
        // Test
        var result = new ServiceBusAsyncApiContributor().Describe(Section(
            ("ConnectionString", "Endpoint=sb://contoso.servicebus.windows.net/;SharedAccessKeyName=root;SharedAccessKey=TOPSECRET"),
            ("QueueName", "orders")));

        // Assert
        Assert.AreEqual("servicebus", result!.ServerName);
        Assert.AreEqual("contoso.servicebus.windows.net", result.Server.Host);
        Assert.AreEqual("amqp", result.Server.Protocol);
        Assert.AreEqual("orders", result.Address);
        Assert.IsFalse(JsonSerializer.Serialize(result).Contains("TOPSECRET"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_TopicWithoutQueue_UsesTheTopicName()
    {
        // Test
        var result = new ServiceBusAsyncApiContributor().Describe(Section(("TopicName", "events")));

        // Assert
        Assert.AreEqual("events", result!.Address);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_NoConnectionString_FallsBackToAGenericHost()
    {
        // Test
        var result = new ServiceBusAsyncApiContributor().Describe(Section(("QueueName", "orders")));

        // Assert
        Assert.AreEqual("servicebus", result!.Server.Host);
    }
}
