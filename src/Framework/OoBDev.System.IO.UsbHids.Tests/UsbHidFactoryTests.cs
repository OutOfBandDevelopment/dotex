using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.UsbHids.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class UsbHidFactoryTests
{
    [UsbHid(0xFFFE, 0x0001)]
    private sealed class AbsentDevice;

    private sealed class Undecorated;

    [TestMethod]
    public void CanGetDevice_DecoratedDefinition_IsTrue() =>
        Assert.IsTrue(new UsbHidFactory().CanGetDevice(new AbsentDevice()));

    [TestMethod]
    public void CanGetDevice_UndecoratedOrNull_IsFalse()
    {
        Assert.IsFalse(new UsbHidFactory().CanGetDevice(new Undecorated()));
        Assert.IsFalse(new UsbHidFactory().CanGetDevice(null));
    }

    [TestMethod]
    public void GetDevices_NothingMatchesTheIds_ReturnsNone()
    {
        Assert.AreEqual(0, new UsbHidFactory().GetDevices(new AbsentDevice()).Count());
        Assert.IsNull(new UsbHidFactory().GetDevice(new AbsentDevice()));
    }

    [TestMethod]
    public void GetDevices_NullOrUndecorated_ReturnsNone()
    {
        Assert.AreEqual(0, new UsbHidFactory().GetDevices(null).Count());
        Assert.AreEqual(0, new UsbHidFactory().GetDevices(new Undecorated()).Count());
    }
}
