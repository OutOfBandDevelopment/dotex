# Feature request: Ser2NetWin as the way to reach a Windows USB device from a Linux container

- **Status:** triaged
- **Requested by:** owner (notes repository, `projects/windows-usb-passthough`, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/Ser2NetWin`, `OoBDev.System.IO.Ports.Bridge`, `docs/applications/ser2netwin`

## Need

Docker Desktop on Windows cannot pass a USB device into a Linux container. The notes repository keeps scratch notes for the other route: `usbipd-win` shares
the device into WSL (`usbipd wsl list`, `usbipd wsl attach --busid <id>`), and the compose file maps `/dev/ttyUSB0:/dev/ttyUSB0`. That route needs WSL, a manual
attach after every unplug, and a compose device mapping. USB serial adapters, the Aeotec Z-Stick and a Greaseweazle floppy controller (per the notes page, not
checked here) are the devices it was used for. Ser2NetWin already solves the Z-Stick case with a TCP bridge; the notes ask that it be the supported answer
for this whole class, so the scratch notes can be retired.

## Proposed behavior

1. **Container side recipe.** The Ser2NetWin docs describe, and ideally test, how a container presents the bridged port as a device node (for example
   `socat pty,link=/dev/ttyUSB0,raw tcp:host.docker.internal:4000`, or a client that takes `tcp://host:port` directly), so existing compose files that expect
   `/dev/ttyUSB0` keep working without `usbipd-win` or WSL.
2. **Find by device, not by COM number.** A bridge entry may select its serial port by USB vendor and product id (and serial number) instead of `COM13`, so an
   unplug and replug, or a renumbered port, reconnects by itself. `ser2net-win ports` lists the ids.
3. **Scope statement.** The docs say which devices this covers (anything that appears as a Windows COM port) and which it does not (devices with a vendor USB
   protocol and no serial interface), pointing those to `usbipd-win`.

## Acceptance criteria

- Given a bridge configured by vendor and product id, when the device is unplugged and plugged back in on another COM number, then the bridge reopens it without a restart.
- Given two devices with the same ids, when no serial number is configured, then the bridge refuses to start and the log names both.
- Given `ser2net-win ports`, when run, then each row shows the COM name, vendor id, product id and serial number where Windows reports them.
- Given the container recipe in the docs and a bridged port, when a Linux container follows it, then a program opening the device node can exchange bytes with the device (a Hardware-category test or a documented bench report).
- Given a Greaseweazle or a USB serial adapter, when bridged, then it works for the read and write commands the owner uses, or the docs list what does not work.

## Existing code checked

- `docs/applications/ser2netwin` and `docs/design/Ser2NetWin`: bridge by COM name only; one client per port; RFC 2217 and mDNS exist. No selection by USB id and no container recipe.
- `OoBDev.System.IO.UsbHids`, `OoBDev.System.IO.UsbTmc`: other USB transports that already read vendor and product ids; they could share the lookup (not checked).
- The notes page `projects/windows-usb-passthough` is scratch commands only. Not checked here: whether a Greaseweazle enumerates as a COM port on Windows (probably, not
  verified), and whether Greaseweazle tools run over a `socat` pseudo-terminal at all (its timing may be sensitive to the network hop).

## Security and operations

The bridge exposes a device on a TCP port with no authentication, so the docs should say to bind it to the Docker host interface and not to the LAN. Cost is one
listener per bridge. A raw device behind a TCP hop adds latency; some tools may not tolerate it.

## Priority

- **Impact:** removes the WSL and `usbipd-win` dependency for serial-class devices; retires a set of manual attach commands.
- **Needed by:** no date
- **Workaround today:** `usbipd-win` with WSL2, attaching by hand.

## Open questions

- Is selection by USB id wanted in the bridge configuration, or only in the `ports` listing?
- Does the container recipe live in the Ser2NetWin docs only, or does dotex ship a small helper image (the `containers` repository is a candidate)?
- Should non-serial USB (vendor protocols) be declared permanently out of scope, or is a USB/IP bridge ever wanted here?
- Does the Greaseweazle case need its own bench pass before it is claimed in the docs?

## Triage notes

Origin: notes repository `projects/windows-usb-passthough` and `projects/ser2net-win`. Maintainers: accept, schedule or decline.

2026-11-11: triaged. Accepted as a backlog idea with no design yet; it needs hardware on the bench (see the test hardware map) before a design is written. Backlog: BACKLOG.md, Devices ideas.
