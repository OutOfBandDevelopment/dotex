using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO.UsbHids;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.IO.Ports;
using System.Linq;

namespace OoBDev.Devices.TestUtilities;

/// <summary>
/// Checks that a physical device is reachable before a <c>Hardware</c> test uses it; otherwise the test ends
/// <see cref="Assert.Inconclusive(string)"/>.
/// </summary>
public static class DevicePreflight
{
    /// <summary>
    /// Returns the serial port named by the run parameter <paramref name="parameter"/>, or ends the test Inconclusive when the
    /// parameter is blank or the operating system does not list that port.
    /// </summary>
    /// <param name="testContext">The test context that carries the run parameters.</param>
    /// <param name="parameter">The run parameter that names the port, for example <c>ZWAVE_SERIAL_PORT</c>.</param>
    /// <returns>The name of a port that is present.</returns>
    public static string RequireSerialPort(TestContext testContext, string parameter)
    {
        ArgumentNullException.ThrowIfNull(testContext);

        var port = testContext.GetPropertyOrDefault(parameter, string.Empty);
        if (string.IsNullOrWhiteSpace(port))
            Assert.Inconclusive($"No '{parameter}' run parameter: set it in .runsettings to the serial port of the device.");

        if (!Array.Exists(SerialPort.GetPortNames(), name => string.Equals(name, port, StringComparison.OrdinalIgnoreCase)))
            Assert.Inconclusive($"'{port}' is not listed by the operating system: is the device plugged in?");

        return port;
    }

    /// <summary>
    /// Opens the first USB HID device that matches the <c>[UsbHid]</c> attribute on <paramref name="definition"/>, or ends the test
    /// Inconclusive when none is enumerated or it cannot be opened.
    /// </summary>
    /// <param name="definition">The device definition object carrying the <c>[UsbHid]</c> attribute.</param>
    /// <returns>The open HID stream; the caller disposes it.</returns>
    public static Stream RequireUsbHid(object definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var adapter = new UsbHidFactory().GetDevices(definition).FirstOrDefault();
        if (adapter is null)
            Assert.Inconclusive($"No USB HID device for {definition.GetType().Name} is enumerated: is it plugged in?");
        if (!adapter.TryOpen(out var stream) || stream is null)
            Assert.Inconclusive($"The {definition.GetType().Name} USB HID device is present but cannot be opened (is another program using it?).");
        return stream;
    }
}
