# Swashbuckle replaced by OpenAPI transformers and Scalar

**Date:** 2026-10-09 · **Epic:** API documentation · **Status:** Phase 1 complete (AsyncAPI phase 2 done, see its own record)

## Summary

Swashbuckle is gone. `OoBDev.AspNetCore.Mvc` and `OoBDev.AspNetCore.JwtAuthentication` use `Microsoft.AspNetCore.OpenApi` transformers and the Scalar API reference. This was a breaking change by the owner's choice (no shim). Design: [OpenApiScalar](../design/OpenApiScalar/README.md).

## What changed

**Table 1 — Changes**

| Item | Change |
|------|--------|
| Packages | `Swashbuckle.AspNetCore` removed; `Microsoft.AspNetCore.OpenApi` 10.0.12 and `Scalar.AspNetCore` 2.17.14 added; the Swashbuckle CLI tool removed from `.config/dotnet-tools.json` |
| Mvc | `SwaggerGen/` and the filters replaced by `OpenApi/` transformers; `MapApiReference()` added; `FormFileOperationFilter` deleted |
| JwtAuthentication | `OAuth2SwaggerOptions`, `ConfigureOAuthSwaggerGenOptions`, `ConfigureOAuthSwaggerUIOptions` replaced by `OAuth2OpenApiOptions`, `OAuthDocumentTransformer`, `OAuthApiReferenceConfigurator`; `TryAddJwtBearerSwaggerGen` is now `TryAddJwtBearerOpenApi` |
| Config | Section `OAuth2SwaggerOptions` is now `OAuth2OpenApiOptions`; builder property renamed to match |
| Example and template | `Program.cs` uses `MapApiReference()`; launch URL is `/scalar/all`; the Keycloak realm has the Scalar redirect URIs |
| Tests | `OpenApiDocumentTests` (4 Simulate tests) |

## Behaviour changes

Security flow moved from implicit to authorization code with PKCE; documents are found at startup; XML comments are read at runtime; the orphan `<Type>Filter` and `OrderBy` component schemas were dropped in favour of property descriptions and `orderBy.<column>` string enums.

## Follow-up

- Phase 2 (AsyncAPI document and viewer) is done: [change record](migration-asyncapi-2026-10-09.md).
- `scripts/templates/verify-templates.ps1` fails at the "test capability" step with NU1008 (generated projects carry package versions while central package management is on). This predates the change and is not fixed here.
- The API reference pages under `docs/Libraries/` and `docs/generated/` still describe the old Swashbuckle classes until they are regenerated.
