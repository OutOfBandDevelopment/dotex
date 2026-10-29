using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AsyncApi;
using OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.Tests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void TryAddServiceBrokerServices_RegistersProvidersFactoryAndContributor()
    {
        // Stage
        var services = new ServiceCollection();

        // Test
        services.TryAddServiceBrokerServices().TryAddServiceBrokerServices();

        // Assert
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IMessageSenderProvider) && d.ServiceKey is null));
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IMessageSenderProvider) && Equals(d.ServiceKey, ServiceBrokerGlobals.MessageProviderKey)));
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IMessageReceiverProvider) && Equals(d.ServiceKey, ServiceBrokerGlobals.MessageProviderKey)));
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IBrokerConnectionFactory)));
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IAsyncApiContributor)));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_ConnectionString_UsesOnlyTheServerAndQueue()
    {
        // Stage
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["q:ConnectionString"] = "Server=db.local,1433;User Id=sa;Password=hunter2;",
            ["q:QueueName"] = "orders",
        }).Build().GetSection("q");

        // Test
        var result = new ServiceBrokerAsyncApiContributor().Describe(config);

        // Assert
        Assert.AreEqual("db.local,1433", result!.Server.Host);
        Assert.AreEqual("orders", result.Address);
        Assert.IsFalse(JsonSerializer.Serialize(result).Contains("hunter2"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_UnparsableConnectionString_FallsBackToLocalhost()
    {
        // Stage
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["q:ConnectionString"] = "not=a=valid" }).Build().GetSection("q");

        // Test / Assert
        Assert.AreEqual("localhost", new ServiceBrokerAsyncApiContributor().Describe(config)!.Server.Host);
    }
}
