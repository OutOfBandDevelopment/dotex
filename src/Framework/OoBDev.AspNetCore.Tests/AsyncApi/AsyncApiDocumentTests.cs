using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Amazon.Sqs;
using OoBDev.AsyncApi;
using OoBDev.AspNetCore.Mvc;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.Microsoft.Azure.ServiceBus;
using OoBDev.RabbitMQ;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.AsyncApi;

/// <summary>Channel used by the order messages.</summary>
public class OrdersChannel { }

/// <summary>Channel used by the audit messages.</summary>
public class AuditChannel { }

/// <summary>A queued order.</summary>
public record OrderPlaced(int OrderId, string Customer, DateTimeOffset PlacedOn);

/// <summary>A queued audit entry.</summary>
public record AuditEntry(string Action);

public class OrderPlacedHandler : IMessageQueueHandler<OrdersChannel, OrderPlaced>
{
    public Task HandleAsync(OrderPlaced message, IMessageContext context) => Task.CompletedTask;
    public Task HandleAsync(object message, IMessageContext context) => Task.CompletedTask;
}

public class AuditEntryHandler : IMessageQueueHandler<AuditChannel, AuditEntry>
{
    public Task HandleAsync(AuditEntry message, IMessageContext context) => Task.CompletedTask;
    public Task HandleAsync(object message, IMessageContext context) => Task.CompletedTask;
}

