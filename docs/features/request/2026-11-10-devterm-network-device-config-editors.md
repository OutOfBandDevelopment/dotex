# Feature request: DevTerm network device config editors and discovery probes

- **Status:** triaged
- **Requested by:** owner (retire dev-term, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm` (`Configuration/Discovery`, device manifests)

## Need

Network discovery has a Detect button and one row per device, but the EByte and USR UDP probes and the in-app config editors are missing. The Configure device button has an interface (`IDeviceConfigEditor`) and a fake, no real editor.

## Proposed behavior

- EByte E810-DTU (RS485) UDP discovery probe and config editor (read-only first).
- USR-TCP232-302 search packet and editor (the web-login USR probe exists).
- Decision of 2026-10-13: editors are manifests with a config UI where possible; a `DevTerm.Devices.*` project only when custom protocol code is needed.

## Source (read-only, to be retired)

dev-term `docs/design/proposals/network-device-discovery.md`, `network-device-config-editors.md`, `ebyte-e810-dtu-config-protocol.md`; dotex copies: [discovery](../../design/NetworkDevices/network-device-discovery.md), [editors](../../design/NetworkDevices/network-device-config-editors.md), [EByte protocol](../../protocols/ebyte-e810-dtu-config-protocol.md).

## Acceptance criteria

- Given a device that answers the probe, then Detect lists it with a badge and its address.
- Given an EByte unit, then Configure device shows its settings read-only first; writes need an explicit confirm.
- Given no captures yet, then unit tests use recorded packets stored with the protocol doc.

## Existing code checked

`Configuration/Discovery` (web-login USR probe), `IDeviceConfigEditor` and its fake. No EByte or USR UDP code.

## Security and operations

Broadcast probes reach the whole segment: show before sending and allow turning it off. Config writes can brick a device: read-only first, confirm, no blind writes.

## Priority

- **Impact:** setup of real network bridges from the app.
- **Needed by:** blocked on captures
- **Workaround today:** the vendors' own tools

## Open questions

Needs a fresh capture of each real device (the dev-term TODO said "continue to wait"). Depends on the [UDP transport](../completed/2026-11-10-devterm-udp-transport.md).

## Triage notes

Origin: dev-term BACKLOG (Proposals tracker, Proposed Ideas).

2026-11-11: triaged. Discovery and the `IDeviceConfigEditor` seam exist; the Ebyte editor is queued behind the first WPF device panel (owner, 2026-11-09). Design: [NetworkDevices](../../design/NetworkDevices/README.md). Backlog: BACKLOG.md, DevTerm.
