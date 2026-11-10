# Feature request: LANC frame builder and transport

- **Status:** new
- **Requested by:** owner (notes repository project mapping, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/HardwareDevices/OoBDev.LanC`

## Need

`OoBDev.LanC` holds the command and status tables only. Its README says there is no transport or frame builder, and no Hardware test because no LANC
interface or camera is attached. A program cannot send "zoom in" to a camera without composing the two command bytes by hand. The notes backlog has a LANC
camera controller expansion that needs this layer.

## Proposed behavior

A frame builder that turns a table entry (`LanCSubCommand` plus `LanCCameraVtrCommand` or `LanCCameraSpecialCommand`) into the bytes the author's LANC to
RS-485 bridge expects, a decoder for the status byte (`LanCStatusCode`), and a device adapter over the serial port at 57,600 baud. The existing
`LanCPanel` then sends through it instead of raw bytes.

## Acceptance criteria

- Given a camera zoom command, when built, then the two bytes match the published table entry.
- Given a status byte from the bridge, when decoded, then it maps to the right `LanCStatusCode` or reports unknown.
- Given the serial adapter and a recorded exchange, when replayed, then the panel shows the expected state (no camera needed).
- Given a Hardware-category test and a bridge plus camera, when run, then a transport command is acknowledged (skipped when no bridge is present).

## Existing code checked

- `src/HardwareDevices/OoBDev.LanC`: tables, a panel and tests; the README says the bridge protocol is not implemented as a transport.
- `OoBDev.System.IO.Ports`: the serial adapter it would use.
- Not checked: the bridge firmware in the EmbeddedBakery repository, to confirm the byte format on the serial side.

## Security and operations

None beyond opening a local serial port. The command set is camera control only.

## Priority

- **Impact:** the LANC package is not usable on its own.
- **Needed by:** no date
- **Workaround today:** none in dotex.

## Open questions

- Does the serial format of the bridge need a spec first (bench or firmware read)?
- Is a recorded-exchange test enough for acceptance until a camera is attached?

## Triage notes

Origin: notes repository backlog (`projects/project-ideas.md`, LANC camera controller) and `OoBDev.LanC` README. Maintainers: accept, schedule or decline.
