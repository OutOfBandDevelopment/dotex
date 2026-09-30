# Pattern 7 — Options binding by section name

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 6 — Builder *records* carrying config section names](./06-builder-records.md) · [Pattern 8 — Config-gated registration →](./08-config-gated-registration.md)
<!-- nav -->

```csharp
services.Configure<OllamaApiClientOptions>(o => configuration.Bind(sectionName, o));
// consumer:
public OllamaApiClientFactory(IOptions<OllamaApiClientOptions> options) ...
```

* Option types are **`record`s with `required … { get; init; }`** (`OllamaApiClientOptions`) or plain classes (`FileTemplatingOptions`).
* Consumers take `IOptions<T>` (not `IOptionsMonitor`) – configuration is read once.
* There is **no `ValidateOnStart` / DataAnnotations validation** in the codebase; a missing value fails at first use with `ConfigurationMissingException` or a `NullReference`.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 6 — Builder *records* carrying config section names](./06-builder-records.md) · [Pattern 8 — Config-gated registration →](./08-config-gated-registration.md)
<!-- nav -->
