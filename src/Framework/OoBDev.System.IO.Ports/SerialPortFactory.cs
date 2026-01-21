using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SerialPort = System.IO.Ports.SerialPort;

namespace OoBDev.System.IO.Ports;

/// <summary>
/// Factory for creating serial port device adapters based on <see cref="SerialPortAttribute"/> configuration.
/// </summary>
[DeviceTarget(typeof(SerialPortAttribute))]
public class SerialPortFactory : IDeviceFactory
{
    /// <inheritdoc/>
    public bool CanGetDevice(object? definition) => definition?.GetType()?.GetCustomAttributes<SerialPortAttribute>()?.Any() ?? false;

    /// <inheritdoc/>
    public IDeviceAdapter? GetDevice(string devicePath, object? definition)
    {
        var assignedDevicePath = SerialPort.GetPortNames()
                               .FirstOrDefault(sp => string.Equals(sp, devicePath, StringComparison.InvariantCultureIgnoreCase));
        if (string.IsNullOrWhiteSpace(assignedDevicePath))
            return null;
        if (definition == null)
            return null;

        var def = definition.GetType();
        var config = def.GetCustomAttribute<SerialPortAttribute>();
        if (config == null)
            return null;

        return new SerialPortDeviceAdapter(
            new SerialPort(
                portName: assignedDevicePath,
                baudRate: config.BaudRate,
                parity: config.Parity.AsSystem(),
                dataBits: config.DataBits,
                stopBits: config.StopBits.AsSystem()
                )
            {
                ReadTimeout = config.ReadTimeout,
                WriteTimeout = config.WriteTimeout,
            });
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetDeviceNames() => SerialPort.GetPortNames().OrderBy(s => s);
}
