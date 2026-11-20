# OoBDev.ComplexEvents

`ComplexEventDispatcher` runs the handlers for an event type in attribute order and throws one `AggregateException` if some fail. `InMemoryScheduleStore` (lease, expiry, re-registration by key, failed state after the attempt limit) and `ScheduleRunner` (leases due items through `TimeProvider`, runs a callback, completes on success) cover CE-1 to CE-4 in process. Handlers must be idempotent: a lease that expires repeats the item.

Not built: the EF Core store (CE-5), a hosted service wrapper, the Event Hubs provider (CE-6), validated options (CE-7), catch-up policies. Design: [ComplexEvents](../../../docs/design/ComplexEvents/README.md).
