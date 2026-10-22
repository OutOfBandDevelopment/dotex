# OpenAPI and Scalar design reconciled with the code

**Date:** 2026-10-22 · **Compared:** `docs/design/OpenApiScalar/*` against `OoBDev.AspNetCore.Mvc/OpenApi`, `OoBDev.AspNetCore.JwtAuthentication/OpenApi` and `OpenApiDocumentTests`.

## Design updated to match the code

- Overview status: phase 2 (AsyncAPI) is implemented, with a link to its own design.
- API design: `ApiPermissionsExtension` (writes `x-permissions`) added to the type table.
- Testing strategy: phase 2 tests point to the AsyncApi design.

## Design wanted, code lacks

Nothing found: registration, catalog, transformers, `MapApiReference`, the OAuth options and the four Simulate tests match.

## Verification

`OoBDev.Docs.Tests` run on `docs/design/OpenApiScalar`.
