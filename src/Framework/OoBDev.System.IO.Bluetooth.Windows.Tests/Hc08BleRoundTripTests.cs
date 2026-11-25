using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Bluetooth.Windows.Tests;

/// <summary>
/// Bench loop-back for the BLE transport: an SH-HC-08 module (a BLE peripheral with a serial side) is wired to a COM port on this
/// machine, and this machine's Bluetooth radio is its BLE central. Bytes written over BLE must come out of the COM port, and bytes
/// written to the COM port must arrive as BLE notifications. The module answers AT commands on the COM port only while no BLE
/// client is connected.
/// </summary>
[TestClass]
[TestCategory(TestCategories.Hardware)]
[TestCategory(TestCategories.Ble)]
[TestCategory(TestCategories.Serial)]
public sealed class Hc08BleRoundTripTests
{
    // The HC-08 exposes one service (FFE0) with one characteristic (FFE1) that is both the write and the notify characteristic.
    private const string _serviceUuid = "0000FFE0-0000-1000-8000-00805F9B34FB";
    private const string _characteristicUuid = "0000FFE1-0000-1000-8000-00805F9B34FB";
    private static readonly TimeSpan _wait = TimeSpan.FromSeconds(10);

    public TestContext TestContext { get; set; } = null!;

    private string? PortName => TestContext.Properties.TryGetValue("HC08_SERIAL_PORT", out var value) ? value as string : null;

    [TestMethod]
    public void SerialSide_AtCommands_AnswerTheVersionAndAddress()
    {
        using var port = OpenModule();

        Assert.AreEqual("OK", Ask(port, "AT"));
        StringAssert.StartsWith(Ask(port, "AT+VERSION"), "OK:");
        Assert.AreEqual(12, Ask(port, "AT+ADDR?").Replace("OK+ADDR:", string.Empty, StringComparison.Ordinal).Length);
    }

    [TestMethod]
    public async Task BleToSerial_BytesWrittenOverBle_ComeOutOfTheComPort()
    {
        using var port = OpenModule();
        await using var ble = await ConnectAsync(port).ConfigureAwait(false);
        var payload = Encoding.ASCII.GetBytes("ble-to-serial");

        await ble.WriteAsync(payload).ConfigureAwait(false);

        Assert.AreEqual("ble-to-serial", await ReadSerialAsync(port, payload.Length).ConfigureAwait(false));
    }

    [TestMethod]
    public async Task SerialToBle_BytesWrittenToTheComPort_ArriveAsNotifications()
    {
        using var port = OpenModule();
        await using var ble = await ConnectAsync(port).ConfigureAwait(false);
        var received = new StringBuilder();
        var arrived = new SemaphoreSlim(0);
        ble.NotificationReceived += (_, data) =>
        {
            lock (received)
            {
                received.Append(Encoding.ASCII.GetString(data.Span));
            }

            arrived.Release();
        };

        port.Write("serial-to-ble");

        var deadline = DateTime.UtcNow + _wait;
        while (DateTime.UtcNow < deadline && !Contains(received, "serial-to-ble"))
        {
            await arrived.WaitAsync(TimeSpan.FromMilliseconds(250)).ConfigureAwait(false);
        }

        Assert.IsTrue(Contains(received, "serial-to-ble"), $"Notifications carried '{received}'.");
    }

    private static bool Contains(StringBuilder text, string expected)
    {
        lock (text)
        {
            return text.ToString().Contains(expected, StringComparison.Ordinal);
        }
    }

    private SerialPort OpenModule()
    {
        var name = PortName;
        if (string.IsNullOrWhiteSpace(name) || !SerialPort.GetPortNames().Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            Assert.Inconclusive("HC08_SERIAL_PORT is blank or not present: is the HC-08 attached?");
        }

        var port = new SerialPort(name, 9600, Parity.None, 8, StopBits.One) { ReadTimeout = 1000, WriteTimeout = 1000, Encoding = Encoding.ASCII };
        try
        {
            port.Open();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or global::System.IO.IOException)
        {
            port.Dispose();
            Assert.Inconclusive($"{name} cannot be opened ({ex.Message}): is the module attached and not in use?");
        }

        // A previous BLE client (an earlier test, a phone) keeps the module out of AT mode until its link drops.
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (Ask(port, "AT") != "OK")
        {
            if (DateTime.UtcNow > deadline)
            {
                port.Dispose();
                Assert.Inconclusive("The module still holds a BLE link after 30 s, so it does not answer AT commands: is another client connected?");
            }
        }

        return port;
    }

    /// <summary>One AT command; the module needs a pause before it answers and takes no line terminator on the command.</summary>
    private static string Ask(SerialPort port, string command)
    {
        port.DiscardInBuffer();
        port.Write(command);
        Thread.Sleep(600);
        return port.ReadExisting().Trim();
    }

    private async Task<IBleConnection> ConnectAsync(SerialPort port)
    {
        var address = Ask(port, "AT+ADDR?").Replace("OK+ADDR:", string.Empty, StringComparison.Ordinal);
        Assert.AreEqual(12, address.Length, "The module did not report its address.");
        var colonAddress = string.Join(':', Enumerable.Range(0, 6).Select(i => address.Substring(i * 2, 2)));

        var device = new WindowsBleDeviceDiscovery().GetDevices()
            .FirstOrDefault(d => d.DeviceId.Contains(colonAddress, StringComparison.OrdinalIgnoreCase));
        if (device is null)
        {
            Assert.Inconclusive($"The module's BLE address {colonAddress} was not found by discovery: is Bluetooth on, and is the module advertising?");
        }

        var connection = new WindowsBleConnectionFactory().Create(new BleDeviceAdapterOptions
        {
            DeviceId = device.DeviceId,
            ServiceUuid = _serviceUuid,
            WriteCharacteristicUuid = _characteristicUuid,
            NotifyCharacteristicUuid = _characteristicUuid,
        });
        // The first GATT connection after the radio has been idle can take longer than the rest, so a timed-out attempt is retried once.
        for (var attempt = 1; ; attempt++)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            try
            {
                await connection.ConnectAsync(timeout.Token).ConfigureAwait(false);
                return connection;
            }
            catch (OperationCanceledException) when (attempt < 2)
            {
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }
        }
    }

    private static async Task<string> ReadSerialAsync(SerialPort port, int count)
    {
        var buffer = new StringBuilder();
        var deadline = DateTime.UtcNow + _wait;
        while (DateTime.UtcNow < deadline && buffer.Length < count)
        {
            buffer.Append(port.ReadExisting());
            await Task.Delay(100).ConfigureAwait(false);
        }

        return buffer.ToString();
    }
}
