# CloudEvents HTTP edge: migration plan

**Status:** placement confirmed by the owner 2026-11-12 (`OoBDev.EventSourcing.Web` in `src/Extensions`); built 2026-11-16 ([readme](../../../src/Extensions/OoBDev.EventSourcing.Web/README.EventSourcing.Web.md)).
**Last Updated:** 2026-11-16

## Contents

- [Need](#need)
- [Migration plan](#migration-plan)
- [Behaviour](#behaviour)
- [Test plan](#test-plan)

## Need

External producers post CloudEvents 1.0 (structured JSON, `application/cloudevents+json`) to `POST /events`; the endpoint maps each event with the existing `CloudEventMapper` and appends or forwards it so the normal handlers run (owner, 2026-11-11; see [api-design](api-design.md#cloudevents-at-the-edge)).

## Migration plan

**Table 1 — Project placement**

| Project | Current location | Target location | Namespace | Package | References that change |
|---------|------------------|-----------------|-----------|---------|------------------------|
| `OoBDev.EventSourcing.Web` | none (new) | `src/Extensions/OoBDev.EventSourcing.Web` | `OoBDev.EventSourcing.Web` | `OoBDev.EventSourcing.Web` | `FrameworkReference Microsoft.AspNetCore.App`; `ProjectReference` to `OoBDev.EventSourcing` and `OoBDev.EventSourcing.Abstractions` |
| `OoBDev.EventSourcing.Web.Tests` | none (new) | `src/Extensions/OoBDev.EventSourcing.Web.Tests` | `OoBDev.EventSourcing.Web.Tests` | not packed | references the project above |
| `OoBDev.EventSourcing` | `src/Framework` | unchanged | unchanged | unchanged | none (stays host-free, keeps `CloudEventMapper`) |

Reason for `src/Extensions`: Framework must not carry an ASP.NET `FrameworkReference`, and the existing web libraries (`OoBDev.AspNetCore.OData`, `OoBDev.AspNetCore.Cgi`) live there. No existing project moves.

## Behaviour

- `MapCloudEvents(this IEndpointRouteBuilder, string pattern = "/events")` registers `POST`; the registration class is `ServiceCollectionExtensions`.
- Accepts a single structured event; a batch (`application/cloudevents-batch+json`) is a later option.
- Responses are RFC 9457 problem details: 415 for another content type, 400 for an invalid event, 202 when accepted.
- Authorization follows the host: the endpoint is mapped with the host's policy (`RequireAuthorization`), never anonymous by default.

## Test plan

Unit tests with a real Kestrel host on a free port (no `TestHost` package) in `OoBDev.EventSourcing.Web.Tests`: accepted event reaches the store, wrong content type 415, malformed event 400, unauthenticated request rejected when a policy is set. Category `Unit`.
