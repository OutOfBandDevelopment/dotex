using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.MessageQueueing.Services;
using OoBDev.PostgreSql.Pgmq.MessageQueueing;
using OoBDev.System;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.PostgreSql.Pgmq.Tests;

[TestClass]
public class PgmqSettingsTests
{
    private static IConfigurationSection Section(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build().GetSection("q");

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void From_AppliesDefaults()
    {
        var settings = PgmqSettings.From(Section(new() { ["q:ConnectionString"] = "Host=x", ["q:QueueName"] = "orders" }));

        Assert.AreEqual("Host=x", settings.ConnectionString);
        Assert.AreEqual("orders", settings.QueueName);
        Assert.AreEqual(TimeSpan.FromSeconds(30), settings.VisibilityTimeout);
        Assert.AreEqual(TimeSpan.FromMilliseconds(500), settings.PollInterval);
        Assert.AreEqual(10, settings.BatchSize);
        Assert.AreEqual(5, settings.MaxDeliveries);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void From_ReadsOverrides()
    {
        var settings = PgmqSettings.From(Section(new()
        {
            ["q:ConnectionString"] = "Host=x",
            ["q:QueueName"] = "orders",
            ["q:VisibilityTimeoutSeconds"] = "5",
            ["q:PollIntervalMilliseconds"] = "50",
            ["q:BatchSize"] = "3",
        }));

        Assert.AreEqual(TimeSpan.FromSeconds(5), settings.VisibilityTimeout);
        Assert.AreEqual(TimeSpan.FromMilliseconds(50), settings.PollInterval);
        Assert.AreEqual(3, settings.BatchSize);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void From_MissingQueueName_Throws()
        => Assert.ThrowsExactly<ConfigurationMissingException>(
            () => PgmqSettings.From(Section(new() { ["q:ConnectionString"] = "Host=x" })));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void From_MissingConnectionString_Throws()
        => Assert.ThrowsExactly<ConfigurationMissingException>(
            () => PgmqSettings.From(Section(new() { ["q:QueueName"] = "orders" })));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddPgmqQueueServices_RegistersProvidersAndKeys()
    {
        var services = new ServiceCollection();

        services.TryAddPgmqQueueServices();

        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IMessageSenderProvider) && Equals(d.ServiceKey, PgmqGlobals.MessageProviderKey)));
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IMessageReceiverProvider) && Equals(d.ServiceKey, PgmqGlobals.MessageProviderKey)));
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(OoBDev.AsyncApi.IAsyncApiContributor)));
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IPgmqClientFactory)));
    }
}
