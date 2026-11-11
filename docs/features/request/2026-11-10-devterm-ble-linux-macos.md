# Feature request: DevTerm BLE backends for Linux and macOS

- **Status:** triaged
- **Requested by:** owner (retire dev-term, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm` (BLE transport adapters)

## Need

The BLE serial transport works on Windows only (verified on real hardware). The adapter seam allows other platforms independently.

## Proposed behavior

- A Linux adapter over BlueZ (D-Bus) and a macOS adapter over CoreBluetooth, each behind the existing adapter interface and selected at run time.

## Source (read-only, to be retired)

dev-term `docs/design/transports.md` (BLE section) and `docs/changes/2026-09-25.md`, `2026-09-29.md`; dotex copies under [transports](../../design/Transports/transports.md) and the imported change history.

## Acceptance criteria

- Given a BLE device on Linux or macOS, then connect, send and receive behave as on Windows.
- Given a platform without an adapter, then the transport reports it clearly.

## Existing code checked

Windows adapter and the seam in DevTerm; no BlueZ or CoreBluetooth code.

## Security and operations

Pairing and permissions are the OS's; do not store keys.

## Priority

- **Impact:** portability.
- **Needed by:** blocked: no Linux or macOS test machine
- **Workaround today:** use Windows

## Open questions

Which platform first, and is a machine available for it?

## Triage notes

Origin: dev-term BACKLOG (Transports); dev-term TODO "continue to wait".

2026-11-11: triaged. Windows adapter and seam only; BlueZ and CoreBluetooth wait for hardware to test (macOS deferred, owner 2026-11-10). Design: [Transports](../../design/Transports/README.md). Backlog: BACKLOG.md, DevTerm.
