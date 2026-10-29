using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.ZWave.Tests;

[TestClass]
public class ZWaveNamesTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Manufacturer_Registered_ReturnsName() =>
        Assert.AreEqual("2B Electronics", ZWaveNames.Manufacturer(0x0028));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Manufacturer_Unknown_ReturnsHex() =>
        Assert.AreEqual("0xFFFE", ZWaveNames.Manufacturer(0xFFFE));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Product_Registered_ReturnsName() =>
        Assert.AreEqual("CT50e Thermostat", ZWaveNames.Product(0x0098, 0x3200, 0x015E));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Product_Unknown_ReturnsNull() =>
        Assert.IsNull(ZWaveNames.Product(0x0098, 0xFFFF, 0xFFFF));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CommandClass_OnlyInRegistry_ReturnsRegistryName() =>
        Assert.AreEqual("Controller Replication", ZWaveNames.CommandClass(0x21));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CommandClass_Unknown_ReturnsHex() =>
        Assert.AreEqual("class 0xFE", ZWaveNames.CommandClass(0xFE));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void DeviceClass_Registered_ReturnsName() =>
        Assert.AreEqual("Static PC Controller", ZWaveNames.DeviceClass(0x02, 0x01));
}
