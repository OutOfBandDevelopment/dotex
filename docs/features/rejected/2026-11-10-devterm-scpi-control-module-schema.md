# Feature request: DevTerm declarative SCPI control-module schema

- **Status:** declined (owner, 2026-11-19, Q21: covered by the JSON profile panels)
- **Requested by:** owner (retire dev-term, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm`, `src/HardwareDevices` (SCPI)

## Need

Device control modules are code. A declarative schema (send template plus response pattern, an SCPI baseline for common bench-instrument commands) would let a new instrument be added without code.

## Proposed behavior

- The schema and loader for control modules.
- Decision 2026-10-13: SCPI profiles stay separate from `DeviceManifest`, layered as a generic SCPI profile plus device-specific sub-types.

## Source (read-only, to be retired)

dev-term `docs/design/device-control-modules.md`, `docs/design/features/format-schema-files.md`; dotex copies: [SCPI instruments](../../design/ScpiInstruments/scpi-instrument-control.md), [device decoding](../../design/DeviceDecoding/README.md).

## Acceptance criteria

- Given a profile file for an instrument, then DevTerm lists its commands and parses replies.
- Given a device sub-type, then it overrides the generic profile for that command only.

## Existing code checked

SCPI device packages load from `plugins/`. No declarative schema in `src`.

## Security and operations

Profiles are data: validate before use; a command template must not send unvetted input.

## Priority

- **Impact:** new instruments without code.
- **Needed by:** no date
- **Workaround today:** a device package per instrument

## Open questions

Does the manifest model (JSON panels) cover enough that a second schema is not needed?

## Triage notes

Origin: dev-term BACKLOG (Device control modules; UX decisions).

2026-11-11: triaged. Declarative SCPI profiles exist (`OoBDev.Scpi` JSON profiles, drop-in directories, layered `Extends`, generated `Panels`); the note above predates them. What remains is the control-module schema beyond profiles. Design: [ScpiInstruments](../../design/ScpiInstruments/README.md). Backlog: BACKLOG.md, DevTerm.
