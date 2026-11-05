using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Bridge.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class MdnsPlanTests
{
    [TestMethod]
    public void DefaultEntryOnlyWhenNoExtras()
    {
        var plan = MdnsAdvertiser.Plan(new BridgeOptions { ComPort = "COM13", Name = "zstick", Protocol = BridgeProtocol.Rfc2217 }, 4000, "_ser2net._tcp");

        Assert.HasCount(1, plan);
        Assert.AreEqual("_ser2net._tcp", plan[0].ServiceType);
        Assert.AreEqual("zstick", plan[0].Name);
        Assert.AreEqual("rfc2217", plan[0].Txt!["protocol"]);
    }

    [TestMethod]
    public void ExtrasInheritNamePortAndTxtUnlessOverridden()
    {
        var bridge = new BridgeOptions
        {
            ComPort = "COM13",
            MdnsServices =
            [
                new MdnsService { ServiceType = "_zwave._tcp" },
                new MdnsService { ServiceType = "_other._tcp", Name = "alt", Port = 5000, Txt = new() { ["k"] = "v" } },
            ],
        };

        var plan = MdnsAdvertiser.Plan(bridge, 4000, "_ser2net._tcp");

        Assert.HasCount(3, plan);
        Assert.AreEqual("COM13", plan[1].Name);
        Assert.AreEqual(4000, plan[1].Port);
        Assert.AreEqual("raw", plan[1].Txt!["protocol"]);
        Assert.AreEqual("alt", plan[2].Name);
        Assert.AreEqual(5000, plan[2].Port);
        Assert.AreEqual("v", plan[2].Txt!["k"]);
    }

    [TestMethod]
    public void ValidatorRejectsBadServiceTypes()
    {
        var bad = new BridgeOptions { MdnsServices = [new MdnsService { ServiceType = "zwave" }] };
        var good = new BridgeOptions { MdnsServices = [new MdnsService { ServiceType = "_zwave-js-server._tcp" }] };

        Assert.IsNotEmpty(ConfigValidator.Validate(bad));
        Assert.IsEmpty(ConfigValidator.Validate(good));
    }
}
