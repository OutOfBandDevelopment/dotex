using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Nmea.Tests;

[TestClass]
public class NmeaHardwareTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task EarthmateBt20_ReceivesAtLeastOneSentenceTest()
    {
        var device = new NmeaEarthmateBt20Device();
        using var stream = DevicePreflight.RequireUsbHid(device);

        var pump = new SegmentPump<NmeaSentence>(device);
        var buffer = new byte[256];
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationTokenSource.Token);
        cts.CancelAfter(TimeSpan.FromSeconds(35));

        try
        {
            while (pump.Messages.Count == 0)
            {
                var read = await stream.ReadAsync(buffer, cts.Token).ConfigureAwait(false);
                if (read > 0) await pump.PushAsync(buffer[..read]).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }

        Assert.IsTrue(pump.Messages.Count > 0, "The BT-20 sent no NMEA sentence within 35 seconds.");
    }
}
