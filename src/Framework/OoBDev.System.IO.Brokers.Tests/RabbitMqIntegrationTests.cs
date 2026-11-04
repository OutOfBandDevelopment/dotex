using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Text;
using OoBDev.System.IO.Pipelines;
using Microsoft.Extensions.Options;
using OoBDev.TestUtilities;
namespace OoBDev.System.IO.Brokers.Tests;

/// <summary>Round trips through the real RabbitMQ in containers/testing (integration-up): AMQP 0-9-1, STOMP, and the TLS listeners (ports and credentials from the RABBITMQ_* test parameters).</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class RabbitMqIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    private string Host => TestContext.GetPropertyOrDefault("RABBITMQ_HOST", "127.0.0.1");

    private int AmqpPort => TestContext.GetPropertyOrDefault("RABBITMQ_AMQP_PORT", 5673);

    private int StompPort => TestContext.GetPropertyOrDefault("RABBITMQ_STOMP_PORT", 21613);

    private int AmqpsPort => TestContext.GetPropertyOrDefault("RABBITMQ_AMQPS_PORT", 5671);

    private int StompTlsPort => TestContext.GetPropertyOrDefault("RABBITMQ_STOMP_TLS_PORT", 21614);

    private string Username => TestContext.GetPropertyOrDefault("RABBITMQ_USERNAME", "guest");

    private string Password => TestContext.GetPropertyOrDefault("RABBITMQ_PASSWORD", "guest");

    [TestMethod]
    public async Task Amqp_PublishedLine_ComesBackAsRoutingKeyTabPayload()
    {
        await RequireAsync(AmqpPort);
        var options = new BrokerDeviceAdapterOptions { Host = Host, Port = AmqpPort, Username = Username, Password = Password, SubscribeTopics = ["devterm-it.#"], PublishTopic = "devterm-it.out" };
        await using var transport = new BrokerDeviceAdapter(new AmqpConnectionFactory(), Options.Create(options));
        await transport.OpenAsync(TestContext.CancellationToken);

        await transport.WriteAsync("hello amqp\n"u8.ToArray(), TestContext.CancellationToken);

        Assert.AreEqual("devterm-it.out\thello amqp\n", await ReadAsync(transport));
        Assert.AreEqual(ConnectionState.Open, transport.State);
    }

    [TestMethod]
    public async Task Stomp_PublishedLine_ComesBackAsDestinationTabPayload()
    {
        await RequireAsync(StompPort);
        var options = new BrokerDeviceAdapterOptions { Host = Host, Port = StompPort, Username = Username, Password = Password, SubscribeTopics = ["/topic/devterm-it"], PublishTopic = "/topic/devterm-it" };
        await using var transport = new BrokerDeviceAdapter(new StompConnectionFactory(), Options.Create(options));
        await transport.OpenAsync(TestContext.CancellationToken);

        await transport.WriteAsync("hello stomp\n"u8.ToArray(), TestContext.CancellationToken);

        Assert.AreEqual("/topic/devterm-it\thello stomp\n", await ReadAsync(transport));
    }

    [TestMethod]
    public async Task Stomp_WrongPassword_FailsTheOpen()
    {
        await RequireAsync(StompPort);
        var options = new BrokerDeviceAdapterOptions { Host = Host, Port = StompPort, Username = Username, Password = "wrong" };
        await using var transport = new BrokerDeviceAdapter(new StompConnectionFactory(), Options.Create(options));

        await Assert.ThrowsAsync<IOException>(() => transport.OpenAsync(TestContext.CancellationToken));

        Assert.AreEqual(ConnectionState.Faulted, transport.State);
    }

    [TestMethod]
    public async Task Amqps_WithTheTestCa_RoundTrips()
    {
        var ca = await RequireTlsAsync(AmqpsPort);
        var options = new BrokerDeviceAdapterOptions { Host = "localhost", Port = AmqpsPort, Username = Username, Password = Password, UseTls = true, TlsCaCertificatePath = ca, SubscribeTopics = ["devterm-tls.#"], PublishTopic = "devterm-tls.out" };
        await using var transport = new BrokerDeviceAdapter(new AmqpConnectionFactory(), Options.Create(options));
        await transport.OpenAsync(TestContext.CancellationToken);

        await transport.WriteAsync("hello amqps\n"u8.ToArray(), TestContext.CancellationToken);

        Assert.AreEqual("devterm-tls.out\thello amqps\n", await ReadAsync(transport));
    }

    [TestMethod]
    public async Task StompOverTls_WithTheTestCa_RoundTrips()
    {
        var ca = await RequireTlsAsync(StompTlsPort);
        var options = new BrokerDeviceAdapterOptions { Host = "localhost", Port = StompTlsPort, Username = Username, Password = Password, UseTls = true, TlsCaCertificatePath = ca, SubscribeTopics = ["/topic/devterm-tls"], PublishTopic = "/topic/devterm-tls" };
        await using var transport = new BrokerDeviceAdapter(new StompConnectionFactory(), Options.Create(options));
        await transport.OpenAsync(TestContext.CancellationToken);

        await transport.WriteAsync("hello stomps\n"u8.ToArray(), TestContext.CancellationToken);

        Assert.AreEqual("/topic/devterm-tls\thello stomps\n", await ReadAsync(transport));
    }

    [TestMethod]
    public async Task StompOverTls_WithoutTrustingTheCa_RefusesTheServerCertificate()
    {
        await RequireTlsAsync(StompTlsPort);
        var options = new BrokerDeviceAdapterOptions { Host = "localhost", Port = StompTlsPort, Username = Username, Password = Password, UseTls = true };
        await using var transport = new BrokerDeviceAdapter(new StompConnectionFactory(), Options.Create(options));

        await Assert.ThrowsAsync<global::System.Security.Authentication.AuthenticationException>(() => transport.OpenAsync(TestContext.CancellationToken));
    }

    private async Task<string> RequireTlsAsync(int port)
    {
        await RequireAsync(port);
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var ca = Path.Combine(dir.FullName, "containers", "testing", "devterm", "certs", "ca.pem");
            if (File.Exists(ca))
            {
                return ca;
            }
        }

        Assert.Inconclusive("No containers/testing/devterm/certs/ca.pem; run integration-up (it creates the certificates) and recreate the rabbitmq container.");
        return string.Empty;
    }

    private async Task<string> ReadAsync(BrokerDeviceAdapter transport)
    {
        var result = await transport.Input.ReadAsync(TestContext.CancellationToken);
        return Encoding.UTF8.GetString(result.Buffer);
    }

    private async Task RequireAsync(int port)
    {
        using var probe = new TcpClient();
        try
        {
            await probe.ConnectAsync(Host, port, TestContext.CancellationToken);
        }
        catch (SocketException)
        {
            Assert.Inconclusive($"No broker on {Host}:{port}; start containers/testing (integration-up).");
        }
    }
}
