using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.PostgreSql.Pgmq.MessageQueueing;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.Text.Json;

namespace OoBDev.PostgreSql.Pgmq.Tests;

[TestClass]
public class PgmqAsyncApiContributorTests
{
    private static IConfigurationSection Section(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build().GetSection("q");

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_ConnectionString_UsesOnlyHostPortAndQueue()
    {
        // Stage
        var config = Section(new() { ["q:ConnectionString"] = "Host=db.local;Port=5434;Username=u;Password=hunter2", ["q:QueueName"] = "orders" });

        // Test
        var result = new PgmqAsyncApiContributor().Describe(config);

        // Assert
        Assert.AreEqual("db.local:5434", result!.Server.Host);
        Assert.AreEqual("orders", result.Address);
        Assert.IsFalse(JsonSerializer.Serialize(result).Contains("hunter2"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_UnparsableConnectionString_FallsBackToLocalhost()
    {
        // Stage
        var config = Section(new() { ["q:ConnectionString"] = "not=a=valid" });

        // Test and Assert
        Assert.AreEqual("localhost", new PgmqAsyncApiContributor().Describe(config)!.Server.Host);
    }
}
