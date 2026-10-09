# OpenApiScalar — OpenAPI Transformers, Scalar Reference and AsyncAPI

**Status:** Phase 1 (OpenAPI and Scalar) implemented 2026-10-09 ([change](../../changes/migration-openapi-scalar-2026-10-09.md)) · Phase 2 (AsyncAPI document and viewer) designed, not started · Epic: API documentation

Swashbuckle is removed. Every custom filter and option is now a `Microsoft.AspNetCore.OpenApi` document, operation or schema transformer, and [Scalar](https://github.com/scalar/scalar) is the API reference. Phase 2 describes the message surfaces (SQS, Service Bus, RabbitMQ) with an AsyncAPI document and a viewer.

## Documents

**Table 1 — Document set**

| Document | Purpose |
|----------|---------|
| [requirements.md](requirements.md) | Problem, requirements, non-goals |
| [architecture.md](architecture.md) | Documents, transformers, registration flow, AsyncAPI plan |
| [api-design.md](api-design.md) | Public types, registration calls, configuration |
| [testing-strategy.md](testing-strategy.md) | Simulate tests and the live checks |

## Decisions in one view

**Table 2 — Summary**

| Topic | Decision |
|-------|----------|
| Generator | `Microsoft.AspNetCore.OpenApi` 10.x; `MapOpenApi()` serves `/openapi/{name}.json` |
| Reference UI | `Scalar.AspNetCore`; `/scalar/{name}` |
| Documents | `all` plus one per assembly that contains controllers (found at registration) |
| Compatibility | No shim. Swashbuckle types, the `SwaggerGen` namespaces and the `Swagger` config key are removed |
| Customisation | Transformers registered through `IConfigureOptions<OpenApiOptions>` classes ([pattern 20](../../patterns-discovery/02-design-patterns/20-configure-options-classes.md)) |
| Sign-in | OAuth2 authorization code with PKCE; the JwtBearer `Audience` is the client id |
| XML comments | Read at runtime from the `*.xml` files beside the application |
| Async messaging | AsyncAPI 3 document and viewer (phase 2) |

[Requirements →](requirements.md)
