using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.Tcp.Tests;

/// <summary>Round trip through the real ser2net RFC 2217 server in containers/testing (its virtual port echoes).</summary>
[TestCategory("Integration")]
[TestClass]
[TestCategory(TestCategories.Rfc2217)]
public sealed class Ser2NetIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    [IntegrationTestMethod("oobd-test-ser2net")]
    public async Task SentBytes_ComeBackFromTheServersEchoPort()
    {
        var host = TestContext.GetPropertyOrDefault("SER2NET_HOST", "127.0.0.1");
        var port = TestContext.GetPropertyOrDefault("SER2NET_PORT", 2217);
        using (var probe = new TcpClient())
        {
            try
            {
                await probe.ConnectAsync(host, port, TestContext.CancellationToken);
            }
            catch (SocketException)
            {
                Assert.Inconclusive($"No ser2net on {host}:{port}; start containers/testing (integration-up).");
            }
        }

        var options = new Rfc2217ClientOptions { Host = host, Port = port, BaudRate = 9600 };
        await using var transport = new Rfc2217ClientDeviceAdapter(new SystemTcpConnectionSource(), Options.Create(options));
        await transport.OpenAsync(TestContext.CancellationToken);

        await transport.WriteAsync("ping\n"u8.ToArray(), TestContext.CancellationToken);

        var received = new StringBuilder();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        timeout.CancelAfter(5000);
        while (!received.ToString().Contains("ping", StringComparison.Ordinal))
        {
            var result = await transport.Input.ReadAsync(timeout.Token);
            received.Append(Encoding.UTF8.GetString(result.Buffer));
            transport.Input.AdvanceTo(result.Buffer.End);
        }

        Assert.AreEqual(ConnectionState.Open, transport.State);
        Assert.IsTrue(transport.ComPortControlNegotiated);
    }
}
