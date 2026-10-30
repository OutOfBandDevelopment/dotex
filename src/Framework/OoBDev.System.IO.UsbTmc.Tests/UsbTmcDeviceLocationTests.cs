using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.UsbTmc.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class UsbTmcDeviceLocationTests
{
    [TestMethod]
    public void Format_BusAndPortChain_LooksLikeASysfsName()
    {
        Assert.AreEqual("usb:1-4", UsbTmcDeviceLocation.Format(1, [4]));
        Assert.AreEqual("usb:2-1.3.2", UsbTmcDeviceLocation.Format(2, [1, 3, 2]));
    }

    [TestMethod]
    public void Format_NoPortChain_IsNull() => Assert.IsNull(UsbTmcDeviceLocation.Format(1, []));
}
