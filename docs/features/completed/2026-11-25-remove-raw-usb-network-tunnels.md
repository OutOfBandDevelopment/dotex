# Feature request: remove the raw USB HID and USBTMC network bridge

- **Status:** done (2026-11-25, [change](../../changes/2026-11-25.md#raw-usb-network-bridge-removed))
- **Requested by:** owner
- **Date:** 2026-11-25
- **Area:** `OoBDev.System.IO.Usb.Bridge`, `OoBDev.Ser2NetWin.Cli` (`UsbBridges`), `docs/design/Ser2NetWin/usb-bridge.md`

## Need

The USB bridge carries HID and USBTMC calls over a plain framed TCP port with no authentication. Anyone who reaches the port can drive the instrument or the HID device. The authenticated route is the remote DevTerm web host (owner, 2026-11-25), so the raw tunnel is a second, weaker way to do the same thing.

## Proposed behavior

Remove `OoBDev.System.IO.Usb.Bridge` and its tests, the `UsbBridges` configuration and host wiring in the Ser2NetWin CLI, and the USB bridge documents. The serial bridge (`ser2net-win`, USB-id selection, container recipe) stays. Devices reach DevTerm on another machine through the web host instead (see the DevTerm remote host request in the backlog).

## Acceptance criteria

- Given the solution, when it builds, then no project references `OoBDev.System.IO.Usb.Bridge`.
- Given a Ser2NetWin configuration with `UsbBridges`, when the host starts, then it fails with a message pointing to the remote DevTerm host.
- Given the docs, when searched, then the USB bridge appears only as a removed feature in the change document.

## Existing code checked

- The bridge was built 2026-11-24 (server, clients, CLI config) and bench-tested on the DG1022 and the Busylight; the DM3058E bulk-IN `Pipe` error is [an issue](../../issues/usbtmc-dm3058e-bulk-in-pipe-error.md) that also fails without the bridge and stays.
- Remote DevTerm web host: WebSocket tunnel, session and panel APIs, shared bearer token; no JWT yet.

## Security and operations

Removal closes an unauthenticated device-control port. Nothing else depends on it.

## Priority

- **Impact:** removes an unauthenticated control surface.
- **Needed by:** after the remote DevTerm host can reach HID and USBTMC devices with JWT authentication.
- **Workaround today:** bind the bridge to the Docker host interface only.

## Open questions

None. Owner answer to Q41, 2026-11-25: use the web host, and remove the raw tunnels.

## Triage notes

2026-11-25: triaged. Order: remote DevTerm host with JWT exchange and a Tui and WPF remote client first, then this removal (the usb-bridge design was deleted with the code). Backlog: Devices.
