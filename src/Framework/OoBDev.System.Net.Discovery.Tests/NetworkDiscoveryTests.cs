using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Net.Discovery;

namespace OoBDev.System.Net.Discovery.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class NetworkDiscoveryTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task UsrBridgeProbe_KeepsOnlyHostsWhoseLoginRealmIsAUsrBridge()
    {
        var headers = new Dictionary<string, string?>
        {
            ["10.0.0.110"] = "HTTP/1.0 401 Unauthorized\r\nWWW-Authenticate: Basic realm=\"USR-TCP232-302\"\r\nServer: lwIP/1.4.1\r\n\r\n",
            ["10.0.0.48"] = "HTTP/1.1 301 Moved Permanently\r\n\r\n",
            ["10.0.0.67"] = "HTTP/1.0 401 Unauthorized\r\nWWW-Authenticate: Basic realm=\"NAS\"\r\n\r\n",
        };
        var probe = new UsrBridgeProbe(() => [.. headers.Keys, "10.0.0.99"], (host, _, _) => Task.FromResult(headers.GetValueOrDefault(host)));

        var hits = await probe.ProbeAsync(TimeSpan.FromSeconds(1), TestContext.CancellationToken);

        Assert.HasCount(1, hits);
        Assert.AreEqual("10.0.0.110", hits[0].Address);
        Assert.AreEqual(23, hits[0].Port);
        Assert.AreEqual("tcp", hits[0].Transport);
        Assert.AreEqual("usr-tcp232", hits[0].Kind);
        Assert.AreEqual("USR-TCP232-302 serial bridge", hits[0].DisplayName);
    }

    [TestMethod]
    public void GroupByDevice_GivesOneRowPerAddressWithAServiceBadgePerPort()
    {
        var devices = NetworkDiscovery.GroupByDevice(
        [
            new NetworkDeviceHit("10.0.0.9", 1883, "mqtt", "mqtt", "broker", "mdns"),
            new NetworkDeviceHit("10.0.0.5", 5025, "tcp", "lxi", "Rigol,DG1062Z", "lxi"),
            new NetworkDeviceHit("10.0.0.5", 0, "vxi11", "lxi", "Rigol", "lxi"),
            new NetworkDeviceHit("10.0.0.5", 80, "tcp", "unknown", "Rigol web", "ssdp", "dg1062z.local"),
        ]);

        Assert.HasCount(2, devices);
        Assert.AreEqual("10.0.0.5", devices[0].Address);
        Assert.AreEqual("Rigol,DG1062Z", devices[0].Name);
        Assert.AreEqual("vxi11, tcp:80, tcp:5025", devices[0].Badges);
        Assert.AreEqual("dg1062z.local", devices[0].Hostname);
        Assert.AreEqual("mqtt:1883", devices[1].Badges);
    }

    [TestMethod]
    public void BuildPtrQuery_EncodesLabelsAndTheUnicastBit()
    {
        var query = DnsMessage.BuildPtrQuery(["_mqtt._tcp.local"]);

        Assert.AreEqual(1, query[5]);
        CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("\u0005_mqtt"), query[12..18]);
        CollectionAssert.AreEqual(new byte[] { 0, 12, 0x80, 1 }, query[^4..]);
    }

    [TestMethod]
    public async Task MdnsProbe_TurnsPtrSrvAndAIntoAHit()
    {
        var probe = new MdnsProbe(new CannedExchange(new UdpReply(IPAddress.Parse("10.0.0.9"), MdnsAnswer())));

        var hits = await probe.ProbeAsync(TimeSpan.FromMilliseconds(10), TestContext.CancellationToken);

        Assert.HasCount(1, hits);
        Assert.AreEqual("10.0.0.5", hits[0].Address);
        Assert.AreEqual(1883, hits[0].Port);
        Assert.AreEqual("mqtt", hits[0].Kind);
        Assert.AreEqual("broker", hits[0].DisplayName);
        Assert.AreEqual("mdns", hits[0].Source);
    }

    [TestMethod]
    public async Task MdnsProbe_Ser2NetBridge_IsASerialBridgeHitOnItsTcpPort()
    {
        var probe = new MdnsProbe(new CannedExchange(new UdpReply(IPAddress.Parse("10.0.0.9"), MdnsAnswer("_ser2net._tcp", "overwatch-com4", 4000))));

        var hits = await probe.ProbeAsync(TimeSpan.FromMilliseconds(10), TestContext.CancellationToken);

        Assert.HasCount(1, hits);
        Assert.AreEqual("10.0.0.5", hits[0].Address);
        Assert.AreEqual(4000, hits[0].Port);
        Assert.AreEqual("tcp", hits[0].Transport);
        Assert.AreEqual("serial-bridge", hits[0].Kind);
        Assert.AreEqual("overwatch-com4", hits[0].DisplayName);
    }

    [TestMethod]
    public async Task MdnsProbe_ZWaveBridge_IsASerialBridgeHit()
    {
        var probe = new MdnsProbe(new CannedExchange(new UdpReply(IPAddress.Parse("10.0.0.9"), MdnsAnswer("_zwave._tcp", "zstick", 4000))));

        var hits = await probe.ProbeAsync(TimeSpan.FromMilliseconds(10), TestContext.CancellationToken);

        Assert.HasCount(1, hits);
        Assert.AreEqual("serial-bridge", hits[0].Kind);
    }

    [TestMethod]
    public void ParseRecords_NeverThrowsOnGarbageOrTruncatedInput()
    {
        var answer = MdnsAnswer();
        for (var length = 0; length < answer.Length; length++)
        {
            _ = DnsMessage.ParseRecords(answer.AsSpan(0, length));
        }

        Assert.IsEmpty(DnsMessage.ParseRecords([0xFF, 0xFF, 0xFF]));
    }

    [TestMethod]
    public async Task SsdpProbe_TakesThePortFromLocationAndIgnoresNon200()
    {
        var ok = "HTTP/1.1 200 OK\r\nLOCATION: http://10.0.0.7:49152/desc.xml\r\nSERVER: Linux UPnP/1.0 Widget/2\r\n\r\n";
        var search = "M-SEARCH * HTTP/1.1\r\n\r\n";
        var probe = new SsdpProbe(new CannedExchange(
            new UdpReply(IPAddress.Parse("10.0.0.7"), Encoding.ASCII.GetBytes(ok)),
            new UdpReply(IPAddress.Parse("10.0.0.8"), Encoding.ASCII.GetBytes(search))));

        var hits = await probe.ProbeAsync(TimeSpan.FromMilliseconds(10), TestContext.CancellationToken);

        Assert.HasCount(1, hits);
        Assert.AreEqual(49152, hits[0].Port);
        Assert.AreEqual("upnp", hits[0].Kind);
        Assert.AreEqual("Linux UPnP/1.0 Widget/2", hits[0].DisplayName);
    }

    [TestMethod]
    public void Merge_CollapsesSameEndpointPreferringAKnownKindAndOrdersByAddress()
    {
        var merged = NetworkDiscovery.Merge(
        [
            new NetworkDeviceHit("10.0.0.10", 80, "tcp", "upnp", "x", "ssdp"),
            new NetworkDeviceHit("10.0.0.2", 5555, "tcp", "unknown", "?", "mdns"),
            new NetworkDeviceHit("10.0.0.2", 5555, "tcp", "lxi", "RIGOL", "lxi"),
        ]);

        Assert.HasCount(2, merged);
        Assert.AreEqual("lxi", merged[0].Kind);
        Assert.AreEqual("10.0.0.10", merged[1].Address);
    }

    private static byte[] MdnsAnswer(string service = "_mqtt._tcp", string instance = "broker", int port = 1883)
    {
        var message = new List<byte> { 0, 0, 0x84, 0, 0, 0, 0, 1, 0, 0, 0, 2 };
        message.AddRange(Name(service + ".local"));
        message.AddRange(Record(12, Name(instance + "." + service + ".local")));
        message.AddRange(Name(instance + "." + service + ".local"));
        message.AddRange(Record(33, [0, 0, 0, 0, (byte)(port >> 8), (byte)port, .. Name("host.local")]));
        message.AddRange(Name("host.local"));
        message.AddRange(Record(1, [10, 0, 0, 5]));
        message[7] = 1;
        message[11] = 2;
        return [.. message];
    }

    private static byte[] Name(string name)
    {
        var bytes = new List<byte>();
        foreach (var label in name.Split('.'))
        {
            bytes.Add((byte)label.Length);
            bytes.AddRange(Encoding.ASCII.GetBytes(label));
        }

        bytes.Add(0);
        return [.. bytes];
    }

    private static byte[] Record(int type, byte[] data) => [0, (byte)type, 0, 1, 0, 0, 0, 120, (byte)(data.Length >> 8), (byte)data.Length, .. data];

    private sealed class CannedExchange(params UdpReply[] replies) : IUdpExchange
    {
        public Task<IReadOnlyList<UdpReply>> ExchangeAsync(IPEndPoint target, byte[] request, TimeSpan listenFor, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<UdpReply>>(replies);
    }

    private sealed class ThrowingProbe : INetworkDeviceProbe
    {
        public string Name => "boom";

        public Task<IReadOnlyList<NetworkDeviceHit>> ProbeAsync(TimeSpan listenFor, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("boom");
    }
}
