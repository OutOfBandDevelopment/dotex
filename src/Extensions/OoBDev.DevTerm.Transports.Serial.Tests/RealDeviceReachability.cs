using System;
using System.Linq;
using global::System.IO.Ports;

namespace OoBDev.DevTerm.Transports.Serial.Tests;

internal static class RealDeviceReachability
{
    public static bool IsSerialPortAvailable(string portName) =>
        SerialPort.GetPortNames().Contains(portName, StringComparer.OrdinalIgnoreCase);
}
