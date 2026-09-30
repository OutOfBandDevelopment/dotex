# Pattern 16 — Injectable non-determinism

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 15 — Result envelope](./15-result-envelope.md) · [Pattern 17 — Attribute + mapper data access →](./17-attribute-mapper-data-access.md)
<!-- nav -->

`IDateTimeProvider`, `IGuidProvider`, `ITempFileFactory`, `ICurrentUserAccessor`, `IHttpPrepareRequest`… Everything that touches the clock, GUIDs, filesystem or ambient identity is behind an interface registered by `TryAddProviders()`.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 15 — Result envelope](./15-result-envelope.md) · [Pattern 17 — Attribute + mapper data access →](./17-attribute-mapper-data-access.md)
<!-- nav -->
