# Feature request: DevTerm BYTECC BT-UP01 USB-over-network bridge (research)

- **Status:** declined (owner, 2026-11-19, Q17: no hardware)
- **Requested by:** owner (retire dev-term, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm`, research

## Need

Reach USB devices through the BYTECC BT-UP01 network bridge. Low priority by choice (2026-09-15): the plan is to reverse-engineer it only once a real capture exists, not to use a Raspberry Pi with `usbip`.

## Proposed behavior

Two cheap checks first: does the vendor client make the remote USB device appear local (then DevTerm needs nothing), and does the box already speak USB/IP. Only if both fail is it a real project (tunneling USB: enumeration, control, bulk and interrupt transfers).

## Source (read-only, to be retired)

dev-term `docs/design/proposals/bytecc-bt-up01-usb-network-bridge.md`; dotex: [protocol notes](../../protocols/bytecc-bt-up01-usb-network-bridge.md).

## Acceptance criteria

- The two checks are answered and recorded; the request is then declined or re-scoped.

## Existing code checked

No BYTECC code in `src`.

## Security and operations

Capturing and decompiling vendor software: confirm the licence allows it.

## Priority

- **Impact:** low.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Is the hardware still owned? Without it, decline.

## Triage notes

Origin: dev-term BACKLOG (Research). Probably a decline.

2026-11-11: triaged. Research only; no BYTECC code. Kept as a research note in the DevTerm backlog and declined as a build item until a device is on the bench. Backlog: BACKLOG.md, DevTerm.
