# Feature request: health check detail by caller level

- **Status:** triaged
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** `OoBDev.AspNetCore.Mvc` (health checks, OpenAPI document)

## Need

The health endpoint returns the same body to everyone. An anonymous probe only needs the overall status; a signed-in caller can see the list of checks; only a caller with a special claim should see descriptions, data and errors (which can leak internals). The OpenAPI transformer also hard-codes the endpoint path instead of looking it up.

## Proposed behavior

Three response levels chosen from the caller's claims: anonymous gets `status` only; authenticated gets the check list without descriptions; a configured claim or application right gets descriptions, data and errors. The documented path comes from the mapped endpoint.

## Acceptance criteria

- Given an anonymous request, when `/health` is called, then only the overall status is returned.
- Given an authenticated caller without the detail right, then check names and statuses are returned without descriptions.
- Given a caller with the detail right, then descriptions, data and errors are returned.
- Given the health endpoint is mapped at another path, then the OpenAPI document shows that path.

## Existing code checked

`HealthCheckOptionsFactory`, `HealthChecksDocumentTransformer`, `docs/design/ApplicationRights`. No existing request or design covers the response levels.

Source comments (replaced by a reference to this request):

- `src/Framework/OoBDev.AspNetCore.Mvc/Diagnostics/HealthChecks/HealthCheckOptionsFactory.cs`: "make it so unauthenticated only gets the status"; "authenticates should get result list without descriptions"; "special claim gets descriptions, data and errors".
- `src/Framework/OoBDev.AspNetCore.Mvc/OpenApi/HealthChecksDocumentTransformer.cs`: "make so this can be looked up" (the `/health` path).

## Security and operations

Authorization uses application rights (`[ApplicationRight]`, owner decision); the detail level is a right, not a role. No new processes or storage.

## Priority

- **Impact:** health detail is either exposed to everyone or hidden from operators.
- **Needed by:** no date.
- **Workaround today:** none.

## Open questions

Which right name gates the detailed view (default `health.details`)?

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments). Design document to follow when built.
