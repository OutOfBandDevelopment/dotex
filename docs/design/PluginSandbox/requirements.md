# Sandboxed Plugin Host — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Draft · **Date:** 2026-10-29

## Contents

- [Goals and non-goals](#goals-and-non-goals)
- [Requirements](#requirements)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals and non-goals

Goals: deny by default; every external effect goes through a broker that checks an approved grant; a plugin that breaks a limit is stopped and the reason reported.

Non-goals (first version): macOS isolation, in-process plugins, GPU access, a plugin marketplace.

## Requirements

<a id="table-1"></a>**Table 1 — Requirements**

| ID | Requirement | Priority |
|----|-------------|----------|
| REQ-001 | A plugin runs in a separate process with no ambient access to files, network or environment | Must |
| REQ-002 | A package manifest lists requested permissions; the user approves a subset and the grant is stored and auditable | Must |
| REQ-003 | A call to a resource outside the grant is refused and logged | Must |
| REQ-004 | A call inside the grant succeeds through the broker | Must |
| REQ-005 | CPU, memory, process count and wall-time limits are enforced; the host stops the plugin and reports which limit | Must |
| REQ-006 | A package with a missing or invalid signature is refused | Must |
| REQ-007 | Network access is off by default and, when granted, limited to named hosts | Must |
| REQ-008 | The PoC escape tests pass on Windows and Linux | Must |
| REQ-009 | The wire protocol is language-neutral and versioned | Should |
| REQ-010 | A threat model is written and reviewed before implementation | Must |
