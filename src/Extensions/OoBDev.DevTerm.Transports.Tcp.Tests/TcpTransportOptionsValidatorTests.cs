using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Transports.Tcp.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class TcpTransportOptionsValidatorTests
{
    private readonly TcpTransportOptionsValidator _validator = new();

    [TestMethod]
    public void Validate_ClientModeWithHostAndValidPort_Succeeds()
    {
        var result = _validator.Validate(null, new TcpTransportOptions { Mode = TcpTransportMode.Client, Host = "device.local", Port = 502 });

        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    public void Validate_ClientModeWithoutHost_Fails()
    {
        var result = _validator.Validate(null, new TcpTransportOptions { Mode = TcpTransportMode.Client, Host = null, Port = 502 });

        Assert.IsTrue(result.Failed);
    }

    [TestMethod]
    public void Validate_ListenerModeWithoutHost_Succeeds()
    {
        var result = _validator.Validate(null, new TcpTransportOptions { Mode = TcpTransportMode.Listener, Host = null, Port = 502 });

        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(65536)]
    public void Validate_PortOutOfRange_Fails(int port)
    {
        var result = _validator.Validate(null, new TcpTransportOptions { Mode = TcpTransportMode.Listener, Port = port });

        Assert.IsTrue(result.Failed);
    }
}
