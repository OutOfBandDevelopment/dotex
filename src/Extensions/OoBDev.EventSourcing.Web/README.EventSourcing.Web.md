# OoBDev.EventSourcing.Web

The CloudEvents HTTP edge for the event store (owner, 2026-11-11; placement in `src/Extensions` confirmed 2026-11-12). It lives here, not in `OoBDev.EventSourcing`, because it needs the ASP.NET `FrameworkReference`. Plan: [cloudevents-edge-plan](../../../docs/design/CqrsEventSourcing/cloudevents-edge-plan.md).

## Use

```csharp
app.MapCloudEvents().RequireAuthorization("events-writer"); // POST /events
```

`MapCloudEvents(pattern = "/events")` maps `POST` for one structured CloudEvent 1.0 (`application/cloudevents+json`). The event is mapped with `CloudEventMapper.FromCloudEvent` and appended to the selected event store (`EventSourcingGlobals.EventStoreSelectedKey`, registered by `TryAddEventSourcing`) with the expected sequence `sequence - 1`, the same rule as `CloudEventsIngressHandler`.

| Status | When |
|--------|------|
| 202 | the event was appended |
| 400 | not valid JSON or not a valid CloudEvent (problem details) |
| 409 | the event does not follow its stream (problem details with the last sequence) |
| 415 | another content type (problem details) |

The endpoint is never anonymous by policy of its own: it carries none, so the host chains `RequireAuthorization`. Batches (`application/cloudevents-batch+json`) are a later option.

## Tests

`OoBDev.EventSourcing.Web.Tests` (`Unit`, a real Kestrel host on a free port): accepted, next in stream, sequence gap, wrong content type, three invalid events, and rejection when a policy is set with no user.
