# OoBDev.EventSourcing

Implementation side of event sourcing (contracts in `OoBDev.EventSourcing.Abstractions`; design in `docs/design/CqrsEventSourcing/`).

- `InMemoryEventStore` and `InMemoryEntityStore`: thread-safe in-memory providers for tests and single-process use.
- `EventRelay`: `RunOnceAsync` reads the event store after its checkpoint, sends each event through `IMessageQueueSender<EventsChannel>` (message id `{StreamId}:{Sequence}`, at-least-once) and saves the checkpoint in the entity store. Events younger than `SafetyWindow` wait for the next pass. A hosted loop is a follow-up.
- `TryAddEventSourcing()`: registers the in-memory defaults, the configured selection of the stores (`OoBDev:EventSourcing:EventStore:Type`, `OoBDev:EventSourcing:EntityStore:Type`; an unknown key throws) and the relay (options `OoBDev:EventSourcing:Relay`). It expects an `IMessageQueueSender<EventsChannel>` from the message-queue registration.

Entity Framework stores, the CloudEvents mapper and the queue adapters are separate projects (see the backlog).

## CloudEvents at the edge

`CloudEventMapper.ToCloudEvent(record, source)` and `FromCloudEvent(json)` convert an `EventRecord` to and from a CloudEvents 1.0 structured JSON document (`application/cloudevents+json`). `id` is `{StreamId}:{Sequence}`, `subject` the stream id; `sequence` and `metadata` are extension attributes. An external event that has only an `id` of the form `stream:sequence` is accepted. Anything else raises `FormatException`.

## Aggregates

`AggregateRoot` (Abstractions) changes state only through `Raise(event)`, which calls your `Apply(object)` and keeps the event until it is saved. `AggregateRepository<T>` (resolved as `IAggregateRepository<T>` after `TryAddEventSourcing()`) loads by replaying the stream `{TypeName}-{id}` and saves the uncommitted events with the aggregate's `Version` as the expected sequence; a concurrent writer gets `EventStreamConflictException`. Events are stored as JSON under a name from `[EventType("name")]` (or the class name); register the classes with `EventTypeRegistry.Register<T>()` or `RegisterAssembly`. Not yet: snapshots and upcasting.

The relay loop is hosted by `OoBDev.EventSourcing.Hosting` (`TryAddEventRelayHost()`); the pause between empty passes is `OoBDev:EventSourcing:Relay:PollInterval`.

## Projections

Implement `IProjection` (`Name`, idempotent `ApplyAsync(EventRecord)`) and register it. `ProjectionRunner.CatchUpAsync()` reads the event store from each projection checkpoint (kept in the entity store as `projection-checkpoint`), applies the events in batches and saves the checkpoint after each batch; `ResetAsync(name)` clears the checkpoint to rebuild. Run one runner per projection. Queue-driven projections stay possible with an `IMessageQueueHandler<EventsChannel, EventRecord>`.

## Upcasting

Register `IEventUpcaster` implementations (`FromType`, `Upcast(payload)` returning the new type name and payload). `EventUpcastPipeline` chains them (a cycle throws) and `AggregateRepository` applies it when loading, so stored events are never rewritten.

## Snapshots

An aggregate that implements `ISnapshotable` (`CaptureState`, `RestoreState`) is snapshotted into the entity store (type `aggregate-snapshot`) each time its stream version passes a multiple of `OoBDev:EventSourcing:Snapshots:Interval` (default 100, 0 turns it off). `AggregateRepository` restores the snapshot and replays only the later events; the stream stays the source of truth, so a snapshot can be deleted at any time.

## CloudEvents ingress

`CloudEventsIngressHandler` is an `IMessageQueueHandler<EventsChannel, string>`: register it with the queue provider's handler registration and any queue can feed a stream with CloudEvents 1.0 JSON. The event keeps its stream and sequence; a message that does not follow the stream's last sequence throws, so the queue retries or dead-letters it. Set `OoBDev:EventSourcing:Relay:CloudEventsSource` and the relay publishes CloudEvents JSON instead of `EventRecord`, so the same handler type reads what another instance sends. An HTTP endpoint (Q-CQRS-EDGE) is not built.
