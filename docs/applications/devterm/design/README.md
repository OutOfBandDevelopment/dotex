# DevTerm design documents

The application's own design documents stay here. Reusable capability designs moved to `docs/design/` and protocol references to `docs/protocols/` on 2026-11-04 (map: [migration plan](../../../migration/README.md#devterm-ui-shells-and-plugins)). Requirements, architecture, API design and testing strategy of the application are one level up: [README](../README.md).

## Contents

- [Application design documents](#application-design-documents)
- [Capability designs](#capability-designs)
- [Protocol references](#protocol-references)

## Application design documents

**Table 1 — Documents kept here**

| Document | Covers |
|----------|--------|
| [architecture.md](architecture.md) | Core abstractions (session, transport, presenter, pipeline, plugin host, front ends) and the data flow between them |
| [frontends.md](frontends.md) | The console app (CLI and TUI), the WPF app and the web host, and how they share the core |
| [platform.md](platform.md) | .NET, Generic Host as composition root and the Options pattern |
| [observability.md](observability.md) | Opt-in OpenTelemetry export of session traces and metrics |
| [testing.md](testing.md) | Test categories and how hardware-backed tests report Inconclusive |

## Capability designs

**Table 2 — Designs now under `docs/design/`**

| Folder | Covers |
|--------|--------|
| [Transports](../../../design/Transports/README.md) | Transport contract, RFC 2217, VXI-11, USBTMC, message brokers, LXI |
| [Presenters](../../../design/Presenters/README.md) | Text, numeric, decoding and rendering presenters |
| [Plugins](../../../design/Plugins/README.md) | Plugin model, out-of-process plugins, plugin-contributed panels |
| [UiDefinitions](../../../design/UiDefinitions/README.md) | UI definitions, device manifests, control modules, `.ksy` importer, schema files, expression builder |
| [ConnectionProfiles](../../../design/ConnectionProfiles/README.md) | Saved connection profiles |
| [MultiSessionUi](../../../design/MultiSessionUi/README.md) | Several sessions per window |
| [SessionLogging](../../../design/SessionLogging/README.md) | Session log format and playback |
| [Theming](../../../design/Theming/README.md) | Themes and the theme builder |
| [SessionChannel](../../../design/SessionChannel/README.md) | Cross-process session pipe |
| [NetworkDevices](../../../design/NetworkDevices/README.md) | Network device discovery and bridge config editors |
| [ProjectState](../../../design/ProjectState/README.md) | Saving and restoring connections |
| [WebFrontend](../../../design/WebFrontend/README.md) | Web host and Blazor front end |
| [ZWave](../../../design/ZWave/README.md) | Z-Wave Serial API support |
| [StreamTools](../../../design/StreamTools/README.md) | Stream content detection and converter tools |
| [ScpiInstruments](../../../design/ScpiInstruments/README.md) | SCPI instrument control |

## Protocol references

Device protocols (DE-5000, EByte E810-DTU, BYTECC BT-UP01, Busylight, K8055, Radex One, Tektronix 2230, Zoom H4n, NMEA, USBTMC) are in [docs/protocols](../../../protocols/), rejected ones in [rejected](../../../protocols/rejected/README.md).
