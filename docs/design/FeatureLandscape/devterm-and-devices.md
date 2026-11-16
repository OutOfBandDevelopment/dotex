# Landscape: DevTerm and devices

Part of the [feature landscape](README.md). Status is "a project exists under `src`", not "verified". The detailed state of the DevTerm work is in the [parity status](../DevTermParity/status.md), which is more current than the leaf designs.

## Contents

1. [Summary](#summary)
2. [Transports and DeviceDecoding](#transports-and-devicedecoding)
3. [Presenters, SessionLogging and SessionChannel](#presenters-sessionlogging-and-sessionchannel)
4. [UiDefinitions, Theming and WindowsForms](#uidefinitions-theming-and-windowsforms)
5. [Plugins and PluginSandbox](#plugins-and-pluginsandbox)
6. [Instruments and networks](#instruments-and-networks)

**List of tables**

1. [Table 1 — DevTerm and device features](#table-1)

## Summary

**Table 1 — DevTerm and device features**

| Feature | Code | Standards | Main gap |
|---------|------|-----------|----------|
| Transports | `System.IO.Brokers`, `.Mqtt`, `.Ports.Bridge`, `.UsbHids`, `.UsbTmc`, loopback in DevTerm.Core | RFC 2217, MQTT 3.1.1/5, STOMP 1.2, USBTMC | BLE and VXI-11 not in the project list |
| DeviceDecoding | per-device projects (Busylight, DE-5000, K8055, Radex One, Zoom H4n, Nmea, ZWave) | NMEA 0183, Kaitai Struct | shared decoder test fixtures |
| Presenters | text and structured presenters, `KeyValue` plugin | none | none recorded |
| SessionLogging | log to file, notes, playback, merged log in Wpf | none | log format not versioned |
| SessionChannel, MultiSessionUi, ProjectState, ConnectionProfiles | in DevTerm.Core and Wpf slices | none | leaf designs have no status line |
| UiDefinitions | Wpf, Tui, Blazor, ViewModels | none | Avalonia and WinForms renderers (backlog, layering question open) |
| Theming | `WpfTheme`, theme builder, colour picker | WCAG 2.2 contrast | Tui and Web themes |
| WindowsForms | `Extensions.Windows.Forms` | none | no UiDefinitions renderer |
| Plugins | `DevTerm.Plugins.KeyValue`, `DevTerm.Core.TestPlugin` | none | plugin discovery order not documented |
| PluginSandbox | `Spike.PluginSandbox` only | none | spike result not turned into a design |
| ScpiInstruments | `OoBDev.Scpi` | IEEE 488.2, SCPI-99 | manuals per device under `docs/devices` |
| NetworkDevices, Ser2NetWin | `Applications/Ser2NetWin` | RFC 2217 | NetworkDevices has no project |
| StreamTools, WebFrontend | Wpf stream monitor; `DevTerm.Web` | none | Web parity with Wpf unverified |

## Transports and DeviceDecoding

- **Works with:** Presenters (decoded text or structure), UiDefinitions (device panels), the Docker test stack (RabbitMQ, Mosquitto, ser2net, loopback).
- **Known issues:** hardware tests are DevLocal and need the bench (see `docs/how-tos/test-hardware.md`); the transport list in the design predates the Brokers and Mqtt projects.
- **Standards:** RFC 2217 (Telnet COM port control), MQTT, STOMP, USBTMC, NMEA 0183.
- **Built 2026-11-16:** a capability matrix in `docs/design/Transports/transports.md`; reconnect defaults are the core `SessionLimits`.

## Presenters, SessionLogging and SessionChannel

- **Works with:** Transports (input), UiDefinitions (output), StreamTools (export and convert).
- **Known issues:** leaf designs are not marked done although the parity status says the slices are.
- **Built 2026-11-16:** the log header carries `version` (1 today; a newer one is rejected with a clear message), and `SessionLogCompatibilityTests` freezes a version 1 log: it must load and write back byte-identical.

## UiDefinitions, Theming and WindowsForms

- **Works with:** Plugins (panels), ViewModels (MVVM with real `ICommand` types, owner decision), GeneratedGuides (screenshots).
- **Known issues:** Avalonia has only a spike; WinForms has a control library but no renderer. The open layering question (renderers in Framework beside Wpf and Tui, or in Extensions) is queued in `OPEN-QUESTIONS.md`.
- **Standards:** WCAG 2.2 contrast ratios for themes.
- **Built 2026-11-16:** `XamlAccessibilityTests` (OoBDev.CodingStandards.Tests) requires `AutomationProperties.Name` or `LabeledBy` on every input control in the Applications XAML, as a per-file ratchet: 80 existing gaps are the baseline (five small windows already fixed), a new gap fails the test, a fixed one forces the baseline down.
- **Missing:** keyboard navigation (tab order, access keys) as testable requirements.

## Plugins and PluginSandbox

- **Works with:** UiDefinitions and Presenters (plugin kinds).
- **Known issues:** the sandbox is a spike; in-process .NET plugins cannot be isolated by `AssemblyLoadContext` alone, so the design must pick out-of-process hosting or accept trusted plugins.
- **Designed:** the manifest (version, publish and subscribe topics, permissions), package signing and the grant prompt are in [PluginSandbox](../PluginSandbox/protocol-and-manifest.md) and spiked (`PackageVerifier`, `GrantStore`); the production host waits for the owner sign-off tracked in `BACKLOG.md`.

## Instruments and networks

- **Works with:** Transports (VISA-like access over USBTMC and sockets), DeviceDecoding.
- **Known issues:** SCPI profiles depend on a researched manual per device (skill `device-manual`).
- **Standards:** IEEE 488.2, SCPI-99, VXI-11 (RFC 1057 based) and HiSLIP for LAN instruments.
- **Missing:** LXI discovery (mDNS, VXI-11 portmapper) and a NetworkDevices design with a project.
