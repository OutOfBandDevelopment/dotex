using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Hosting;

namespace OoBDev.DevTerm.Core.Tests.Configuration;

[TestCategory("Unit")]
[TestClass]
public sealed class RoutingLinkFactoryTests
{
    [TestMethod]
    [DataRow("mqtt", "MqttRoutingLink")]
    [DataRow("MQTT", "MqttRoutingLink")]
    [DataRow("amqp", "BrokerRoutingLink")]
    [DataRow("stomp", "BrokerRoutingLink")]
    public async Task Create_Protocol_BuildsMatchingLink(string protocol, string expected)
    {
        // Stage
        var options = new RoutingOptions { Protocol = protocol, Host = "broker.local" };

        // Test
        await using var link = new RoutingLinkFactory().Create(options);

        // Assert
        Assert.AreEqual(expected, link.GetType().Name);
    }

    [TestMethod]
    public void Create_NullOptions_Throws()
    {
        // Test / Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => new RoutingLinkFactory().Create(null!));
    }

    [TestMethod]
    public void AddDevTermCore_Registered_ResolvesRoutingLinkFactory()
    {
        // Stage
        var services = new ServiceCollection().AddDevTermCore();

        // Test
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<RoutingLinkFactory>(provider.GetRequiredService<IRoutingLinkFactory>());
    }
}
