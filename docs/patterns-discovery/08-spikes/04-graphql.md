# Spike — GraphQL (HotChocolate) beside the rights middleware and the search syntax

**Last Updated:** 2026-11-25

## Question

Owner decision: GraphQL is under evaluation without breaking the custom search syntax and the `[ApplicationRight]` model. Can HotChocolate sit on the existing `ISearchProvider` and `IApplicationRightChecker` without a second rule set?

Code: [`src/Spikes/OoBDev.Spike.GraphQl`](../../../src/Spikes/OoBDev.Spike.GraphQl) (HotChocolate.Execution 15.1.21, in-process executor, no web host). Five unit tests pass; `dotnet test src/OoBDev.Spikes.slnx` stays green.

## Result

**Table 1 — Findings**

| Concern | Finding |
|---------|---------|
| Rights | A field attribute (`[GraphQlApplicationRight("search.read")]`, an `ObjectFieldDescriptorAttribute`) runs the same `IApplicationRightChecker` as field middleware: unauthenticated and forbidden calls get an error with a code and the provider is never called. One checker, one right name per capability. |
| Principal | The host puts the `ClaimsPrincipal` into request global state (the JWT bearer pipeline already produced it); no GraphQL-specific identity. |
| Custom search syntax | Works unchanged as one `text` argument parsed by `SearchTextParser`, so existing queries and the REST form keep their grammar. |
| Typed filters | A second field (`searchTyped(ext: ...)`) builds the same `SearchRequest` from arguments; the schema then documents the filters, but each filter is hand-mapped, and `SupportedFilters` checks stay in the provider. |
| Paging | `first`/`after` map to `PageSize`/`Cursor`; the opaque cursor passes through, so the hybrid provider's cursor works as is. |
| Cost | One new package family; the executor alone is enough for tests, an HTTP endpoint needs `HotChocolate.AspNetCore` (a host-bound reference, so it belongs in an application or an `AspNetCore` adapter, per the layering rule). |

## Verdict and next step

Fits, with no change to the rights model or the search grammar. Risks: per-field rights do not cover nested selections unless every field is attributed (a depth and cost limit is needed before exposure), and a GraphQL endpoint is a second surface to keep in step with the OpenAPI document. Recommendation: do not add a framework project now; when a consumer asks, add an application-level endpoint using this attribute, keeping REST and OData as the primary surfaces. Backlog item closed as a spike; the build stays on demand.
