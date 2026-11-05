using System.IO.Ports;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Bridge.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class ConfigStoreTests
{
    private string _directory = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _directory = Path.Combine(Path.GetTempPath(), "ser2net-win-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_directory, recursive: true);

    [TestMethod]
    public void MissingFileLoadsAsEmptyConfig()
    {
        var store = new ConfigStore(Path.Combine(_directory, "none.json"));

        Assert.IsEmpty(store.Load().Bridges);
    }

    [TestMethod]
    public void MdnsIsOffByDefaultAndProtocolAndNameRoundTrip()
    {
        var store = new ConfigStore(Path.Combine(_directory, "m.json"));
        Assert.IsFalse(store.Load().Mdns.Enabled);

        var config = new Ser2NetConfig
        {
            Bridges = [new BridgeOptions { ComPort = "COM5", Protocol = BridgeProtocol.Rfc2217, Name = "zstick" }],
            Mdns = new MdnsOptions { Enabled = true },
        };
        store.Save(config);

        var loaded = store.Load();
        Assert.IsTrue(loaded.Mdns.Enabled);
        Assert.AreEqual(BridgeProtocol.Rfc2217, loaded.Bridges[0].Protocol);
        Assert.AreEqual("zstick", loaded.Bridges[0].Name);
        StringAssert.Contains(File.ReadAllText(store.FilePath), "\"Rfc2217\"");
    }

    [TestMethod]
    public void RoundTripsBridgesWithStringEnums()
    {
        var path = Path.Combine(_directory, "config.json");
        var store = new ConfigStore(path);
        var bridge = new BridgeOptions
        {
            ComPort = "COM13",
            BaudRate = 9600,
            DataBits = 7,
            Parity = global::System.IO.Ports.Parity.Even,
            StopBits = global::System.IO.Ports.StopBits.Two,
            TcpPort = 4001,
            BindAddress = "127.0.0.1",
        };

        store.Save(new Ser2NetConfig { Bridges = [bridge] });
        var loaded = store.Load();

        Assert.AreEqual(bridge, loaded.Bridges.Single());
        StringAssert.Contains(File.ReadAllText(path), "\"Parity\": \"Even\"");
    }

    [TestMethod]
    public void SavePreservesOtherSections()
    {
        var path = Path.Combine(_directory, "config.json");
        File.WriteAllText(path, """
            {
              // comment
              "Logging": { "LogLevel": { "Default": "Warning" } },
              "Bridges": []
            }
            """);
        var store = new ConfigStore(path);

        store.Save(new Ser2NetConfig { Bridges = [new BridgeOptions { ComPort = "COM3" }] });

        var text = File.ReadAllText(path);
        StringAssert.Contains(text, "\"Warning\"");
        Assert.AreEqual("COM3", store.Load().Bridges.Single().ComPort);
    }

    [TestMethod]
    public void MissingOptionalFieldsUseDefaults()
    {
        var path = Path.Combine(_directory, "config.json");
        File.WriteAllText(path, """{ "Bridges": [ { "ComPort": "COM13", "TcpPort": 4000 } ] }""");

        var bridge = new ConfigStore(path).Load().Bridges.Single();

        Assert.AreEqual(115200, bridge.BaudRate);
        Assert.AreEqual(8, bridge.DataBits);
        Assert.AreEqual(global::System.IO.Ports.Parity.None, bridge.Parity);
        Assert.AreEqual(global::System.IO.Ports.StopBits.One, bridge.StopBits);
    }
}
