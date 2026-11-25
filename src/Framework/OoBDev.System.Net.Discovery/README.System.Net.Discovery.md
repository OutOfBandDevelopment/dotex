# OoBDev.System.Net.Discovery

Finds network devices on the local network and groups the hits into one row per device. Moved out of DevTerm on 2026-11-20 (owner Q25), so any application can use it.

## What it holds

| Type | Role |
|------|------|
| `INetworkDeviceProbe` | One discovery method; returns `NetworkDeviceHit` rows. |
| `NetworkDiscovery` | Runs the probes together (`CreateDefault`, `DiscoverAsync`) and merges hits per device (`GroupByDevice`). |
| `MdnsProbe`, `SsdpProbe` | Multicast DNS and SSDP over UDP. |
| `UsrBridgeProbe` | Finds USR-TCP232 bridges by the login realm of their web page, sweeping the local /24. |
| `IUdpExchange`, `SystemUdpExchange`, `UdpReply` | The UDP seam; tests replace it. |
| `DnsMessage`, `DnsRecord` | Minimal DNS wire format for mDNS. |
| `NetworkDevice`, `NetworkDeviceHit` | The result rows. |

## Use

```csharp
var hits = await NetworkDiscovery.CreateDefault().DiscoverAsync(TimeSpan.FromSeconds(3));
foreach (var device in NetworkDiscovery.GroupByDevice(hits))
    Console.WriteLine($"{device.Address} {device.Name}");
```

## Notes

- References only `OoBDev.System.Abstractions`. The VXI-11 portmapper probe stays in `OoBDev.System.IO.Tcp` (`LxiDiscovery`); the thin probe adapter that lets it join one detect list is open in the backlog.
- Tests: `OoBDev.System.Net.Discovery.Tests` (Unit, fake UDP exchange, no network).
- Design: [network device discovery](../../../docs/design/NetworkDevices/network-device-discovery.md), [migration plan](../../../docs/design/NetworkDevices/migration-plan.md).
