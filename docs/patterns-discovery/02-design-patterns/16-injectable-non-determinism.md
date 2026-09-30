# Pattern 16 — Injectable non-determinism

[↑ Design Patterns](./README.md) · [← 15. Result envelope](./15-result-envelope.md) · [17. Attribute + mapper data access →](./17-attribute-mapper-data-access.md)

`IDateTimeProvider`, `IGuidProvider`, `ITempFileFactory`, `ICurrentUserAccessor`, `IHttpPrepareRequest`… Everything that touches the clock, GUIDs, filesystem or ambient identity is behind an interface registered by `TryAddProviders()`.

---

[↑ Design Patterns](./README.md) · [← 15. Result envelope](./15-result-envelope.md) · [17. Attribute + mapper data access →](./17-attribute-mapper-data-access.md)
