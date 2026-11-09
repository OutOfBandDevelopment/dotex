using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Bluetooth.Tests;

[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Ble)]
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
