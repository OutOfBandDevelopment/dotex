using System;
using System.Linq;
using global::System.IO.Ports;

namespace OoBDev.System.IO.Ports.Tests;

internal static class RealDeviceReachability
{
    public static bool IsSerialPortAvailable(string portName) =>
        SerialPort.GetPortNames().Contains(portName, StringComparer.OrdinalIgnoreCase);
}
