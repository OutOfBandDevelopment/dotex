# USB serial devices and Linux containers

Docker Desktop on Windows cannot pass a USB device into a Linux container. For every device that appears as a Windows COM port, `ser2net-win` is the supported answer: it bridges the port to TCP and the container connects to the host. This replaces the `usbipd-win` and WSL route for serial-class devices.

## Contents

- [Scope](#scope)
- [Select the port by USB id](#select-the-port-by-usb-id)
- [Container recipe](#container-recipe)
- [Security](#security)

## Scope

**Table 1 — What the bridge covers**

| Device | Covered | Route |
| --- | --- | --- |
| USB serial adapters (FTDI, Prolific, CH340), the Aeotec Z-Stick, anything with a Windows COM port | Yes | `ser2net-win` |
| Devices with a vendor USB protocol and no serial interface | Built: a framed USBTMC and HID bridge, not USB/IP (owner, 2026-11-24): add a `UsbBridges` entry (`Kind` `UsbTmc` or `Hid`, `UsbVendorId`, `UsbProductId`, `TcpPort`); one port per device; clients are `RemoteUsbTmcDevice` and `RemoteUsbHidDevice` ([design](../../design/Ser2NetWin/usb-bridge.md)) |

## Select the port by USB id

A COM number changes when a device is unplugged and plugged into another socket. A bridge can name the device instead; the port is looked up each time the bridge opens it, so a replug reconnects without a restart.

```json
{
  "UsbVendorId": "067B",
  "UsbProductId": "2303",
  "UsbSerialNumber": "optional, only when two devices share the ids",
  "TcpPort": 4000
}
```

- `ComPort` is ignored when both ids are set, and may be left out.
- Two present devices with the same ids and no `UsbSerialNumber`: the bridge does not open either, and its log names both ports.
- `ser2net-win ports` lists each present port with its ids, for example `COM13 067B:2303` or `COM21 0658:0200 serial ABC123`. A port with no ids is not a USB device, or Windows has no record of its ids.
- Ids are read from `HKLM\SYSTEM\CurrentControlSet\Enum\USB`; only ports that are present now count.

## Container recipe

Bind the bridge to the Docker host interface, then present it as a device node inside the container. The first form needs `socat` in the image and `--privileged` or a pseudo-terminal in a path the program can open.

```bash
socat pty,link=/dev/ttyUSB0,raw,echo=0 tcp:host.docker.internal:4000 &
```

A program that accepts `tcp://host:port` directly (pyserial `serial_for_url`, many Z-Wave and Modbus libraries) needs no `socat`: use `tcp://host.docker.internal:4000`. An existing compose file that expects `/dev/ttyUSB0` keeps working with the `socat` line as the container entry point.

For RFC 2217 clients, set `Protocol` to `Rfc2217` on the bridge so the client can change baud and line settings; with `Raw` the bridge keeps the settings from its configuration.

Not bench-tested here: the byte exchange through `socat` with a real device (a Hardware-category check needs the device attached; tracked in owner question Q33).

## Security

The bridge listens on a TCP port with no authentication. Bind it to the Docker host interface (`BindAddress`), not the LAN. A raw device behind a TCP hop adds latency; tools with strict timing may not tolerate it.
