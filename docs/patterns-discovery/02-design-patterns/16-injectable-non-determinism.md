# Pattern 16 — Injectable non-determinism

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 15 — Result envelope](./15-result-envelope.md) · [Pattern 17 — Attribute + mapper data access →](./17-attribute-mapper-data-access.md)
<!-- nav -->

`IDateTimeProvider`, `IGuidProvider`, `ITempFileFactory`, `ICurrentUserAccessor`, `IHttpPrepareRequest`… Everything that touches the clock, GUIDs, filesystem or ambient identity is behind an interface registered by `TryAddProviders()`.

**Origin and direction (under review, tracked in [`TODO.md`](../../../TODO.md)):** many of these interfaces were hand-built because .NET had no equivalent primitive at the time. Where the platform now provides one, prefer it: `IDateTimeProvider` is now `[Obsolete]` and `DateTimeProvider` delegates to `System.TimeProvider`, which `TryAddProviders()` registers as `TimeProvider.System`. New code injects `TimeProvider` (the Handlebars `date_now` helper already does) and tests use `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`. The rest are evaluated case by case.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 15 — Result envelope](./15-result-envelope.md) · [Pattern 17 — Attribute + mapper data access →](./17-attribute-mapper-data-access.md)
<!-- nav -->
