# SQL Server Service Broker — Testing strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft · **Date:** 2026-10-29

## Approach

Unit tests cover configuration validation, retry counting and registration (target 85% line coverage of non-I/O code). Integration tests run against the SQL Server container of the test stack and are marked `Integration`; the test creates a scratch database, enables the broker, creates the objects with the on-demand script and drops everything afterward.

### List of Tables

1. [Table 3 — Requirement coverage](#table-3)

<a id="table-3"></a>**Table 3 — Requirement coverage**

| Requirement | Test category | Test idea |
|-------------|---------------|-----------|
| REQ-001, REQ-003 | Integration | send one message, receive it once, queue empty afterward |
| REQ-002 | Integration | cancel the receiver; `RunAsync` ends without exception |
| REQ-004, REQ-008 | Unit | keyed resolution and AsyncAPI contributor registered |
| REQ-005 | Integration | handler throws twice then succeeds; handler always throws, message ended with error and queue stays enabled |
| REQ-006 | Integration | broker disabled database gives the configuration exception |
| REQ-007 | Integration | on-demand script is idempotent (run twice) |
| REQ-009 | — | category audit |
