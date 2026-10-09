# OpenApiScalar — Requirements

[← Overview](README.md) · [Architecture →](architecture.md)

## Problem

Swashbuckle was the template default for years, needed fixes for each .NET release and cannot describe message-based surfaces. The owner asked for Scalar and an AsyncAPI viewer, and for the custom extensions to move to the newer model.

## Requirements

**Table 1 — Requirements**

| Id | Requirement |
|----|-------------|
| REQ-001 | Serve OpenAPI documents from the built-in generator; no Swashbuckle package remains |
| REQ-002 | Keep one document with every endpoint (`all`) and one document per controller assembly |
| REQ-003 | Show the application rights of each operation (`x-permissions`) and the `/health` endpoint |
| REQ-004 | Describe `IQueryable<T>` endpoints: sortable columns, paging and search parameters, `SearchQuery<T>` body for POST, `PagedQueryResult<T>` response |
| REQ-005 | Show XML documentation summaries, remarks and parameter descriptions |
| REQ-006 | Sign in from the API reference with OAuth2 (authorization code with PKCE) against the configured STS |
| REQ-007 | Title, version and description come from `IVersionProvider` or assembly information |
| REQ-008 | Each custom behaviour is a small class registered in DI, testable on its own |
| REQ-009 | (Phase 2) Publish an AsyncAPI document for the SQS, Service Bus and RabbitMQ channels and show it in a viewer |

## Non-goals

- Keeping the Swashbuckle API surface (owner decision 2026-10-09).
- Client code generation (NSwag stays a separate option).
- The `Filter` expression of `SearchQuery<T>` as query parameters (unchanged: still not described).

[← Overview](README.md) · [Architecture →](architecture.md)
