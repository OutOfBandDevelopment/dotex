using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Microsoft.Azure.StorageAccount.MessageQueueing;
using System.Collections.Generic;
using System.Text.Json;

namespace OoBDev.Microsoft.Azure.StorageAccount.Tests.MessageQueueing;

[TestClass]
public class AzureStorageQueueAsyncApiContributorTests
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
    public void Describe_AccountConnectionString_BuildsTheQueueHostWithoutTheKey()
    {
        // Test
        var result = new AzureStorageQueueAsyncApiContributor().Describe(Section(
            ("ConnectionString", "DefaultEndpointsProtocol=https;AccountName=contoso;AccountKey=TOPSECRET;EndpointSuffix=core.windows.net"),
            ("QueueName", "orders")));

        // Assert
        Assert.AreEqual("azure-storage-queue", result!.ServerName);
        Assert.AreEqual("contoso.queue.core.windows.net", result.Server.Host);
        Assert.AreEqual("https", result.Server.Protocol);
        Assert.AreEqual("orders", result.Address);
        Assert.IsFalse(JsonSerializer.Serialize(result).Contains("TOPSECRET"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_EmulatorQueueEndpoint_UsesItsAuthorityAndScheme()
    {
        // Test
        var result = new AzureStorageQueueAsyncApiContributor().Describe(Section(
            ("ConnectionString", "AccountName=devstoreaccount1;AccountKey=abc;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;"),
            ("QueueName", "jobs")));

        // Assert
        Assert.AreEqual("127.0.0.1:10001", result!.Server.Host);
        Assert.AreEqual("http", result.Server.Protocol);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_NoConnectionString_FallsBackToTheGenericHost()
    {
        // Test
        var result = new AzureStorageQueueAsyncApiContributor().Describe(Section(("QueueName", "jobs")));

        // Assert
        Assert.AreEqual("queue.core.windows.net", result!.Server.Host);
    }
}
