# Network device probes: migration plan (done 2026-11-20)

Status: done 2026-11-20. The owner chose a new Framework library (Q25) and asked for the move on 2026-11-20 ("do these"); the table below is what was built, except the `LxiDiscovery` probe adapter, which is still open in the backlog (it needs a reference to `OoBDev.System.IO.Tcp`).

[← Overview](README.md)

**Table 1 — Proposed projects**

| Project | Layer folder | Namespace | Package | Contents | References |
|---------|--------------|-----------|---------|----------|------------|
| `OoBDev.System.Net.Discovery` (new) | Framework | `OoBDev.System.Net.Discovery` | `OoBDev.System.Net.Discovery` | `INetworkDeviceProbe`, `NetworkDiscovery`, `NetworkDevice`, `NetworkDeviceHit`, `MdnsProbe`, `SsdpProbe`, `UsrBridgeProbe`, `DnsMessage`, `DnsRecord`, `IUdpExchange`, `SystemUdpExchange`, `UdpReply` | `OoBDev.System.Abstractions` |
| `OoBDev.System.Net.Discovery.Tests` (new) | Framework | same + `.Tests` | none | the probe and merge tests moved from `OoBDev.DevTerm.Core.Tests` (`NetworkDiscoveryTests`) | the library, `OoBDev.TestUtilities` |
| `OoBDev.DevTerm.Core` (changed) | Applications | usings updated from `OoBDev.DevTerm.Core.Configuration.Discovery` | none | loses the Discovery folder; keeps the Connection Editor view model that calls the discovery | adds the new library |

`LxiDiscovery` stays in `OoBDev.System.IO.Tcp` (the VXI-11 portmapper probe); a thin `INetworkDeviceProbe` adapter for it is part of the move so the detect list can include LXI instruments from one registration.

Steps: move the files with `git mv` and fix namespaces, move the tests, add the project and test project to `src/OoBDev.slnx`, add `README.System.Net.Discovery.md`, update the references in DevTerm, rebuild DevTerm and the Connection Editor tests, update the [discovery design](network-device-discovery.md) and the catalog.
