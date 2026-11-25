using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;
using OoBDev.System;
using System;
using System.Collections.Generic;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.Tests.MessageQueueing;

[TestClass]
public class ServiceBrokerOptionsTests
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

    private static (string, string)[] Required() =>
    [
        ("ConnectionString", "Server=.;Database=x"),
        ("QueueName", "orders"),
        ("ServiceName", "//svc/orders"),
    ];

    [TestMethod]
    [TestCategory("Unit")]
    public void From_RequiredKeysOnly_UsesDefaults()
    {
        // Test
        var options = ServiceBrokerOptions.From(Section(Required()));

        // Assert
        Assert.AreEqual("//svc/orders", options.TargetServiceName);
        Assert.AreEqual(ServiceBrokerGlobals.DefaultContractName, options.ContractName);
        Assert.AreEqual(3, options.MaxRetries);
        Assert.AreEqual(5, options.ReceiveTimeoutSeconds);
        Assert.IsFalse(options.EnsureObjects);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("ConnectionString")]
    [DataRow("QueueName")]
    [DataRow("ServiceName")]
    public void From_MissingRequiredKey_ThrowsConfigurationMissing(string missing)
    {
        // Stage
        var values = Array.FindAll(Required(), v => v.Item1 != missing);

        // Test / Assert
        var ex = Assert.Throws<ConfigurationMissingException>(() => ServiceBrokerOptions.From(Section(values)));
        StringAssert.Contains(ex.Message, missing);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void From_AllKeys_AreRead()
    {
        // Test
        var options = ServiceBrokerOptions.From(Section(
        [
            .. Required(),
            ("TargetServiceName", "//svc/target"),
            ("ContractName", "//c"),
            ("MaxRetries", "2"),
            ("ReceiveTimeoutSeconds", "9"),
            ("EnsureObjects", "true"),
        ]));

        // Assert
        Assert.AreEqual("//svc/target", options.TargetServiceName);
        Assert.AreEqual("//c", options.ContractName);
        Assert.AreEqual(2, options.MaxRetries);
        Assert.AreEqual(9, options.ReceiveTimeoutSeconds);
        Assert.IsTrue(options.EnsureObjects);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("a b")]
    [DataRow("a];DROP TABLE x;--")]
    [DataRow("a.b.c")]
    [DataRow("1abc")]
    public void From_QueueNameNotAnIdentifier_Throws(string queue)
    {
        // Stage
        var values = Array.ConvertAll(Required(), v => v.Item1 == "QueueName" ? (v.Item1, queue) : v);

        // Test / Assert
        Assert.Throws<InvalidOperationException>(() => ServiceBrokerOptions.From(Section(values)));
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("0")]
    [DataRow("5")]
    public void From_MaxRetriesOutOfRange_Throws(string retries)
    {
        // Test / Assert
        Assert.Throws<InvalidOperationException>(() => ServiceBrokerOptions.From(Section([.. Required(), ("MaxRetries", retries)])));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void From_TimeoutOutOfRange_Throws()
    {
        // Test / Assert
        Assert.Throws<InvalidOperationException>(() => ServiceBrokerOptions.From(Section([.. Required(), ("ReceiveTimeoutSeconds", "0")])));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void QuotedQueueName_SchemaQualified_QuotesEachPart()
    {
        // Test
        var options = ServiceBrokerOptions.From(Section([("ConnectionString", "x"), ("QueueName", "app.orders"), ("ServiceName", "s")]));

        // Assert
        Assert.AreEqual("[app].[orders]", options.QuotedQueueName);
    }
}
