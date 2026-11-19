# Feature request: complex events, scheduling and Azure Event Hubs

- **Status:** triaged
- **Requested by:** owner (SharedFramework comparison, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Framework` (abstractions and common), `src/ExternalServices/Microsoft` (Event Hubs)

## Need

Handle domain events with attribute-declared handlers, schedule work to run at a time, persist schedule state, and publish or consume through Azure Event Hubs. dotex has queues (`MessageQueueing`) but no event handler model, scheduler or Event Hubs provider.

## Proposed behavior

- A complex event handler model: `IComplexEventHandler`, a handler attribute, a factory and a resolver.
- A scheduler: register, get, release and update schedule instances; `ScheduleAt` attribute; calculations and an execution provider.
- Schedule persistence with an Entity Framework Core provider.
- An Event Hubs provider and client behind an interface.

## Source projects

Under `C:\repo\mine\proving-grounds\examples\SharedFramework` (read-only reference):

- `OoBDev.ComplexEvents.Abstractions` (19 files)
- `OoBDev.ComplexEvents.Common` (13 files)
- `OoBDev.ComplexEvents.EntityFrameworkCore` (3 files)
- `OoBDev.ComplexEvents.DatabaseExtensions` (no C# files; likely SQL only, check)
- `OoBDev.ComplexEvents.Common.Tests`
- `OoBDev.Microsoft.Azure.EventHub` and `.Tests` (7 files)

## Acceptance criteria

- Given a handler attribute on a class, then the resolver finds it and dispatches a matching event to it.
- Given a schedule, then it runs once at the due time and is released; a restart does not run it twice.
- Given Event Hubs, then tests use an emulator or a fake client; live tests are `LiveIntegration`.

## Existing code checked

Searched `src` for `IComplexEventHandler`, `ScheduleAt`, `IEventHubProvider` and `Azure.Messaging.EventHubs`: no matches. `OoBDev.MessageQueueing*` and `OoBDev.Communications.MessageQueueing` cover queues, not this.

## Security and operations

A scheduler that runs persisted work needs idempotence and a lease so two instances do not run the same item. Connection strings come from configuration.

## Priority

- **Impact:** an event and scheduling backbone; large.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

The owner's event-sourcing material (`event-sourcing`, `sync-mesh`) overlaps; the rule is to ask before copying it. Does this stay separate from it? Is `ComplexEvents.DatabaseExtensions` a database project to bring as well?

## Triage notes

Origin: SharedFramework. Proposed design written 2026-11-19: [ComplexEvents](../../design/ComplexEvents/README.md).

2026-11-11: triaged. Accepted as a backlog item: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, SharedFramework requests.
