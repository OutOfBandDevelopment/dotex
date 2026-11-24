# OoBDev.ComplexEvents

`ComplexEventDispatcher` runs the handlers for an event type in attribute order and throws one `AggregateException` if some fail. `InMemoryScheduleStore` (lease, expiry, re-registration by key, failed state after the attempt limit) and `ScheduleRunner` (leases due items through `TimeProvider`, runs a callback, completes on success) cover CE-1 to CE-4 in process. Handlers must be idempotent: a lease that expires repeats the item.

`CronRecurrence`, `RecurringSchedule` and the catch-up policies (`ScheduleAt` attribute in the Abstractions) cover recurring events. Not built here: the Event Hubs provider (CE-6), persisting the last run time. Design: [ComplexEvents](../../../docs/design/ComplexEvents/README.md).

Failure hook: pass an `IScheduleFailureHandler` to `ScheduleRunner` (or register one for `ScheduleHost`) to dead-letter an item after its last attempt.

Recurring runs: `RecurringScheduler` (a set of `RecurringSchedule`s plus an `ILastRunStore`) is picked up by `ScheduleHost` when registered: it registers missed and upcoming occurrences on start from the recorded last run, and after each run records it and registers the next. `InMemoryLastRunStore` loses the record on restart.

Event Hubs: `EventHubPublisher` serializes an event as JSON and publishes it through an `IEventHubClient` tagged with the event type name; `EventHubConsumer` reads the hub and dispatches each message to the handlers of its type (types given at construction; unknown or malformed messages are counted in `Skipped`). `InMemoryEventHubClient` is the fake; the Azure client is in `OoBDev.Microsoft.Azure.EventHub`.
