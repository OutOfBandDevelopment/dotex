using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.Tcp.Tests;

/// <summary>Identifies the bench Rigol DG1062Z over VXI-11; Inconclusive when it is not reachable.</summary>
[TestCategory("Hardware")]
[TestCategory("Hardware")]
[TestCategory("Hardware")]
[TestClass]
public sealed class RealHardwareVxi11Tests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Dg1062z_AnswersIdentify()
    {
        // The unit takes a DHCP address (.87, then .127), so find it by LXI discovery instead of a fixed host.
        var found = await LxiDiscovery.ScanAsync(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), TestContext.CancellationToken);
        var device = found.FirstOrDefault(d => d.Identity.Contains("DG1062Z", StringComparison.OrdinalIgnoreCase));
        if (device is null)
        {
            Assert.Inconclusive("No DG1062Z answered LXI discovery.");
        }

        var host = device.Host;
        await using var transport = new Vxi11DeviceAdapter(new SystemTcpConnectionSource(), Options.Create(new Vxi11DeviceAdapterOptions { Host = host, AppendLineFeedAtEnd = false }));
        await transport.OpenAsync(TestContext.CancellationToken);
        await transport.WriteAsync("*IDN?\n"u8.ToArray(), TestContext.CancellationToken);

        var received = new StringBuilder();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        while (!received.ToString().Contains('\n'))
        {
            var result = await transport.Input.ReadAsync(timeout.Token);
            received.Append(Encoding.ASCII.GetString(result.Buffer));
            transport.Input.AdvanceTo(result.Buffer.End);
        }

        TestContext.WriteLine(received.ToString());
        StringAssert.Contains(received.ToString(), "DG1062Z");
    }
}
