using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Ports;

public sealed class SystemSerialPortFactory : ISerialPortFactory
{
    public ISerialPort Create(SerialDeviceAdapterOptions options) => new SystemSerialPort(options);
}
