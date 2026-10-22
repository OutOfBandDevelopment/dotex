# HTTP QUERY Verb — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Draft · **Date:** 2026-10-22

## Contents

- [Goals](#goals)
- [Non-goals](#non-goals)
- [Requirements](#requirements)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals

1. A query too large for a URL travels in a body, with a safe and idempotent verb.
2. No endpoint changes: an `IQueryable<T>` action serves `GET`, `QUERY` and the fallback.
3. The same rights apply on every verb.

## Non-goals

- A new query language (OData and GraphQL are separate items).
- Replacing the custom search syntax.

## Requirements

<a id="table-1"></a>**Table 1 — Requirements**

| ID | Priority | Requirement |
|----|----------|-------------|
| REQ-001 | Must | A request with method `QUERY` is routed as `GET`; the original method is available as `IQueryVerbFeature.OriginalMethod`. |
| REQ-002 | Must | A `POST` with header `X-HTTP-Method-Override: QUERY` (case-insensitive) is treated as `QUERY`. A `POST` without it, and any other override value, is untouched. |
| REQ-003 | Must | The request body and content type are not altered, so the search model binder reads a JSON body as it does for `POST` today. |
| REQ-004 | Must | A `QUERY` request with no body or a non-JSON, non-form content type is rejected with a 415 problem details response. |
| REQ-005 | Must | The override applies only to `POST`; a `GET`, `PUT`, `PATCH` or `DELETE` carrying the header is never rewritten (no verb escalation). |
| REQ-006 | Should | (Planned, BACKLOG) Collection responses advertise `Accept-Query: "application/json"` so clients can discover support. |
| REQ-007 | Should | Registration is one extension, `UseHttpQueryVerb()`, called before routing; nothing happens unless it is added. |
