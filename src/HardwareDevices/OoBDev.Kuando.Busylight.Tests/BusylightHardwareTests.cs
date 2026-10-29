using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Kuando.Busylight.Tests;

[TestClass]
public class BusylightHardwareTests
{
    private static async Task SendAsync(Stream stream, byte[] report)
    {
        await stream.WriteAsync(report).ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task SingleCommand_RedGreenBlueThenOff_ChangesTheLightTest()
    {
        // Stage
        await using var stream = DevicePreflight.RequireUsbHid(new BusylightDevice());
        var encoder = new BusylightMessageEncoder();

        try
        {
            // Test
            foreach (var color in new[] { BusylightColors.Red, BusylightColors.Green, BusylightColors.Blue })
            {
                var command = BusylightCommand.Solid(color);
                await SendAsync(stream, encoder.Encode(ref command).ToArray()).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
        finally
        {
            // Verify: the light is left off
            var off = BusylightCommand.Solid(BusylightColors.Off);
            await SendAsync(stream, encoder.Encode(ref off).ToArray()).ConfigureAwait(false);
        }
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task Poll_OnTheDevice_AnswersWithAnIdentificationTest()
    {
        // Stage
        await using var stream = DevicePreflight.RequireUsbHid(new BusylightDevice());
        stream.ReadTimeout = 3000;

        // Test
        await SendAsync(stream, BusylightSequence.Poll()).ConfigureAwait(false);
        var buffer = new byte[256];
        var read = await stream.ReadAsync(buffer).ConfigureAwait(false);

        // Assert
        var identification = new BusylightMessageDecoder().Decode(new global::System.Buffers.ReadOnlySequence<byte>(buffer, 0, read));
        TestContext?.WriteLine(identification.Text);
        StringAssert.Contains(identification.Text, "PLENOM");
    }

    public TestContext? TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task Sequence_SixColorSteps_ProgramFrameIsSentTest()
    {
        // Stage: the big-endian checksum is the untested guess about why the earlier program frame did nothing; watch the light
        await using var stream = DevicePreflight.RequireUsbHid(new BusylightDevice());
        static BusylightCommand Step(byte next, BusylightColor color) => new(next, 0x01, color, 0x05, 0x00, BusylightAudio.Muted);
        var frame = BusylightSequence.Encode(
        [
            Step(1, BusylightColors.Red),
            Step(2, BusylightColors.Yellow),
            Step(3, BusylightColors.Green),
            Step(4, new BusylightColor(0, 255, 255)),
            Step(5, BusylightColors.Blue),
            Step(0, new BusylightColor(255, 0, 255)),
        ]);

        try
        {
            // Test
            await SendAsync(stream, frame).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(4)).ConfigureAwait(false);
        }
        finally
        {
            var encoder = new BusylightMessageEncoder();
            var off = BusylightCommand.Solid(BusylightColors.Off);
            await SendAsync(stream, encoder.Encode(ref off).ToArray()).ConfigureAwait(false);
        }
    }
}
