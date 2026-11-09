using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Tcp.Tests;

[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Tcp)]
public sealed class TcpTransportOptionsValidatorTests
{
    private readonly TcpEndpointOptionsValidator _validator = new();

    [TestMethod]
    public void Validate_ClientModeWithHostAndValidPort_Succeeds()
    {
        var result = _validator.Validate(null, new TcpEndpointOptions { Mode = TcpEndpointMode.Client, Host = "device.local", Port = 502 });

        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    public void Validate_ClientModeWithoutHost_Fails()
    {
        var result = _validator.Validate(null, new TcpEndpointOptions { Mode = TcpEndpointMode.Client, Host = null, Port = 502 });

        Assert.IsTrue(result.Failed);
    }

    [TestMethod]
    public void Validate_ListenerModeWithoutHost_Succeeds()
    {
        var result = _validator.Validate(null, new TcpEndpointOptions { Mode = TcpEndpointMode.Listener, Host = null, Port = 502 });

        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(65536)]
    public void Validate_PortOutOfRange_Fails(int port)
    {
        var result = _validator.Validate(null, new TcpEndpointOptions { Mode = TcpEndpointMode.Listener, Port = port });

        Assert.IsTrue(result.Failed);
    }
}
