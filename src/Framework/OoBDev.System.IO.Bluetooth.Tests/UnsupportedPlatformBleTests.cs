using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.Bluetooth.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class UnsupportedPlatformBleTests
{
    [TestMethod]
    public void Create_ThrowsPlatformNotSupported()
    {
        var factory = new UnsupportedPlatformBleConnectionFactory();

        Assert.ThrowsExactly<PlatformNotSupportedException>(() => factory.Create(new BleDeviceAdapterOptions { DeviceId = "x" }));
    }

    [TestMethod]
    public void GetDevices_ReturnsEmpty()
    {
        var discovery = new UnsupportedPlatformBleDeviceDiscovery();

        Assert.IsEmpty(discovery.GetDevices());
    }
}
