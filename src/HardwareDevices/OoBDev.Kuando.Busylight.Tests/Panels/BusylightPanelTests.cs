using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Kuando.Busylight.Panels.Tests;

[TestClass]
[TestCategory(TestCategories.Kuando_Busylight)]
public sealed class BusylightPanelTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("hid", 0x04D8, 0xF848, true)]
    [DataRow("hid", 0x27BB, 0x3BCA, true)]
    [DataRow("hid", 0x04D8, 0x0001, false)]
    [DataRow("tcp", 0x27BB, 0x3BCA, false)]
    public void IsAvailable_MatchesTheBuiltInBusylightGate(string transport, int vendorId, int productId, bool expected) =>
        Assert.AreEqual(expected, new BusylightPanel().IsAvailable(transport, vendorId, productId));
}
