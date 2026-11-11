# OoBDev.EventSourcing

Implementation side of event sourcing (contracts in `OoBDev.EventSourcing.Abstractions`; design in `docs/design/CqrsEventSourcing/`).

- `InMemoryEventStore` and `InMemoryEntityStore`: thread-safe in-memory providers for tests and single-process use.
- `EventRelay`: `RunOnceAsync` reads the event store after its checkpoint, sends each event through `IMessageQueueSender<EventsChannel>` (message id `{StreamId}:{Sequence}`, at-least-once) and saves the checkpoint in the entity store. Events younger than `SafetyWindow` wait for the next pass. A hosted loop is a follow-up.
- `TryAddEventSourcing()`: registers the in-memory defaults, the configured selection of the stores (`OoBDev:EventSourcing:EventStore:Type`, `OoBDev:EventSourcing:EntityStore:Type`; an unknown key throws) and the relay (options `OoBDev:EventSourcing:Relay`). It expects an `IMessageQueueSender<EventsChannel>` from the message-queue registration.

Entity Framework stores, the CloudEvents mapper and the queue adapters are separate projects (see the backlog).

## CloudEvents at the edge

`CloudEventMapper.ToCloudEvent(record, source)` and `FromCloudEvent(json)` convert an `EventRecord` to and from a CloudEvents 1.0 structured JSON document (`application/cloudevents+json`). `id` is `{StreamId}:{Sequence}`, `subject` the stream id; `sequence` and `metadata` are extension attributes. An external event that has only an `id` of the form `stream:sequence` is accepted. Anything else raises `FormatException`.
