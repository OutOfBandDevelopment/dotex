# Application Rights — Testing Strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft · **Date:** 2026-10-22 · **Related:** [requirements](requirements.md)

## Contents

- [Targets](#targets)
- [Requirement coverage](#requirement-coverage)

### List of Tables

1. [Table 4 — Requirement coverage](#table-4)

## Targets

Coverage 85% or more for the new types. All tests are `Unit` in `OoBDev.AspNetCore.Tests`: no host or container is needed, because the mapper, transformation and checker take plain objects.

## Requirement coverage

<a id="table-4"></a>**Table 4 — Requirement coverage**

| Requirement | Test | Category |
|-------------|------|----------|
| REQ-001, REQ-002 | Mapper returns the union of rights for several roles; role names are case-insensitive | Unit |
| REQ-003 | Transformation adds claims; running twice adds nothing; unauthenticated principal unchanged | Unit |
| REQ-004 | Roles read from each configured claim type; a custom type list is honored | Unit |
| REQ-005 | Existing right claims survive and are not duplicated | Unit |
| REQ-006 | Checker: unauthenticated, forbidden, allowed, empty list needs only authentication | Unit |
| REQ-007 | Unmapped role grants nothing | Unit |
| REQ-008 | Filter: 401 challenge, 403 forbid, allowed | Unit |
| REQ-009 | CGI 401/403 tests stay green on the checker; `Get_RegisteredChecker_DecidesTheOutcome` shows a registered checker decides | Simulate |
| REQ-010 | Debug log of rights granted (`Transform_GrantedRights_LogsCountsAtDebugAsync`); options validation rejects a blank role or right; registration keeps an existing mapper | Unit |
| REQ-011 | A previously registered `IRoleRightMapper` wins over the default | Unit |
