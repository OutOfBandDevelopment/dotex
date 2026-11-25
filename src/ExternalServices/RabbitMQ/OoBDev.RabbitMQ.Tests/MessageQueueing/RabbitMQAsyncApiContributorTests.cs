using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.RabbitMQ.MessageQueueing;
using System.Collections.Generic;
using System.Text.Json;

namespace OoBDev.RabbitMQ.Tests.MessageQueueing;

[TestClass]
public class RabbitMQAsyncApiContributorTests
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
    public void Describe_HostPortAndQueue_BuildsServerAndAmqpBinding()
    {
        // Test
        var result = new RabbitMQAsyncApiContributor().Describe(Section(("HostName", "mq.local"), ("Port", "5673"), ("QueueName", "jobs")));

        // Assert
        Assert.AreEqual("rabbitmq", result!.ServerName);
        Assert.AreEqual("mq.local:5673", result.Server.Host);
        Assert.AreEqual("amqp", result.Server.Protocol);
        Assert.AreEqual("jobs", result.Address);
        Assert.AreEqual("queue", (string?)result.Bindings!["amqp"]!["is"]);
        Assert.AreEqual("jobs", (string?)result.Bindings["amqp"]!["queue"]!["name"]);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_Defaults_UseLocalhostWithoutPortOrBinding()
    {
        // Test
        var result = new RabbitMQAsyncApiContributor().Describe(Section());

        // Assert
        Assert.AreEqual("localhost", result!.Server.Host);
        Assert.IsNull(result.Address);
        Assert.IsNull(result.Bindings);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_UserNameAndPassword_AreNeverCopiedIntoTheContribution()
    {
        // Test
        var result = new RabbitMQAsyncApiContributor().Describe(Section(("QueueName", "jobs"), ("UserName", "svc-account"), ("Password", "hunter2")));

        // Assert
        var text = JsonSerializer.Serialize(result);
        Assert.IsFalse(text.Contains("hunter2"));
        Assert.IsFalse(text.Contains("svc-account"));
    }
}
