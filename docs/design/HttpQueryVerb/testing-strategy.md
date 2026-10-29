# HTTP QUERY Verb — Testing Strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft · **Date:** 2026-10-22

## Contents

- [Targets](#targets)
- [Requirement coverage](#requirement-coverage)

### List of Tables

1. [Table 3 — Requirement coverage](#table-3)

## Targets

85% or more for the new types. Unit tests drive the middleware with `DefaultHttpContext` and a recording `next` delegate; no host is needed.

## Requirement coverage

<a id="table-3"></a>**Table 3 — Requirement coverage**

| Requirement | Test | Category |
|-------------|------|----------|
| REQ-001 | `QUERY` reaches `next` as `GET` with the feature set | Unit |
| REQ-002 | `POST` with the override (any case) becomes `GET`; other values stay `POST` | Unit |
| REQ-003 | Body and content type are the same after the rewrite | Unit |
| REQ-004 | `QUERY` without JSON or form content returns 415 and does not call `next` | Unit |
| REQ-005 | `PUT` with the header is untouched | Unit |
| REQ-006 | `OnResultExecuting_SearchAction_AddsAcceptQueryHeader` | Unit |
| REQ-007 | `UseHttpQueryVerb_InPipeline_RoutesQueryAsGetAsync` builds a pipeline with the extension | Unit |
