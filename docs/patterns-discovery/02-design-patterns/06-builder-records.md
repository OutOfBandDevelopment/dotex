# Pattern 6 — Builder *records* carrying config section names

[↑ Design Patterns](./README.md) · [← 5. Config-resolved provider per channel/message](./05-config-resolved-provider.md) · [7. Options binding by section name →](./07-options-binding-by-section-name.md)

**What:** Each layer has a `record …Builder` whose properties are **config section names** (and a few feature switches), defaulting to `nameof(TheOptionsType)`.

```csharp
public record ExternalExtensionBuilder
{
    public string QdrantOptionSection { get; init; } = nameof(QdrantOptions);
    public string OllamaApiClientOptionSection { get; init; } = nameof(OllamaApiClientOptions);
    ...
}
public record HostingBuilder { public bool DisableMessageQueueing { get; init; } = false; ... }
```

**Why it works well:** zero-config by default, per-app override with object-initializer syntax, records are immutable and diff-friendly, and it composes: `TryAllCommonExtensions` takes one builder per layer.

**Repeat:** one builder record per *registration entry point*; property names end in `Section` for config paths, `Disable…`/`Require…` for switches, and enums with `[Flags]` for multi-select (`IdentityProviders`).

---

[↑ Design Patterns](./README.md) · [← 5. Config-resolved provider per channel/message](./05-config-resolved-provider.md) · [7. Options binding by section name →](./07-options-binding-by-section-name.md)