[TestClass]
[TestCategory(TestCategories.Simulate)]
public class AsyncApiDocumentTests
{
    private static WebApplication? _app;
    private static HttpClient? _client;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext _)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AsyncApi:Title"] = "Test messaging",

            ["MessageQueue:OrdersChannel:OrderPlaced:Provider"] = "sqs",
            ["MessageQueue:OrdersChannel:OrderPlaced:Config:QueueName"] = "orders.fifo",
            ["MessageQueue:OrdersChannel:OrderPlaced:Config:Region"] = "us-west-2",
            ["MessageQueue:OrdersChannel:OrderPlaced:Config:AccessKeyId"] = "SECRET-ACCESS-KEY",
            ["MessageQueue:OrdersChannel:OrderPlaced:Config:SecretAccessKey"] = "SECRET-ACCESS-VALUE",

            ["MessageQueue:AuditChannel:Provider"] = "rabbit-mq",
            ["MessageQueue:AuditChannel:Config:HostName"] = "rabbit.local",
            ["MessageQueue:AuditChannel:Config:Port"] = "5672",
            ["MessageQueue:AuditChannel:Config:QueueName"] = "audit",
            ["MessageQueue:AuditChannel:Config:UserName"] = "SECRET-USER",
            ["MessageQueue:AuditChannel:Config:Password"] = "SECRET-PASSWORD",

            ["MessageQueue:Notify:Provider"] = "servicebus",
            ["MessageQueue:Notify:Config:QueueName"] = "notify",
            ["MessageQueue:Notify:Config:ConnectionString"] = "Endpoint=sb://contoso.servicebus.windows.net/;SharedAccessKeyName=root;SharedAccessKey=SECRET-BUS-KEY",
        });

        var services = builder.Services;
        services.TryAddMessageQueueingServices();
        services.TryAddAmazonSqsServices();
        services.TryAddAzureServiceBusServices();
        services.TryAddRabbitMQServices();
        services.AddTransient<IMessageQueueHandler, OrderPlacedHandler>();
        services.AddTransient<IMessageQueueHandler, AuditEntryHandler>();
        services.TryAddAsyncApiServices();

        _app = builder.Build();
        _app.UseDeveloperExceptionPage();
        _app.MapAsyncApi();
        await _app.StartAsync();

        var address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        _client = new HttpClient { BaseAddress = new Uri(address) };
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _client?.Dispose();
        if (_app != null) await _app.DisposeAsync();
    }

    private static async Task<string> GetTextAsync(string path)
    {
        var response = await _client!.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(response.IsSuccessStatusCode, $"{(int)response.StatusCode}: {body}");
        return body;
    }

    private static async Task<JsonNode> GetDocumentAsync(string name) => JsonNode.Parse(await GetTextAsync($"/asyncapi/{name}.json"))!;

    [TestMethod]
    public async Task AllDocument_IsAsyncApi3WithTitle()
    {
        var document = await GetDocumentAsync("all");

        Assert.AreEqual("3.0.0", (string?)document["asyncapi"]);
        Assert.AreEqual("Test messaging", (string?)document["info"]!["title"]);
    }

    [TestMethod]
    public async Task AllDocument_DescribesSqsReceiveChannelWithPayload()
    {
        var document = await GetDocumentAsync("all");

        var channel = document["channels"]!["OrdersChannel.OrderPlaced"]!;
        Assert.AreEqual("orders.fifo", (string?)channel["address"]);
        Assert.AreEqual("#/servers/sqs", (string?)channel["servers"]![0]!["$ref"]);
        Assert.IsTrue((bool)channel["bindings"]!["sqs"]!["queue"]!["fifoQueue"]!);
        Assert.AreEqual("sqs.us-west-2.amazonaws.com", (string?)document["servers"]!["sqs"]!["host"]);
        Assert.AreEqual("receive", (string?)document["operations"]!["receiveOrdersChannelOrderPlaced"]!["action"]);

        var schema = document["components"]!["schemas"]!["OrderPlaced"]!;
        Assert.IsNotNull(schema["properties"]!["orderId"]);
        Assert.IsNotNull(schema["properties"]!["customer"]);
    }

    [TestMethod]
    public async Task AllDocument_DescribesRabbitMqAndServiceBus()
    {
        var document = await GetDocumentAsync("all");

        Assert.AreEqual("rabbit.local:5672", (string?)document["servers"]!["rabbitmq"]!["host"]);
        Assert.AreEqual("amqp", (string?)document["servers"]!["rabbitmq"]!["protocol"]);
        Assert.AreEqual("audit", (string?)document["channels"]!["AuditChannel.AuditEntry"]!["address"]);

        Assert.AreEqual("contoso.servicebus.windows.net", (string?)document["servers"]!["servicebus"]!["host"]);
        var notify = document["channels"]!["Notify.Notify"]!;
        Assert.AreEqual("notify", (string?)notify["address"]);
        Assert.AreEqual("send", (string?)document["operations"]!["sendNotifyNotify"]!["action"]);
    }

    [TestMethod]
    public async Task AllDocument_NeverContainsSecrets()
    {
        var text = await GetTextAsync("/asyncapi/all.json");

        foreach (var secret in new[] { "SECRET-ACCESS-KEY", "SECRET-ACCESS-VALUE", "SECRET-USER", "SECRET-PASSWORD", "SECRET-BUS-KEY" })
        {
            Assert.IsFalse(text.Contains(secret, StringComparison.Ordinal), secret);
        }
    }

    [TestMethod]
    public async Task AssemblyDocument_OnlyHasHandlerChannels_CaseInsensitive()
    {
        var name = typeof(OrderPlacedHandler).Assembly.GetName().Name!.ToLowerInvariant();

        var document = await GetDocumentAsync(name);

        Assert.IsNotNull(document["channels"]!["OrdersChannel.OrderPlaced"]);
        Assert.IsNull(document["channels"]!["Notify.Notify"]);
    }

    [TestMethod]
    public async Task UnknownDocument_Returns404()
    {
        var response = await _client!.GetAsync("/asyncapi/nope.json");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Viewer_ReferencesScriptAndDocument()
    {
        var html = await GetTextAsync("/asyncapi/all");

        StringAssert.Contains(html, "/asyncapi/all.json");
        StringAssert.Contains(html, AsyncApiEndpointRouteBuilderExtensions.DefaultViewerScriptUrl);
    }
}
