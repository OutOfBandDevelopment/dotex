using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration.Discovery;
using OoBDev.System.IO.Tcp;

namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class LxiDeviceOptionTests
{
    [TestMethod]
    public void FromDevice_WithARawScpiPort_KeepsIt()
    {
        var option = LxiDeviceOption.FromDevice(new LxiDevice("10.0.0.7", 1024, 5555, "RIGOL,DG1062Z"));

        Assert.AreEqual(new LxiDeviceOption("RIGOL,DG1062Z  (10.0.0.7:5555)", "10.0.0.7", 5555), option);
    }

    [TestMethod]
    public void FromDevice_WithoutARawScpiPort_FallsBackToTheStandardOne()
    {
        var option = LxiDeviceOption.FromDevice(new LxiDevice("10.0.0.8", 1024, 0, string.Empty));

        Assert.AreEqual(5025, option.Port);
        Assert.AreEqual("tcp", option.Transport);
    }

    [TestMethod]
    public void FromHit_Vxi11_DropsThePortAndSwitchesTheTransport()
    {
        var option = LxiDeviceOption.FromHit(new NetworkDeviceHit("10.0.0.9", 1024, "vxi11", "lxi", "Scope", "lxi"));

        Assert.AreEqual(0, option.Port);
        Assert.AreEqual("vxi11", option.Transport);
        Assert.AreEqual("10.0.0.9", option.Host);
    }

    [TestMethod]
    public void FromHit_Mqtt_KeepsThePortAndSwitchesTheTransport()
    {
        var option = LxiDeviceOption.FromHit(new NetworkDeviceHit("10.0.0.10", 1883, "mqtt", "mqtt", "Broker", "mdns"));

        Assert.AreEqual(1883, option.Port);
        Assert.AreEqual("mqtt", option.Transport);
    }

    [TestMethod]
    [DataRow("tcp")]
    [DataRow("unheard-of")]
    public void FromHit_AnythingElse_IsAPlainTcpHit(string transport)
    {
        var option = LxiDeviceOption.FromHit(new NetworkDeviceHit("10.0.0.11", 5025, transport, "unknown", "Thing", "ssdp"));

        Assert.AreEqual(5025, option.Port);
        Assert.AreEqual("tcp", option.Transport);
    }
}
