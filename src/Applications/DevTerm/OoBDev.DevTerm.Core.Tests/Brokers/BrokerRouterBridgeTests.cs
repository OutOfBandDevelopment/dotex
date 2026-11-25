using System;
using OoBDev.TestUtilities.Containers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Buffers;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Options;
using OoBDev.TestUtilities;
using OoBDev.System.IO.Brokers;
namespace OoBDev.DevTerm.Brokers;

/// <summary>A <see cref="MessageRouter"/> wired to AMQP and STOMP through <see cref="BrokerRouterBridge"/>: a fake connection, then the RabbitMQ container in containers/testing.</summary>
[TestClass]
[TestCategory(TestCategories.Brokers)]
public sealed class BrokerRouterBridgeTests
{
    public TestContext TestContext { get; set; } = null!;

    private static RoutingRuleSet Rules(string sensor, string command) => RoutingRuleSet.FromJson($$"""
        {
          "rules": [
            { "direction": "DeviceToBroker", "match": "^A=(?<a>[0-9.]+)", "topic": "{{sensor}}", "payload": "a=${a}" },
            { "direction": "BrokerToDevice", "topic": "{{command}}", "match": "^(?<cmd>[A-Za-z]+)$", "send": "${cmd}?" }
          ]
        }
        """);

    private sealed class FakeConnection : IBrokerConnection
    {
        public List<string> Subscribed { get; } = [];

        public List<(string Address, string Payload)> Published { get; } = [];

        public event Action<string, byte[]>? MessageReceived;

        public event Action<Exception?>? Disconnected
        {
            add { }
            remove { }
        }

        public void Receive(string address, string payload) => MessageReceived?.Invoke(address, Encoding.UTF8.GetBytes(payload));

        public Task ConnectAsync(BrokerDeviceAdapterOptions options, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task SubscribeAsync(string address, CancellationToken cancellationToken)
        {
            Subscribed.Add(address);
            return Task.CompletedTask;
        }

        public Task PublishAsync(string address, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
        {
            lock (Published)
            {
                Published.Add((address, Encoding.UTF8.GetString(payload.Span)));
            }

            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeFactory(FakeConnection connection) : IBrokerConnectionFactory
    {
        public IBrokerConnection Create() => connection;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Start_SubscribesToTheBrokerToDeviceAddresses_AndRoutesBothWays()
    {
        var fake = new FakeConnection();
        var rules = Rules("it.sensor", "it.cmd");
        await using var bridge = new BrokerRouterBridge(new FakeFactory(fake), Options.Create(new BrokerDeviceAdapterOptions { Host = "x" }), rules);
        var router = new MessageRouter(rules, bridge.Sink, terminator: "\n");
        var sent = new List<string>();
        router.Originated += (_, bytes) => sent.Add(Encoding.ASCII.GetString(bytes.Span));
        await bridge.StartAsync(router, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new[] { "it.cmd" }, fake.Subscribed);

        fake.Receive("it.cmd", "STATUS");
        CollectionAssert.AreEqual(new[] { "STATUS?\n" }, sent);

        router.Render(new ReadOnlySequence<byte>("A=21.5\r\n"u8.ToArray()));
        await WaitAsync(() => { lock (fake.Published) { return fake.Published.Count == 1; } });
        Assert.AreEqual(("it.sensor", "a=21.5"), fake.Published[0]);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Publish_BeforeStart_Throws()
    {
        await using var bridge = new BrokerRouterBridge(new FakeFactory(new FakeConnection()), Options.Create(new BrokerDeviceAdapterOptions { Host = "x" }), Rules("a", "b"));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => bridge.PublishAsync("t", "p"u8.ToArray(), TestContext.CancellationToken));
    }

    [IntegrationTestMethod("oobd-test-rabbitmq")]
    [TestCategory("Integration")]
    public Task RealRabbitMq_Amqp_RoutesBothWays() =>
        RoundTripAsync(new AmqpConnectionFactory(), "RABBITMQ_AMQP_PORT", 5673, "devterm-it.router.sensor", "devterm-it.router.cmd");

    [IntegrationTestMethod("oobd-test-rabbitmq")]
    [TestCategory("Integration")]
    public Task RealRabbitMq_Stomp_RoutesBothWays() =>
        RoundTripAsync(new StompConnectionFactory(), "RABBITMQ_STOMP_PORT", 21613, "/topic/devterm-it-router-sensor", "/topic/devterm-it-router-cmd");

    private async Task RoundTripAsync(IBrokerConnectionFactory factory, string portParameter, int defaultPort, string sensor, string command)
    {
        var host = TestContext.GetPropertyOrDefault("RABBITMQ_HOST", "127.0.0.1");
        var port = TestContext.GetPropertyOrDefault(portParameter, defaultPort);
        using (var probe = new TcpClient())
        {
            try
            {
                await probe.ConnectAsync(host, port, TestContext.CancellationToken);
            }
            catch (SocketException)
            {
                Assert.Inconclusive($"No broker on {host}:{port}; start containers/testing (integration-up).");
            }
        }

        var options = new BrokerDeviceAdapterOptions { Host = host, Port = port, Username = TestContext.GetPropertyOrDefault("RABBITMQ_USERNAME", "guest"), Password = TestContext.GetPropertyOrDefault("RABBITMQ_PASSWORD", "guest") };
        var rules = Rules(sensor, command);
        await using var bridge = new BrokerRouterBridge(factory, Options.Create(options), rules);
        var router = new MessageRouter(rules, bridge.Sink, terminator: "\n");
        var sent = new List<string>();
        router.Originated += (_, bytes) =>
        {
            lock (sent)
            {
                sent.Add(Encoding.ASCII.GetString(bytes.Span));
            }
        };
        await bridge.StartAsync(router, TestContext.CancellationToken);

        // A second client plays the rest of the system: it listens for the sensor address and issues the command.
        await using var peer = factory.Create();
        var heard = new List<string>();
        peer.MessageReceived += (address, payload) =>
        {
            lock (heard)
            {
                heard.Add(address + "=" + Encoding.UTF8.GetString(payload));
            }
        };
        await peer.ConnectAsync(options, TestContext.CancellationToken);
        await peer.SubscribeAsync(sensor, TestContext.CancellationToken);

        // STOMP SUBSCRIBE carries no receipt, so give both subscriptions a moment to register before publishing across connections.
        await Task.Delay(500, TestContext.CancellationToken);

        router.Render(new ReadOnlySequence<byte>("A=7.25\r\n"u8.ToArray()));
        await peer.PublishAsync(command, "PING"u8.ToArray(), TestContext.CancellationToken);

        await WaitAsync(() => { lock (heard) { return heard.Count == 1; } });
        await WaitAsync(() => { lock (sent) { return sent.Count == 1; } });
        Assert.AreEqual(sensor + "=a=7.25", heard[0]);
        Assert.AreEqual("PING?\n", sent[0]);
    }

    private async Task WaitAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(20, TestContext.CancellationToken);
        }

        Assert.IsTrue(condition(), "Timed out waiting.");
    }
}
