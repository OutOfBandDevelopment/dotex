using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.UsbTmc.Tests;

/// <summary>Back-to-back queries and reopen against any attached USBTMC device; Inconclusive when none is found.</summary>
[TestCategory("Hardware")]
[TestClass]
[TestCategory(TestCategories.Usbtmc)]
public sealed class RealHardwareUsbTmcTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Query_BackToBackIdentify_AllAnswered()
    {
        // Stage
        await using var adapter = await OpenFirstDeviceAsync();

        // Test
        var replies = new List<string>();
        for (var i = 0; i < 4; i++)
        {
            replies.Add(await QueryAsync(adapter, "*IDN?\n"));
        }

        // Assert
        Assert.IsTrue(replies.All(r => !string.IsNullOrWhiteSpace(r)), string.Join("|", replies));
        Assert.AreEqual(1, replies.Distinct().Count());
    }

    [TestMethod]
    public async Task OpenAsync_CloseAndReopen_StillAnswers()
    {
        // Stage
        await using var adapter = await OpenFirstDeviceAsync();
        var first = await QueryAsync(adapter, "*IDN?\n");

        // Test
        await adapter.CloseAsync(TestContext.CancellationToken);
        await adapter.OpenAsync(TestContext.CancellationToken);
        var second = await QueryAsync(adapter, "*IDN?\n");

        // Assert
        Assert.AreEqual(first, second);
    }

    private async Task<UsbTmcDeviceAdapter> OpenFirstDeviceAsync()
    {
        IReadOnlyList<UsbTmcDeviceDescriptor> devices;
        try
        {
            devices = new SystemUsbTmcDeviceDiscovery().GetDevices();
        }
        catch (Exception ex)
        {
            Assert.Inconclusive($"USB enumeration is unavailable: {ex.Message}");
            throw;
        }

        if (devices.Count == 0)
        {
            Assert.Inconclusive("No USBTMC device is attached.");
        }

        var device = devices[0];
        TestContext.WriteLine($"Using {device.Manufacturer} {device.Product} {device.SerialNumber}");
        var adapter = new UsbTmcDeviceAdapter(
            new SystemUsbTmcDeviceFactory(),
            Options.Create(new UsbTmcDeviceAdapterOptions
            {
                VendorId = device.VendorId,
                ProductId = device.ProductId,
                SerialNumber = device.SerialNumber,
            }));
        await adapter.OpenAsync(TestContext.CancellationToken);
        return adapter;
    }

    private async Task<string> QueryAsync(UsbTmcDeviceAdapter adapter, string command)
    {
        await adapter.WriteAsync(Encoding.ASCII.GetBytes(command), TestContext.CancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        var result = await adapter.Input.ReadAsync(timeout.Token);
        var text = Encoding.ASCII.GetString(result.Buffer);
        adapter.Input.AdvanceTo(result.Buffer.End);
        return text;
    }
}
