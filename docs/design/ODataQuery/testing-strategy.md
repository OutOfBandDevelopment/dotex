# OData Query — Testing Strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft · **Date:** 2026-10-29

## Contents

- [Targets](#targets)
- [Requirement coverage](#requirement-coverage)

### List of Tables

1. [Table 3 — Requirement coverage](#table-3)

## Targets

85% or more for the new types. Middleware tests use `DefaultHttpContext`; query tests run a real in-process host (`WebApplication` on a loopback port) with an in-memory `IQueryable<T>` action (Simulate).

## Requirement coverage

<a id="table-3"></a>**Table 3 — Requirement coverage**

| Requirement | Test | Category |
|-------------|------|----------|
| REQ-001 | Filter, orderby, top, skip, select and count each change the result | Simulate |
| REQ-002 | `$top` over the cap is 400; no `$top` returns the default page | Simulate |
| REQ-003 | An over-complex `$filter` is 400; `$expand` is rejected unless allowed | Simulate |
| REQ-004 | `$apply` is rejected | Simulate |
| REQ-005 | A `[NotFilterable]` property in `$filter` is 400 | Simulate |
| REQ-006 | A bad `$filter` is a problem details 400 | Simulate |
| REQ-007 | Form body fields reach the query string; a JSON body is untouched | Unit |
| REQ-008 | A non-`$` field is ignored; the original query string wins | Unit |
| REQ-009 | Invalid limits fail validation; the defaults bind | Unit |
| REQ-010 | The response carries `Accept-Query` | Simulate |
