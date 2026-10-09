# OpenApiScalar — Testing Strategy

[← API design](api-design.md) · [Overview](README.md)

## Simulate tests

`OpenApiDocumentTests` in `OoBDev.AspNetCore.Tests` starts a Kestrel host on a free port with a small controller and reads the documents over HTTP.

**Table 1 — Cases**

| Test | Checks |
|------|--------|
| `Catalog_ListsAllAndAssemblyDocuments` | Document names |
| `Document_IncludesHealthAndPermissions` | `/health` path, `x-permissions` for a right and for `AllowAnonymous` |
| `Document_AssemblyDocumentOnlyHasItsControllers` | Per-assembly title and paths (also guards the lower-case document name) |
| `Document_SearchQueryEndpoint_DescribesSortingAndPagedResponse` | `IQueryable` tag, `orderBy.<column>`, `pageSize`, paged response schema, XML summary |

The test project generates its XML documentation file so the XML transformers have input. Test controllers must be top-level public classes, because MVC does not discover nested ones.

## Live checks (manual)

Run the example (`dotnet run --project src/Examples/OoBDev.Example.WebApi`), then request `/openapi/all.json`, `/openapi/OoBDev.Example.WebApi.json` and `/scalar/all`. With the Keycloak container up, sign in from Scalar and call a protected endpoint.

## Gaps

- OAuth sign-in in the browser is not automated.
- Phase 2 tests are defined with its own design.

[← API design](api-design.md) · [Overview](README.md)
