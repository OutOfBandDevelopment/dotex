# Feature request: EcoWitt weather station decoder

- **Status:** triaged (HTTP push decoder built 2026-11-12; the local TCP API remains)
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `HardwareDevices`

## Need

EcoWitt weather stations push data over HTTP and a local TCP API; there is no decoder.

## Proposed behavior

A device decoder that turns EcoWitt payloads into typed readings, following the one-package-per-device rule (decisions D1 to D9 in the migration plan).

## Acceptance criteria

- Given a captured EcoWitt payload, then the decoder returns the expected readings.
- Given an unknown or truncated field, then it is reported and the rest still decodes.

## Existing code checked

`src/HardwareDevices` drivers (Busylight, DE-5000, K8055, Radex One, Zoom H4n); nothing for EcoWitt.

## Security and operations

Local network only; store no cloud credentials.

## Priority

- **Impact:** another supported device; needs a real capture to test.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Which transport (HTTP push, TCP, both)?

## Triage notes

Origin: BinaryDataDecoders issue 28. Low priority and specialized; accept, schedule or decline.

2026-11-11: triaged. Accepted at low priority: every BinaryDataDecoders feature is kept (scope rule in `CLAUDE.md`), so it is scheduled behind the DevTerm and event-sourcing work. Backlog: BACKLOG.md, Migrations (specialized formats).
