using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Velleman.K8055.Tests;

[TestClass]
public class K8055HardwareTests
{
    private static async Task SendAsync(Stream stream, K8055Command command)
    {
        var bytes = new K8055MessageEncoder().Encode(ref command);
        await stream.WriteAsync(bytes).ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task ResetCounter1_OnTheBoard_CompletesWithoutFaultTest()
    {
        // Stage
        await using var stream = DevicePreflight.RequireUsbHid(new K8055Device());

        // Test: a fixed frame that touches no output, so the safest real action; there is no reply to wait for
        await SendAsync(stream, K8055Command.ResetCounter1).ConfigureAwait(false);
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task SetOutputs_OnTheBoard_ThenAllOffTest()
    {
        // Stage
        await using var stream = DevicePreflight.RequireUsbHid(new K8055Device());
        var outputs = new K8055Outputs(0, 0, 0).WithDigital(1, true).WithDigital(3, true) with { AnalogOut1 = 128, AnalogOut2 = 255 };

        try
        {
            // Test: watch outputs 1 and 3 and both analogue outputs
            await SendAsync(stream, K8055Command.SetOutputs(outputs)).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        }
        finally
        {
            // Verify: the board is left with every output off
            await SendAsync(stream, K8055Command.SetOutputs(default)).ConfigureAwait(false);
        }
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task Board_StreamsInputReports_DecodedTest()
    {
        // Stage
        await using var stream = DevicePreflight.RequireUsbHid(new K8055Device());
        var buffer = new byte[K8055Reading.Length];

        // Test: the board sends its inputs without being asked
        var read = await stream.ReadAsync(buffer).AsTask().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(K8055Reading.Length, read);
        var reading = new K8055MessageDecoder().Decode(new ReadOnlySequence<byte>(buffer));
        TestContext?.WriteLine(reading.ToString());
    }

    public TestContext? TestContext { get; set; }
}
