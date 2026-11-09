using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Microsoft.Extensions.Options;
using OoBDev.TestUtilities;
namespace OoBDev.System.IO.Brokers.Tests;

[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Brokers)]
public sealed class BrokerDeviceAdapterTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Open_SubscribesEachAddress_InboundBecomesAddressTabPayload_AndWriteUsesThePublishAddress()
    {
        var fake = new FakeConnection();
        await using var transport = new BrokerDeviceAdapter(new FakeFactory(fake), Options.Create(new BrokerDeviceAdapterOptions { Host = "b", SubscribeTopics = ["a.#", " "], PublishTopic = "cmd" }));
        await transport.OpenAsync(TestContext.CancellationToken);

        fake.Receive("a.temp", "21.5"u8.ToArray());
        await transport.WriteAsync("on\n"u8.ToArray(), TestContext.CancellationToken);
        await transport.WriteAsync("other\tx\n"u8.ToArray(), TestContext.CancellationToken);

        var result = await transport.Input.ReadAsync(TestContext.CancellationToken);
        Assert.AreEqual("a.temp\t21.5\n", Encoding.UTF8.GetString(result.Buffer));
        CollectionAssert.AreEqual(new[] { "a.#" }, fake.Subscribed);
        CollectionAssert.AreEqual(new[] { "cmd=on", "other=x" }, fake.Published);
        Assert.AreEqual(ConnectionState.Open, transport.State);
    }

    [TestMethod]
    public async Task Write_WithNoPublishAddress_Throws()
    {
        await using var transport = new BrokerDeviceAdapter(new FakeFactory(new FakeConnection()), Options.Create(new BrokerDeviceAdapterOptions { Host = "b" }));
        await transport.OpenAsync(TestContext.CancellationToken);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => transport.WriteAsync("x\n"u8.ToArray(), TestContext.CancellationToken));
    }

    private sealed class FakeFactory(FakeConnection connection) : IBrokerConnectionFactory
    {
        public IBrokerConnection Create() => connection;
    }

    private sealed class FakeConnection : IBrokerConnection
    {
        public List<string> Subscribed { get; } = [];

        public List<string> Published { get; } = [];

        public event Action<string, byte[]>? MessageReceived;

        public event Action<Exception?>? Disconnected
        {
            add { }
            remove { }
        }

        public void Receive(string address, byte[] payload) => MessageReceived?.Invoke(address, payload);

        public Task ConnectAsync(BrokerDeviceAdapterOptions options, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task SubscribeAsync(string address, CancellationToken cancellationToken)
        {
            Subscribed.Add(address);
            return Task.CompletedTask;
        }

        public Task PublishAsync(string address, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
        {
            Published.Add(address + "=" + Encoding.UTF8.GetString(payload.Span));
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
