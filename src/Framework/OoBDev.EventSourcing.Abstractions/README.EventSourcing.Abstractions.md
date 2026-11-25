# OoBDev.EventSourcing.Abstractions

Contracts for event sourcing with three separate, swappable parts (design: `docs/design/CqrsEventSourcing/`).

- `IEventStore`: append-only history. `AppendAsync` takes the last sequence the caller has seen and returns a result that reports a conflict instead of throwing; `ReadAsync` reads one stream; `ReadAllAsync` reads the global feed by position.
- `IEntityStore`: read models and projection checkpoints with a version as concurrency token.
- Event stream: no new contract. Events travel on the existing message-queue pattern through the `EventsChannel` marker (`IMessageQueueSender<EventsChannel>` and `IMessageQueueHandler<EventsChannel, EventRecord>`).

`EventRecord`, `NewEvent`, `AppendResult` and `EntityRecord` are plain records; payloads are JSON text so every store can hold them.

The in-memory providers, the event relay and the registration are in `OoBDev.EventSourcing`.
