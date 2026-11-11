# Feature request: GPIB (IEEE-488) transport for SCPI instruments

- **Status:** triaged
- **Requested by:** owner (notes repository project mapping, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Framework` (new transport, next to `OoBDev.System.IO.UsbTmc` and `OoBDev.System.IO.Tcp`) and `OoBDev.Scpi`

## Need

Older bench instruments (for example the HP 34401A and the Tektronix TDS2024 with its GPIB option) speak SCPI over GPIB. dotex reaches them over RS-232
today, and the SCPI design marks GPIB out of scope. Some instruments have GPIB as their only usable interface, and the notes backlog lists a GPIB USB
adapter as a project, so a host transport is the other half of it.

## Proposed behavior

A GPIB transport that presents an instrument at a GPIB address as an ordinary `IDeviceAdapter`, so the existing SCPI profiles run unchanged. First pass
through a controller that is itself a serial or network device (a USB-GPIB controller with a text command set, or a VXI-11 LAN-GPIB gateway). The address
is part of the connection path, for example `Gpib:<controller>/<address>`.

## Acceptance criteria

- Given a GPIB connection string, when opened, then the controller is set to the instrument address and talks to it.
- Given `*IDN?`, when sent, then the reply is returned without the terminator and the instrument profile is matched.
- Given a read with no instrument present, when it times out, then the adapter reports a timeout and does not hang (compare dev-term bug 042).
- Given two instruments on one controller, when each is addressed, then replies are not mixed.

## Existing code checked

- `docs/design/ScpiInstruments/scpi-instrument-control.md`: states GPIB paths are out of scope and no GPIB transport exists.
- `docs/design/Transports/lxi-support.md`: covers LXI and VXI-11 on TCP; mentions GPIB only in passing. A VXI-11 gateway could reuse it once VXI-11 is in
  Framework.
- `OoBDev.System.IO.UsbTmc`, `OoBDev.System.IO.Tcp`, `OoBDev.System.IO.Ports`: the transports it would sit beside; none speak GPIB.
- Not checked: which controller adapters have a published, stable command set. The notes project has no adapter chosen.

## Security and operations

A GPIB bus has no authentication; the host controls everything on it. Only the configured addresses should be reachable. Cost is a serial or network
connection per controller.

## Priority

- **Impact:** instruments with only a GPIB interface cannot be reached.
- **Needed by:** no date
- **Workaround today:** RS-232 where the instrument has it, or a vendor tool.

## Open questions

- Which controller class first: serial-attached USB-GPIB, VXI-11 gateway, or the owner's own adapter (a notes backlog project, no design yet)?
- Does it wait for VXI-11 in Framework, which the migration lists as open?
- Is GPIB service request (SRQ) handling in scope, or polling only?

## Triage notes

Origin: notes repository backlog (`projects/project-ideas.md`, Protocol Bridges & Interfaces) and `projects/applications/dev-term-device-terminal/scpi-instrument-control.md`.
Maintainers: accept, schedule or decline.

2026-11-11: triaged. Accepted as a backlog idea with no design yet; it needs hardware on the bench (see the test hardware map) before a design is written. Backlog: BACKLOG.md, Devices ideas.
