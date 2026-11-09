using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class QueueMonitorTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Recent_MoreThanCapacity_KeepsNewestFirstAndDropsOldest()
    {
        var log = new ReceivedMessageLog(new FakeTimeProvider());
        for (var i = 0; i < ReceivedMessageLog.Capacity + 3; i++)
        {
            log.Add($"m{i}", $"c{i}");
        }

        var recent = log.Recent();

        Assert.AreEqual(ReceivedMessageLog.Capacity, recent.Count);
        Assert.AreEqual($"m{ReceivedMessageLog.Capacity + 2}", recent[0].Message);
        Assert.AreEqual("m3", recent.Last().Message);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Add_NonStringMessage_StoresJson()
    {
        var log = new ReceivedMessageLog(new FakeTimeProvider());
        log.Add(new ExampleMessageModel { Input = "hi" }, null);
        StringAssert.Contains(log.Recent()[0].Message, "\"Input\":\"hi\"");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void List_ConfiguredChannels_ShowsQueueHostAndReceiverFlagWithoutSecrets()
    {
        var options = new QueueChannelsOptions
        {
            ["Zeta"] = new() { Provider = "azure-storage-queue", Config = { ["QueueName"] = "z", ["ConnectionString"] = "secret" } },
            ["Alpha"] = new() { Provider = "rabbit-mq", Config = { ["QueueName"] = "a", ["HostName"] = "localhost", ["DisableReceiver"] = "true" } },
        };

        var list = new QueueChannelCatalog(Options.Create(options)).List();

        CollectionAssert.AreEqual(new[] { "Alpha", "Zeta" }, list.Select(c => c.Channel).ToArray());
        Assert.IsFalse(list[0].ReceiverEnabled);
        Assert.AreEqual("localhost", list[0].HostName);
        Assert.IsTrue(list[1].ReceiverEnabled);
        Assert.IsFalse(global::System.Text.Json.JsonSerializer.Serialize(list).Contains("secret"));
    }
}
