# Feature request: CSV driven test data utility

- **Status:** triaged
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.Devices.TestUtilities` or a shared test utilities project

## Need

Tests that need many input rows repeat arrays in code; there is no reusable CSV data source for MSTest.

## Proposed behavior

A test utility that reads CSV into typed objects or object arrays for data-driven tests, with column mapping.

## Acceptance criteria

- Given a CSV file and a row type, then each row becomes a test case with a readable name.
- Given a bad row, then the failure names the file and line.

## Existing code checked

`src/Framework/OoBDev.Devices.TestUtilities`, the `write-tests` skill and the `testing` rule. MSTest dynamic data may already cover part of this.

## Security and operations

Test data only; no secrets in files.

## Priority

- **Impact:** less boilerplate in data-heavy tests.
- **Needed by:** no date
- **Workaround today:** inline `DataRow` attributes

## Open questions

Compare with MSTest dynamic data first. Is a helper still needed?

## Triage notes

Origin: BinaryDataDecoders issue 7. Accept, schedule or decline.

2026-11-11: triaged. Accepted as tooling work through deterministic scripts or MSTest entry points ([GeneratedGuides](../../design/GeneratedGuides/README.md)); scheduled behind the active work. Backlog: BACKLOG.md, Tooling requests.
