using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OoBDev.AsyncApi.Tests;

[TestClass]
public class AsyncApiDocumentBuilderTests
{
    public sealed class OrdersChannel;

    public enum Priority { Low, High }

    public sealed class OrderPlaced
    {
        public int Id { get; set; }
        public string Customer { get; set; } = string.Empty;
        public Priority Priority { get; set; }
    }

    private sealed class OrderHandler : IMessageQueueHandler<OrdersChannel, OrderPlaced>
    {
        public Task HandleAsync(object message, IMessageContext context) => Task.CompletedTask;
        public Task HandleAsync(OrderPlaced message, IMessageContext context) => Task.CompletedTask;
    }

    private sealed class StubContributor(string providerKey, AsyncApiContribution? result) : IAsyncApiContributor
    {
        public string ProviderKey => providerKey;
        public AsyncApiContribution? Describe(IConfigurationSection config) => result;
    }

    private static IConfiguration Config(params (string Key, string Value)[] values)
    {
        var data = new Dictionary<string, string?>();
        foreach (var (key, value) in values)
        {
            data[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }

    private static AsyncApiDocumentBuilder Builder(IConfiguration configuration, string? providerKey, string? path, IEnumerable<IAsyncApiContributor>? contributors = null, bool handlers = true, IEnumerable<AsyncApiSendMessage>? sends = null)
    {
        var resolver = new Mock<IMessagePropertyResolver>();
        resolver.Setup(r => r.ConfigurationSafe(typeof(OrdersChannel), typeof(OrderPlaced)))
            .Returns((path == null ? null : configuration.GetSection(path), "Orders", "OrderPlaced", path));
        resolver.Setup(r => r.ProviderSafe(typeof(OrdersChannel), typeof(OrderPlaced)))
            .Returns((providerKey, "Orders", "OrderPlaced", path));

        IMessageQueueHandler[] list = handlers ? [new OrderHandler()] : [];
        return new AsyncApiDocumentBuilder(list, resolver.Object, configuration, contributors ?? [], sends);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_RegisteredSendMessage_AddsSendOperationWithSchema()
    {
        // Stage
        var builder = Builder(Config(("MessageQueue:Orders:OrderPlaced:Provider", "in-process")), "in-process", "MessageQueue:Orders:OrderPlaced", handlers: false, sends: [new AsyncApiSendMessage(typeof(OrdersChannel), typeof(OrderPlaced))]);

        // Test
        var document = builder.Build("all");

        // Assert
        Assert.IsNotNull(document);
        Assert.AreEqual("send", document.Operations["sendOrdersOrderPlaced"].Action);
        Assert.IsTrue(document.Components.Schemas.ContainsKey("OrderPlaced"));
        Assert.HasCount(1, document.Channels);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void DocumentNames_WithHandlers_ListsAllThenTheAssembly()
    {
        // Stage
        var builder = Builder(Config(), null, null);

        // Test
        var names = builder.DocumentNames();

        // Assert
        CollectionAssert.AreEqual(new[] { "all", typeof(AsyncApiDocumentBuilderTests).Assembly.GetName().Name }, (System.Collections.ICollection)names);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_UnknownName_ReturnsNull()
    {
        // Test
        var document = Builder(Config(), null, null).Build("missing");

        // Assert
        Assert.IsNull(document);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_Handler_AddsReceiveOperationOnTheChannel()
    {
        // Test
        var document = Builder(Config(), null, null).Build("all")!;

        // Assert
        Assert.AreEqual("3.0.0", document.AsyncApi);
        Assert.IsTrue(document.Channels.ContainsKey("Orders.OrderPlaced"));
        var operation = document.Operations["receiveOrdersOrderPlaced"];
        Assert.AreEqual("receive", operation.Action);
        Assert.AreEqual("#/channels/Orders.OrderPlaced", operation.Channel.Ref);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_Handler_ExportsThePayloadSchemaWithEnumsAndProperties()
    {
        // Test
        var document = Builder(Config(), null, null).Build("all")!;

        // Assert
        var schema = document.Components.Schemas["OrderPlaced"];
        var properties = schema["properties"]!.AsObject();
        Assert.IsTrue(properties.ContainsKey("id"));
        Assert.IsTrue(properties.ContainsKey("customer"));
        var priority = properties["priority"]!.AsObject();
        var values = priority["enum"]!.AsArray();
        CollectionAssert.AreEquivalent(new[] { "Low", "High" }, new[] { (string?)values[0], (string?)values[1] });
        Assert.AreEqual("#/components/schemas/OrderPlaced", (string?)document.Channels["Orders.OrderPlaced"].Messages["OrderPlaced"].Payload!["$ref"]);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_ConfigurationTitleAndVersion_AreUsed()
    {
        // Test
        var document = Builder(Config(("AsyncApi:Title", "Shop"), ("AsyncApi:Version", "2.1.0")), null, null).Build("all")!;

        // Assert
        Assert.AreEqual("Shop", document.Info.Title);
        Assert.AreEqual("2.1.0", document.Info.Version);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_ContributorMatchesProviderKey_AddsServerAddressAndBindings()
    {
        // Stage
        var contribution = new AsyncApiContribution("stub", new AsyncApiServer { Host = "h:1", Protocol = "amqp" }, "orders-queue", new JsonObject { ["x"] = 1 });
        var configuration = Config(("MessageQueue:Orders:OrderPlaced:Provider", "stub"));
        var builder = Builder(configuration, "stub", "MessageQueue:Orders:OrderPlaced", [new StubContributor("STUB", contribution)]);

        // Test
        var document = builder.Build("all")!;

        // Assert
        Assert.AreEqual("h:1", document.Servers["stub"].Host);
        var channel = document.Channels["Orders.OrderPlaced"];
        Assert.AreEqual("orders-queue", channel.Address);
        Assert.AreEqual("#/servers/stub", channel.Servers![0].Ref);
        Assert.IsNotNull(channel.Bindings);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_NoContributorForProvider_DescribesTheProviderOnly()
    {
        // Test
        var document = Builder(Config(("MessageQueue:Orders:OrderPlaced:Provider", "other")), "other", "MessageQueue:Orders:OrderPlaced").Build("all")!;

        // Assert
        Assert.AreEqual("Provider: other", document.Channels["Orders.OrderPlaced"].Description);
        Assert.AreEqual(0, document.Servers.Count);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_ConfiguredQueueWithoutHandler_AddsSendOperationInTheAllDocumentOnly()
    {
        // Stage
        var configuration = Config(("MessageQueue:Audit:Provider", "stub"), ("MessageQueue:Audit:Config:QueueName", "audit"));
        var contribution = new AsyncApiContribution("stub", new AsyncApiServer { Host = "h" }, "audit");
        var builder = Builder(configuration, null, null, [new StubContributor("stub", contribution)], handlers: false);

        // Test
        var all = builder.Build("all")!;

        // Assert
        Assert.AreEqual("send", all.Operations["sendAuditAudit"].Action);
        Assert.AreEqual("audit", all.Channels["Audit.Audit"].Address);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_HandlerConfigSection_IsNotRepeatedAsASender()
    {
        // Stage
        var configuration = Config(("MessageQueue:Orders:OrderPlaced:Provider", "stub"));

        // Test
        var document = Builder(configuration, "stub", "MessageQueue:Orders:OrderPlaced").Build("all")!;

        // Assert
        Assert.AreEqual(1, document.Operations.Count);
    }
}
