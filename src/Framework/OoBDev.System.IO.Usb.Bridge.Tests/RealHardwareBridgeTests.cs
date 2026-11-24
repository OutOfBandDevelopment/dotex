using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO.UsbHids;
using OoBDev.System.IO.UsbTmc;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Usb.Bridge.Tests;

/// <summary>Real devices through the bridge over loopback; Inconclusive when the device is not attached.</summary>
[TestClass]
[TestCategory(TestCategories.Hardware)]
[TestCategory(TestCategories.Usbtmc)]
public sealed class RealHardwareBridgeTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class RemoteFactory(int port) : IUsbTmcDeviceFactory
    {
        public IUsbTmcDevice Create(UsbTmcDeviceAdapterOptions options) => new RemoteUsbTmcDevice("127.0.0.1", port);
    }

    [TestMethod]
    [DataRow(0x1AB1, 0x09C4, "DM3058E")]
    [DataRow(0x1AB1, 0x0588, "DG1022")]
    public async Task UsbTmc_IdentifyThroughTheBridge_Answers(int vendor, int product, string model)
    {
        // Stage
        var attached = new SystemUsbTmcDeviceDiscovery().GetDevices().Any(d => d.VendorId == vendor && d.ProductId == product);
        if (!attached)
            Assert.Inconclusive($"{model} ({vendor:X4}:{product:X4}) is not attached.");
        await using (var direct = new UsbTmcDeviceAdapter(new SystemUsbTmcDeviceFactory(), Options.Create(new UsbTmcDeviceAdapterOptions { VendorId = vendor, ProductId = product })))
        {
            try
            {
                await direct.OpenAsync(TestContext.CancellationToken);
                await direct.Output.WriteAsync(Encoding.ASCII.GetBytes("*IDN?\n"), TestContext.CancellationToken);
                using var t = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
                t.CancelAfter(TimeSpan.FromSeconds(8));
                var r = await direct.Input.ReadAsync(t.Token);
                TestContext.WriteLine("Direct control: " + Encoding.ASCII.GetString(r.Buffer).Trim());
                direct.Input.AdvanceTo(r.Buffer.End);
                await direct.CloseAsync(TestContext.CancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"{model} does not answer directly either, so the bridge cannot be judged: {ex.Message}");
            }
        }

        var options = new UsbBridgeOptions
        {
            UsbVendorId = vendor.ToString("X4"),
            UsbProductId = product.ToString("X4"),
            BindAddress = "127.0.0.1",
            TcpPort = 0,
        };
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        using var server = new UsbBridgeServer(options, new SystemUsbTmcDeviceFactory(), new SystemUsbHidDeviceFactory(), NullLogger.Instance);
        server.Start();
        var run = server.RunAsync(cts.Token);
        TestContext.WriteLine($"Bridge for {model} on port {server.BoundPort}");
        await using var adapter = new UsbTmcDeviceAdapter(new RemoteFactory(server.BoundPort), Options.Create(new UsbTmcDeviceAdapterOptions { VendorId = vendor, ProductId = product }));

        // Test
        await adapter.OpenAsync(TestContext.CancellationToken);
        await adapter.Output.WriteAsync(Encoding.ASCII.GetBytes("*IDN?\n"), TestContext.CancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(8));
        var result = await adapter.Input.ReadAsync(timeout.Token);
        var reply = Encoding.ASCII.GetString(result.Buffer).Trim();
        adapter.Input.AdvanceTo(result.Buffer.End);
        TestContext.WriteLine($"*IDN? -> {reply}");

        // Assert
        StringAssert.Contains(reply, "RIGOL", StringComparison.OrdinalIgnoreCase);
        await adapter.CloseAsync(TestContext.CancellationToken);
        await cts.CancelAsync();
        await run;
    }

    [TestMethod]
    [TestCategory(TestCategories.Hid)]
    public async Task Hid_BusylightColourWriteThroughTheBridge_IsAccepted()
    {
        // Stage
        if (!new SystemUsbHidDiscovery().GetDevices().Any(d => d.VendorId == 0x04D8 && d.ProductId == 0xF848))
            Assert.Inconclusive("Busylight (04D8:F848) is not attached.");
        var options = new UsbBridgeOptions { Kind = UsbBridgeKind.Hid, UsbVendorId = "04D8", UsbProductId = "F848", BindAddress = "127.0.0.1", TcpPort = 0 };
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        using var server = new UsbBridgeServer(options, new SystemUsbTmcDeviceFactory(), new SystemUsbHidDeviceFactory(), NullLogger.Instance);
        server.Start();
        var run = server.RunAsync(cts.Token);
        using var remote = new RemoteUsbHidDevice("127.0.0.1", server.BoundPort);

        // Test: a 64-byte report that sets the light off (all zero step table), then a short one is not needed.
        remote.Open();
        var report = new byte[65];
        remote.Write(report, 0, report.Length);
        TestContext.WriteLine("Wrote a 65-byte report through the bridge.");

        // Assert
        Assert.IsTrue(remote.IsOpen);
        remote.Close();
        await cts.CancelAsync();
        await run;
    }
}
