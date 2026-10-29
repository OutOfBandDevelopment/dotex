# Sandboxed Plugin Host — Testing strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft · **Date:** 2026-10-29

## Approach

Grant checking, manifest parsing, signature verification and limit arithmetic are Unit tests (85% target). Launching, limits and escape tests start real processes: `Integration` on Windows and in a Linux container; they never run in the Unit category. Each escape attempt (read a file, open a socket, spawn a child, exceed memory, fork bomb, ptrace) is a named test with the expected refusal.

### List of Tables

1. [Table 3 — Requirement coverage](#table-3)

<a id="table-3"></a>**Table 3 — Requirement coverage**

| Requirement | Category | Test idea |
|-------------|----------|-----------|
| REQ-001, REQ-008 | Integration | escape suite on both platforms, none succeeds |
| REQ-002, REQ-003, REQ-004 | Unit | grant store and broker allow/deny with log entries |
| REQ-005 | Integration | memory hog, CPU spinner and sleeper stopped with the right reason |
| REQ-006 | Unit | tampered and unsigned packages refused |
| REQ-007 | Integration | one granted host reachable, another refused |
| REQ-009 | Unit | protocol version mismatch rejected |
| REQ-010 | Review | [threat model](threat-model.md) exists and is signed off |
