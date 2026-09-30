# Pattern 15 — Result envelope

[↑ Design Patterns](./README.md) · [← 14. Strategy collections (engine → providers → sources)](./14-strategy-collections.md) · [16. Injectable non-determinism →](./16-injectable-non-determinism.md)

`IResult`, `IModelResult<T>`, `IQueryResult<T>`, `IPagedQueryResult<T>` share `IReadOnlyCollection<ResultMessage>` where `ResultMessage` is a record `{ Level, Message, MessageCode, Context, MetaData }`. Interfaces are non-generic + generic (`IModelResult` / `IModelResult<T>` with `new T? Data`) so untyped code (filters, logging) can consume them.

---

[↑ Design Patterns](./README.md) · [← 14. Strategy collections (engine → providers → sources)](./14-strategy-collections.md) · [16. Injectable non-determinism →](./16-injectable-non-determinism.md)
