# Desired-State Database Tooling — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Draft · **Date:** 2026-10-29

## Contents

- [Goals and non-goals](#goals-and-non-goals)
- [Requirements](#requirements)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals and non-goals

Goals: scripts are the source of truth; the tool plans and applies the difference; free, MIT-compatible dependencies; one provider interface over the engines.

Non-goals (first version): data migrations, a GUI, cross-engine script translation.

## Requirements

<a id="table-1"></a>**Table 1 — Requirements**

| ID | Requirement | Priority |
|----|-------------|----------|
| REQ-001 | Given `CREATE` scripts and an empty database, the plan creates a matching schema for each supported engine | Must |
| REQ-002 | Given a changed script, the plan contains only the difference | Must |
| REQ-003 | Plan and apply are separate steps; unsafe changes (drop column or table, narrowing type, lock-heavy) are refused without an explicit flag | Must |
| REQ-004 | Ensure scripts are ordered by file name and idempotent: a second run changes nothing | Must |
| REQ-005 | A bake-off of sqldef, pgschema and Atlas is scored against the test plan and records a buy or build decision per engine with date and tool versions | Must |
| REQ-006 | Diff engines sit behind one provider interface so a tool can be replaced | Should |
| REQ-007 | An optional manifest (`queue: orders`, `extension: pgmq`) compiles to ensure scripts | Could |
| REQ-008 | Licences of every wrapped tool are verified and recorded before adoption | Must |
