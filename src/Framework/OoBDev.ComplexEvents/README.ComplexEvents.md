# OoBDev.ComplexEvents

`ComplexEventDispatcher` runs the handlers for an event type in attribute order and throws one `AggregateException` if some fail. `InMemoryScheduleStore` (lease, expiry, re-registration by key, failed state after the attempt limit) and `ScheduleRunner` (leases due items through `TimeProvider`, runs a callback, completes on success) cover CE-1 to CE-4 in process. Handlers must be idempotent: a lease that expires repeats the item.

`CronRecurrence`, `RecurringSchedule` and the catch-up policies (`ScheduleAt` attribute in the Abstractions) cover recurring events. Not built here: the Event Hubs provider (CE-6), persisting the last run time. Design: [ComplexEvents](../../../docs/design/ComplexEvents/README.md).
