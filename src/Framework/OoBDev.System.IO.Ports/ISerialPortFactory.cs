using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Ports;

/// <summary>
/// Creates the <see cref="ISerialPort"/> a <see cref="SerialDeviceAdapter"/> opens. Indirection
/// point that lets tests substitute a fake factory/port instead of touching real hardware.
/// </summary>
public interface ISerialPortFactory
{
    ISerialPort Create(SerialDeviceAdapterOptions options);
}

public sealed class SystemSerialPortFactory : ISerialPortFactory
{
    public ISerialPort Create(SerialDeviceAdapterOptions options) => new SystemSerialPort(options);
}
