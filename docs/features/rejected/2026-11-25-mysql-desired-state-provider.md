# Feature request: MySQL in the desired-state database tooling

- **Status:** declined (2026-11-25)
- **Requested by:** owner
- **Date:** 2026-11-25
- **Area:** `OoBDev.Data.DesiredState`, `OoBDev.DesiredState.Cli`

## Need

sqldef already ships `mysqldef`, so the desired-state wrapper could cover MySQL and MariaDB as it covers PostgreSQL, SQL Server and SQLite. No store or application needs it today.

## Proposed behavior

`SqldefProvider.ForMySql`, a `mysql` engine in the factory and in `desired-state plan|apply|deploy --engine mysql`, with gates and the package history table working as on the other engines.

## Acceptance criteria

- Given a MySQL container and a desired schema, when `plan` runs on an empty database and `apply` runs, then a second `plan` has 0 statements.
- Given a narrowing column change or a drop, when `apply` runs without the gate flag, then it is refused.
- Given a package with a refactor entry, when `deploy` runs twice, then the second run changes nothing.

## Existing code checked

- `SqldefProvider` is generic over engine name, executable and target; adding an engine is a constant, a factory method and an argument check.
- `PlanAnalyzer` parses sqldef output for hazards; MySQL syntax (backticks, `MODIFY COLUMN`) needs its own examples and tests.
- `mysqldef` is not installed on the owner's machine and was not run, so sqldef's MySQL behaviour is unverified.
- No MySQL service in the integration stack; no MySQL driver or Pomelo EF provider in `src/Directory.Packages.props`.

## Security and operations

A MySQL container in the integration stack and one more driver package (`MySqlConnector`). The EF side would depend on Pomelo supporting .NET 10 (not checked). MySQL has no schemas, so store instances would need a database or prefix per instance.

## Priority

- **Impact:** none today; no consumer.
- **Needed by:** no date
- **Workaround today:** none needed; the other three engines cover every store.

## Open questions

- Which store or application would use MySQL first?
- MariaDB as well? (`mysqldef` covers both.)

## Triage notes

2026-11-25: declined for now (owner), no consumer. Estimate: one to two days for the desired-state side (provider, hazard analysis, CLI, container and tests); the EF Core side is larger. Revisit when an application needs MySQL or MariaDB; reopen by moving this file back to `request/` with status `triaged`. Related: [store instances](../../design/DesiredStateDatabase/instances.md).
