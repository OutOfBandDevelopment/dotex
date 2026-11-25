using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Bridge.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class UsbPortResolverTests
{
    public required TestContext TestContext { get; set; }

    private static readonly UsbPortInfo[] _ports =
    [
        new("COM3", string.Empty, string.Empty, string.Empty),
        new("COM13", "067B", "2303", string.Empty),
        new("COM21", "0658", "0200", "ABC123"),
    ];

    [TestMethod]
    public void Resolve_NoUsbId_ReturnsConfiguredComPort()
    {
        var port = UsbPortResolver.Resolve(new BridgeOptions { ComPort = "COM7" }, _ports);

        Assert.AreEqual("COM7", port);
    }

    [TestMethod]
    public void Resolve_VendorAndProduct_FindsThePortOnWhateverNumberItHas()
    {
        var options = new BridgeOptions { UsbVendorId = "067b", UsbProductId = "0x2303" };

        var before = UsbPortResolver.Resolve(options, _ports);
        var renumbered = UsbPortResolver.Resolve(options, [new("COM44", "067B", "2303", string.Empty)]);
        TestContext.WriteLine($"{before} then {renumbered}");

        Assert.AreEqual("COM13", before);
        Assert.AreEqual("COM44", renumbered);
    }

    [TestMethod]
    public void Resolve_DeviceUnplugged_ThrowsIOException()
    {
        var options = new BridgeOptions { UsbVendorId = "067B", UsbProductId = "2303" };

        var ex = Assert.Throws<IOException>(() => UsbPortResolver.Resolve(options, [_ports[0]]));

        StringAssert.Contains(ex.Message, "067B:2303");
    }

    [TestMethod]
    public void Resolve_TwoDevicesWithoutSerial_RefusesAndNamesBoth()
    {
        UsbPortInfo[] ports = [new("COM5", "067B", "2303", "A"), new("COM6", "067B", "2303", "B")];
        var options = new BridgeOptions { UsbVendorId = "067B", UsbProductId = "2303" };

        var ex = Assert.Throws<InvalidOperationException>(() => UsbPortResolver.Resolve(options, ports));
        TestContext.WriteLine(ex.Message);

        StringAssert.Contains(ex.Message, "COM5");
        StringAssert.Contains(ex.Message, "COM6");
    }

    [TestMethod]
    public void Resolve_TwoDevicesWithSerial_PicksTheMatchingOne()
    {
        UsbPortInfo[] ports = [new("COM5", "067B", "2303", "A"), new("COM6", "067B", "2303", "B")];
        var options = new BridgeOptions { UsbVendorId = "067B", UsbProductId = "2303", UsbSerialNumber = "b" };

        Assert.AreEqual("COM6", UsbPortResolver.Resolve(options, ports));
    }

    [TestMethod]
    public void Validate_UsbIdsWithoutComPort_IsValid()
    {
        var errors = ConfigValidator.Validate(new BridgeOptions { ComPort = string.Empty, UsbVendorId = "067B", UsbProductId = "2303" });

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void Validate_BadHexId_IsRejected()
    {
        var errors = ConfigValidator.Validate(new BridgeOptions { UsbVendorId = "ZZ", UsbProductId = "2303" });

        Assert.HasCount(1, errors);
        StringAssert.Contains(errors[0], "UsbVendorId");
    }

    [TestMethod]
    public void Validate_TwoUsbSelectedBridges_AreNotFlaggedAsDuplicateComPorts()
    {
        var errors = ConfigValidator.Validate(
        [
            new BridgeOptions { ComPort = string.Empty, UsbVendorId = "067B", UsbProductId = "2303", TcpPort = 4000 },
            new BridgeOptions { ComPort = string.Empty, UsbVendorId = "0658", UsbProductId = "0200", TcpPort = 4001 },
        ]);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void Open_UsbSelected_ResolvesThroughTheLister()
    {
        var factory = new SystemSerialPortFactory(new FakeLister(new UsbPortInfo("COM13", "067B", "2303", string.Empty)));
        var options = new BridgeOptions { UsbVendorId = "FFFF", UsbProductId = "FFFF" };

        var ex = Assert.Throws<IOException>(() => factory.Open(options));

        StringAssert.Contains(ex.Message, "FFFF:FFFF");
    }

    private sealed class FakeLister(params UsbPortInfo[] ports) : IUsbPortLister
    {
        public IReadOnlyList<UsbPortInfo> List() => ports;
    }
}
