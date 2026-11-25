# USB bridge: USBTMC and HID over a framed TCP stream

**Status:** built, superseded (owner, 2026-11-25: the remote DevTerm web host replaces it; removal is [a request](../../features/request/2026-11-25-remove-raw-usb-network-tunnels.md)); built (project `OoBDev.System.IO.Usb.Bridge`, server, clients and CLI config; DevTerm connection options still open, see the backlog). Owner decision: a framing bridge with our own protocol; USB/IP is not wanted ("the usbip from Microsoft is absolute trash"). This reverses the scope part of owner Q16 (non-serial USB out of scope), at the owner's request of 2026-11-24.
**Request:** [Ser2NetWin USB device passthrough](../../features/request/2026-11-10-ser2netwin-usb-device-passthrough.md)

## Contents

- [Goal](#goal)
- [Approach](#approach)
- [Frame format](#frame-format)
- [Bridge configuration](#bridge-configuration)
- [Client](#client)
- [Security](#security)
- [Migration plan (needs owner confirmation)](#migration-plan-needs-owner-confirmation)
- [Verification](#verification)
- [Open points](#open-points)

**Tables:** [Table 1 — Operations](#frame-format), [Table 2 — Projects](#migration-plan-needs-owner-confirmation)

## Goal

`ser2net-win` shares a USBTMC instrument or a HID device on a TCP port, and DevTerm (or any dotex code) on another machine or in a Linux container uses it as if it were local. Test devices on the bench: Rigol DM3058E (`1AB1:09C4`) and DG1022 (`1AB1:0588`) for USBTMC, the Busylight (`04D8:F848`) for HID.

## Approach

Forward at the existing device interfaces, not at the USB wire. `IUsbTmcDevice` already exposes whole framed bulk transfers (`WriteBulkOut`, `ReadBulkIn`, `AbortBulkIn`, `AbortBulkOut`, `Clear`, `SetRemote`) and `IUsbHidDevice` exposes a stream plus `Write`. A frame protocol carries those calls. The client side is a `RemoteUsbTmcDevice : IUsbTmcDevice` and a `RemoteUsbHidDevice : IUsbHidDevice`, so the existing `UsbTmcDeviceAdapter` (header codec, quirks, retries) and `UsbHidEndpointDeviceAdapter` run unchanged on the client and DevTerm needs only a new connection option.

**What this does not do:** it does not present a real USB device to the client (no `lsusb`, no vendor tool, no isochronous transfer). Only dotex clients talk to it. That is the accepted trade-off against USB/IP.

## Frame format

All integers big-endian. One frame: `u32 length` (bytes after this field) | `u8 op` | `u32 id` | payload.

**Table 1 — Operations**

| Op | Direction | Payload | Reply |
|----|-----------|---------|-------|
| `Hello` 0x01 | client | `u8 version`, `u8 kind` (1 USBTMC, 2 HID) | `Hello` with `u8 version`, `u8 kind` and a UTF-8 device description |
| `Open` 0x02 | client | none | `Ok`, or `Error` |
| `Close` 0x03 | client | none | `Ok` |
| `WriteBulkOut` 0x10 | client | transfer bytes | `Ok`, or `Error` (timeout) |
| `ReadBulkIn` 0x11 | client | `u32 bufferSize` | `Data`: `u8 stalled`, bytes; or `Error` (timeout) |
| `AbortBulkIn` 0x12 / `AbortBulkOut` 0x13 | client | `u8 bTag` | `Ok` |
| `Clear` 0x14 | client | none | `Ok` |
| `SetRemote` 0x15 | client | `u8 remote` | `Ok` |
| `HidWrite` 0x20 | client | report bytes | `Ok`, or `Error` |
| `HidReport` 0x21 | server, unsolicited | input report bytes | none (pumped from the device stream) |
| `Ok` 0x80, `Data` 0x81, `Error` 0x82 | server | `Error`: `u8 code` (1 timeout, 2 device gone, 3 busy, 4 bad request), text | |

A reply carries the `id` of its request. A request is answered before the next one is read (one in flight, like the USB device itself); `HidReport` frames may interleave. A frame over 16 MiB is a protocol error and closes the connection. Property reads (`MaxTransferSize`, `MaxPacketSize`) are returned in the USBTMC `Open` reply (`u32` each), so the client never asks per call.

## Bridge configuration

A USB bridge is a `UsbBridgeOptions` entry in the `UsbBridges` array of the same config file (`Kind` `UsbTmc` or `Hid`), separate from `Bridges`, so serial config is untouched. It uses `UsbVendorId`, `UsbProductId` and `UsbSerialNumber` for selection (and `TcpPort`, `BindAddress`, `Name`, `ReplaceExistingClient`, mDNS); the serial fields are ignored. The device is resolved on each client `Open`, so a replug reconnects without a restart, as for serial. Two matches with no serial number: refuse and log both, as for serial. The CLI `ports` command lists USBTMC and HID devices with their ids beside the COM ports.

```json
"UsbBridges": [ { "Kind": "UsbTmc", "UsbVendorId": "1AB1", "UsbProductId": "09C4", "TcpPort": 4100 } ]
```

## Client

DevTerm gets `--transport usbtmc-tcp|hid-tcp` style connection options naming `host:port`; the connection editor and manifests follow the existing UsbTmc and HID options (see `UsbtmcDeviceOption`, `HidDeviceOption`). One client per bridge unless `ReplaceExistingClient`.

## Security

Same as serial: no authentication, bind to the Docker host interface. A shared instrument can be reset or have its calibration written by anyone who can connect. Documented in the Ser2NetWin guide.

## Migration plan (needs owner confirmation)

Layout confirmed by the owner on 2026-11-24: a new Framework project (option A).

**Table 2 — Projects**

| Project | Current | Target | Namespace | Unit | References |
|---------|---------|--------|-----------|------|------------|
| `OoBDev.System.IO.Usb.Bridge` (new) | none | `src/Framework` | `OoBDev.System.IO.Usb.Bridge` | NuGet package, used by Ser2NetWin | `OoBDev.System.IO.UsbTmc`, `OoBDev.System.IO.UsbHids` |
| `OoBDev.System.IO.Usb.Bridge.Tests` (new) | none | `src/Framework` | same | test project | the project above |
| `OoBDev.System.IO.Ports.Bridge` | `src/Framework` | unchanged | unchanged | unchanged | none (USB bridges have their own options and config array) |
| `OoBDev.Ser2NetWin.Cli` | `src/Applications/Ser2NetWin` | unchanged | unchanged | unchanged | adds the new project |
| `OoBDev.DevTerm.Core` | `src/Applications/DevTerm` | unchanged | unchanged | unchanged | adds the new project (remote device clients) |

## Verification

Unit tests: frame codec round trips, bad frames, server against fake `IUsbTmcDevice` and `IUsbHidDevice`, client against a fake server. Hardware tests (category `Hardware`): `*IDN?` through the bridge to the DM3058E and DG1022, and a Busylight colour write through the HID bridge.

## Open points

- **Decided 2026-11-24 (owner): one TCP port per device for now.** A shared port is later work: `Hello` carries a device selector, a `List` op returns the attached devices, one connection serves one device, and an optional allow-list limits what is exposed (`Transport: "UsbHub"`). Serial bridges stay one port per COM port.
- **Decided 2026-11-25 (owner, Q41):** DevTerm reaches remote HID and USBTMC devices through the remote DevTerm web host with a client in the Tui and WPF apps; the shared key is exchanged for a JWT; this bridge is to be removed.
- Whether a second client may watch HID reports read-only (default: no, one client).
- A `u32` timeout field per `ReadBulkIn` (default: the server's device timeout).
