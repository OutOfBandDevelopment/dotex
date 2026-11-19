using System.Threading.Tasks;
using OoBDev.TestUtilities.Containers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Options;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Mqtt.Tests;

/// <summary>Round trip through the real Mosquitto broker in containers/testing (integration-up).</summary>
[TestCategory("Integration")]
[TestClass]
[TestCategory(TestCategories.Mqtt)]
public sealed class MosquittoIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    [IntegrationTestMethod("oobd-test-mosquitto")]
    public async Task PublishedLine_ComesBackAsTopicTabPayload()
    {
        var host = TestContext.GetPropertyOrDefault("MOSQUITTO_HOST", "127.0.0.1");
        var port = TestContext.GetPropertyOrDefault("MOSQUITTO_PORT", 1883);
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

        var options = new MqttDeviceAdapterOptions { Host = host, Port = port, SubscribeTopics = ["devterm-it/#"], PublishTopic = "devterm-it/out" };
        await using var transport = new MqttDeviceAdapter(new MqttNetConnectionFactory(), Options.Create(options));
        await transport.OpenAsync(TestContext.CancellationToken);

        await transport.WriteAsync("hello broker\n"u8.ToArray(), TestContext.CancellationToken);

        var result = await transport.Input.ReadAsync(TestContext.CancellationToken);
        Assert.AreEqual("devterm-it/out\thello broker\n", Encoding.UTF8.GetString(result.Buffer));
        Assert.AreEqual(ConnectionState.Open, transport.State);
    }
}
