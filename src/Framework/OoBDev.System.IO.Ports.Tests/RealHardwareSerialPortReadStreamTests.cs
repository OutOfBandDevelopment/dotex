using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.Ports.Tests;


/// <summary>
/// Opt-in real-hardware regression test for bug 057: a <see cref="SerialPortReadStream.ReadAsync"/>
/// waiting on <see cref="SerialPort.DataReceived"/> with no bytes pending had no way to notice the
/// port going away underneath it, since nothing else could complete its wait. Reuses whichever real
/// serial device <c>devterm.runsettings</c> already points at (the Radex One on COM8, per
/// <c>RealSerialRadexOnePort</c>) purely as a real, already-open <see cref="SerialPort"/> to read
/// from - the Radex One's own protocol is irrelevant here, since this drives
/// <see cref="SerialPortReadStream"/> directly rather than going through
/// <c>DevTerm.Devices.RadexOne</c>. See docs/bugs/resolved/057-serial-unplug-not-detected.md.
/// </summary>
[TestCategory("Hardware")]
[TestClass]
public sealed class RealHardwareSerialPortReadStreamTests
{
    public TestContext TestContext { get; set; } = null!;

    private string? GetProperty(string name) => TestContext.Properties.TryGetValue(name, out var value) ? value as string : null;

    [TestMethod]
    [TestCategory("Hardware")]
    public async Task RealDevice_PortGoesAwayWhileWaitingForData_ReadAsyncNoticesInsteadOfHangingForever()
    {
        var portName = GetProperty("RealSerialRadexOnePort");
        if (string.IsNullOrEmpty(portName))
        {
            Assert.Inconclusive("No 'RealSerialRadexOnePort' - run with 'dotnet test --settings devterm.runsettings' to exercise this against real hardware.");
            return;
        }

        if (!RealDeviceReachability.IsSerialPortAvailable(portName))
        {
            Assert.Inconclusive($"'{portName}' is not currently enumerated by the OS - is the device plugged in?");
            return;
        }

        using var port = new SerialPort(portName, 9600, global::System.IO.Ports.Parity.None, 8, global::System.IO.Ports.StopBits.One) { Handshake = global::System.IO.Ports.Handshake.None };
        port.Open();

        var stream = new SerialPortReadStream(port);
        var buffer = new byte[64];

        // Nothing queries the device, so BytesToRead stays 0 and ReadAsync waits on the
        // DataReceived/ErrorReceived events exactly like it would after a real reply finished and
        // before the next one arrives - the same "no bytes pending" state a physical unplug can
        // happen in.
        var readTask = stream.ReadAsync(buffer, TestContext.CancellationToken).AsTask();
        await Task.Delay(TimeSpan.FromMilliseconds(300), TestContext.CancellationToken);
        Assert.IsFalse(readTask.IsCompleted, "Expected ReadAsync to still be waiting for data before the port was closed.");

        // Simulates the device going away with no data in flight: neither DataReceived nor
        // ErrorReceived fires for this (confirmed by the pre-fix version of this test hanging past
        // its own WaitAsync timeout below), the same gap a physical unplug leaves. The pre-057-fix
        // code had no other way to notice and would wait forever; the fix's BytesToRead poll
        // fallback touches the closed port's handle and throws, which is what this asserts.
        port.Close();

        var completed = await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromSeconds(5), TestContext.CancellationToken));
        Assert.AreSame(readTask, completed, "ReadAsync hung instead of noticing the port had gone away.");
        Assert.IsTrue(
            readTask.IsFaulted || readTask.IsCanceled,
            $"Expected ReadAsync to end with a failure once the port closed out from under it, but it completed normally (status: {readTask.Status}).");
    }
}
