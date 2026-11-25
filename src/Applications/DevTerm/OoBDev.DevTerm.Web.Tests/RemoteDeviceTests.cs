using System;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Remote;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Web.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
[TestCategory(TestCategories.Web)]
public class RemoteDeviceTests
{
    public required TestContext TestContext { get; set; }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private async Task<(WebHost.Built Built, string Base, TempDirectoryHandle Folder)> StartAsync()
    {
        var folder = new TempDirectoryHandle();
        var projectPath = Path.Combine(folder.DirectoryPath, "bench.json");
        var project = new ProjectFile { Name = "bench" };
        project.Connections.Add(new ProjectConnection("sim", """{"Transport":"loopback","Presenter":["ascii"]}"""));
        project.Save(projectPath);

        var port = FreePort();
        var built = WebHost.Build(
            new CliOptions { Transport = "loopback", Presenter = ["ascii"], Tui = false, Cli = true, Project = projectPath },
            new WebOptions { Urls = $"http://127.0.0.1:{port}", Token = "secret", ReadOnlyToken = "watch" },
            []);
        await built.Hub.StartAsync();
        await built.App.StartAsync();
        return (built, $"http://127.0.0.1:{port}", folder);
    }

    private static RemoteDeviceAdapter Adapter(string baseUrl, string connection, string key) =>
        new(Options.Create(new RemoteDeviceOptions { BaseUrl = baseUrl, Connection = connection, Key = key }));

    private static async Task<string> ReadAsync(PipeReader reader, string until, CancellationToken cancellationToken)
    {
        var text = new StringBuilder();
        while (!text.ToString().Contains(until, StringComparison.Ordinal))
        {
            var result = await reader.ReadAsync(cancellationToken);
            foreach (var segment in result.Buffer)
            {
                text.Append(Encoding.ASCII.GetString(segment.Span));
            }

            reader.AdvanceTo(result.Buffer.End);
            if (result.IsCompleted)
            {
                break;
            }
        }

        return text.ToString();
    }

    [TestMethod]
    public async Task Open_WithTheSharedKey_RelaysBytesBothWaysToTheHostsDevice()
    {
        // Stage
        var (built, url, folder) = await StartAsync();
        using (folder)
        await using (built.Hub)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(20));
            await using var adapter = Adapter(url, "sim", "secret");

            // Test
            await adapter.OpenAsync(timeout.Token);
            await adapter.Output.WriteAsync(Encoding.ASCII.GetBytes("hello\r\n"), timeout.Token);
            var reply = await ReadAsync(adapter.Input, "From Loopback test", timeout.Token);
            TestContext.WriteLine($"state={adapter.State} path={adapter.Path} reply={reply.Trim()}");

            // Assert
            Assert.AreEqual(ConnectionState.Open, adapter.State);
            StringAssert.Contains(reply, "From Loopback test");
            await adapter.CloseAsync(timeout.Token);
            await built.App.StopAsync(timeout.Token);
        }
    }

    [TestMethod]
    public async Task Open_WithAWrongKey_ThrowsAndFaults()
    {
        // Stage
        var (built, url, folder) = await StartAsync();
        using (folder)
        await using (built.Hub)
        {
            await using var adapter = Adapter(url, "sim", "nope");

            // Test
            var error = await Assert.ThrowsExactlyAsync<IOException>(() => adapter.OpenAsync(TestContext.CancellationToken));
            TestContext.WriteLine(error.Message);

            // Assert
            Assert.AreEqual(ConnectionState.Faulted, adapter.State);
            await built.App.StopAsync(TestContext.CancellationToken);
        }
    }

    [TestMethod]
    public async Task Open_WithTheReadOnlyKey_IsRefusedTheDevice()
    {
        // Stage
        var (built, url, folder) = await StartAsync();
        using (folder)
        await using (built.Hub)
        {
            await using var adapter = Adapter(url, "sim", "watch");

            // Test
            var error = await Assert.ThrowsExactlyAsync<global::System.Net.WebSockets.WebSocketException>(() => adapter.OpenAsync(TestContext.CancellationToken));
            TestContext.WriteLine(error.Message);

            // Assert
            Assert.AreEqual(ConnectionState.Faulted, adapter.State);
            await built.App.StopAsync(TestContext.CancellationToken);
        }
    }

    [TestMethod]
    public async Task Open_ForAnUnknownConnection_IsRefused()
    {
        // Stage
        var (built, url, folder) = await StartAsync();
        using (folder)
        await using (built.Hub)
        {
            await using var adapter = Adapter(url, "missing", "secret");

            // Test and Assert
            await Assert.ThrowsExactlyAsync<global::System.Net.WebSockets.WebSocketException>(() => adapter.OpenAsync(TestContext.CancellationToken));
            Assert.AreEqual(ConnectionState.Faulted, adapter.State);
            await built.App.StopAsync(TestContext.CancellationToken);
        }
    }
}
