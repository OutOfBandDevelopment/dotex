using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Usb.Bridge.Tests;

[TestClass]
public sealed class UsbBridgeConfigTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Load_ReadsUsbBridges_AndIgnoresOtherSections()
    {
        var path = Path.Combine(TestContext.TestRunDirectory!, "cfg-" + global::System.Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, """{ "Bridges": [], "UsbBridges": [ { "Kind": "Hid", "UsbVendorId": "04D8", "UsbProductId": "F848", "TcpPort": 4102 } ] }""");
        var bridges = UsbBridgeConfig.Load(path);
        Assert.AreEqual(1, bridges.Count);
        Assert.AreEqual(UsbBridgeKind.Hid, bridges[0].Kind);
        Assert.AreEqual(4102, bridges[0].TcpPort);
        Assert.AreEqual(0, bridges[0].Validate().Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Load_MissingFile_ReturnsNone() => Assert.AreEqual(0, UsbBridgeConfig.Load("does-not-exist.json").Count);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Validate_BadIdsAndPort_ReportsEach()
    {
        var errors = new UsbBridgeOptions { UsbVendorId = "ZZ", TcpPort = 0 }.Validate();
        Assert.AreEqual(3, errors.Count);
    }
}
