# Pattern 8 — Config-gated registration

[↑ Design Patterns](./README.md) · [← 7. Options binding by section name](./07-options-binding-by-section-name.md) · [9. `#if DEBUG` explicit-argument extension methods →](./09-if-debug-explicit-arguments.md)

**What:** An adapter's `TryAdd…` inspects configuration and registers **nothing** if its section is absent:

```csharp
var url = configuration.GetSection(section)?[nameof(OllamaApiClientOptions.Url)];
if (url == null) return services;   // Ollama not configured → not registered
```

This is what lets `TryAllCommonExtensions` pull in 20+ adapters safely. It also means *failure is late* (`GetRequiredService` throws when someone asks for it).

---

[↑ Design Patterns](./README.md) · [← 7. Options binding by section name](./07-options-binding-by-section-name.md) · [9. `#if DEBUG` explicit-argument extension methods →](./09-if-debug-explicit-arguments.md)
