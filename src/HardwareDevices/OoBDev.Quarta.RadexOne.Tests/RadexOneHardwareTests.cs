using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace OoBDev.Quarta.RadexOne.Tests;

[TestClass]
public class RadexOneHardwareTests
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

    public TestContext TestContext { get; set; } = null!;

    private async Task<RadexOneReply> AskAsync(RadexOneRequest request)
    {
        var portName = DevicePreflight.RequireSerialPort(TestContext, "RADEX_ONE_SERIAL_PORT");
        using var port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One) { Handshake = Handshake.None, ReadTimeout = 200 };
        port.Open();

        var pump = new SegmentPump<RadexOneReply>(new RadexOneDevice());
        var bytes = new RadexOneMessageEncoder().Encode(ref request).ToArray();
        port.Write(bytes, 0, bytes.Length);

        var buffer = new byte[256];
        var deadline = DateTime.UtcNow + _timeout;
        while (pump.Messages.Count == 0 && DateTime.UtcNow < deadline)
        {
            if (port.BytesToRead > 0)
                await pump.PushAsync(buffer.AsMemory(0, port.Read(buffer, 0, buffer.Length))).ConfigureAwait(false);
            else
                await Task.Delay(50).ConfigureAwait(false);
        }

        Assert.AreNotEqual(0, pump.Messages.Count, "No reply from the device within the timeout.");
        return pump.Messages[0];
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task ReadData_OnTheDevice_AnswersWithAReadingTest()
    {
        var reply = await AskAsync(new RadexOneRequest(RadexOneRequestKind.ReadData)).ConfigureAwait(false);

        Assert.IsTrue(reply.TryGetReadData(out var data), "the reply is not a Read Data reply with a valid inner checksum");
        TestContext.WriteLine($"Ambient={data.Ambient} Accumulated={data.Accumulated} Cpm={data.Cpm}");
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task ReadSerialVersion_OnTheDevice_AnswersWithTextTest()
    {
        var reply = await AskAsync(new RadexOneRequest(RadexOneRequestKind.ReadSerialVersion)).ConfigureAwait(false);

        Assert.IsFalse(string.IsNullOrEmpty(reply.SerialVersionText));
        TestContext.WriteLine($"Serial/version: {reply.SerialVersionText}");
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task ReadSettings_OnTheDevice_AnswersWithAlarmSettingsTest()
    {
        var reply = await AskAsync(new RadexOneRequest(RadexOneRequestKind.ReadSettings)).ConfigureAwait(false);

        Assert.IsTrue(reply.TryGetSettings(out var settings));
        TestContext.WriteLine($"Alarm={settings.AlarmMode} Threshold={settings.Threshold}");
    }
}
