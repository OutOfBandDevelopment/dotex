using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.System.IO.Bluetooth.Windows;

public sealed class WindowsBleConnectionFactory : IBleConnectionFactory
{
    public IBleConnection Create(BleDeviceAdapterOptions options) => new WindowsBleConnection(options);
}
