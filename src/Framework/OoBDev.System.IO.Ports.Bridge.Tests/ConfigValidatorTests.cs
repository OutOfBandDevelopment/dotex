using System.IO.Ports;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Bridge.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class ConfigValidatorTests
{
    [TestMethod]
    public void DefaultBridgeIsValid()
    {
        Assert.IsEmpty(ConfigValidator.Validate(new BridgeOptions()));
    }

    [TestMethod]
    public void DuplicateComPortsAreRejectedCaseInsensitively()
    {
        var errors = ConfigValidator.Validate(
        [
            new BridgeOptions { ComPort = "COM3", TcpPort = 4000 },
            new BridgeOptions { ComPort = "com3", TcpPort = 4001 },
        ]);

        Assert.HasCount(1, errors);
        StringAssert.Contains(errors[0], "more than once");
    }

    [TestMethod]
    public void DuplicateTcpPortsAreRejected()
    {
        var errors = ConfigValidator.Validate(
        [
            new BridgeOptions { ComPort = "COM3", TcpPort = 4000 },
            new BridgeOptions { ComPort = "COM4", TcpPort = 4000 },
        ]);

        Assert.HasCount(1, errors);
        StringAssert.Contains(errors[0], "4000");
    }

    [TestMethod]
    public void OutOfRangeValuesAreReported()
    {
        var errors = ConfigValidator.Validate(new BridgeOptions
        {
            BaudRate = 0,
            DataBits = 9,
            StopBits = global::System.IO.Ports.StopBits.None,
            TcpPort = 70000,
            BindAddress = "not-an-ip",
        });

        Assert.HasCount(5, errors);
    }
}
