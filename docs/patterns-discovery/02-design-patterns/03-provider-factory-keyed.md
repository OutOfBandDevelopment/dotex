# Pattern 3 — Provider / Factory with keyed services

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 2 — `TryAdd*` everywhere](./02-tryadd-everywhere.md) · [Pattern 4 — `ISelectedService<T>` — config-selected provider →](./04-selected-service.md)
<!-- nav -->

**What:** A vendor adapter registers itself **twice**: once un-keyed (first `TryAdd` wins → "default"), once keyed (deterministic lookup).

```csharp
// ExternalServices/Redis/OoBDev.Redis.Caching/RedisCachingRegistrar.cs
services.TryAddTransient<ICachingProvider, RedisCachingProvider>();
services.TryAddKeyedTransient<ICachingProvider, RedisCachingProvider>("Redis");
services.TryAddTransient<IConnectionMultiplexerFactory, ConnectionMultiplexerFactory>();
```

Supporting conventions:

**Table 1 — Provider registration conventions**

| Convention | Example |
|------------|---------|
| Provider key is a **string constant in a `{Vendor}Globals` class** | `RabbitMQGlobals.MessageProviderKey = "rabbit-mq"` |
| Keys are **kebab-case** (lower-case words separated by hyphens) | `"rabbit-mq"`, `"azure-storage-queue"`, `"servicebus"` |
| Enum-typed keys forward to a string key | `TryAddKeyedSingleton(HashTypes.Sha256, (sp,key) => sp.GetRequiredKeyedService<IHash>(...))` (today the string is upper-cased; should become kebab-case) |
| Vendor SDK objects are created through a small **`I{Vendor}ClientFactory`** so they can be mocked | `IOllamaApiClientFactory`, `IQueueClientFactory`, `IConnectionMultiplexerFactory` |
| Registration logic sits in an internal **`{Vendor}Registrar`** class; the public extension delegates to it | `TryAddRedisCachingServices → new RedisCachingRegistrar().AddServices(services)` |

**Standard:** provider keys are kebab-case (done 2026-10-09 for Groq, Handlebars, memory cache, SQL Server, Ollama, Redis and SBert; the earlier upper-case aliases (`LegacyKey`) were removed 2026-10-09 because the framework is unreleased), as the message-queue adapters already do (`"rabbit-mq"`, `"sqs"`, `"servicebus"`, `"azure-storage-queue"`).

**Key discovery, not centralization:** keys follow a common, discoverable pattern (a `const string` named `ProviderKey` or `MessageProviderKey` in the adapter's own `{Vendor}Globals` class, value in kebab-case) but are deliberately **not** collected in one global registry. A shared key list would have to be referenced by every adapter and by the abstractions, which breaks the minimum-reference rule that an adapter references only its capability's Abstractions project ([layers](../01-architecture/02-five-source-layers.md)). Consumers find a key by convention (search for `*Globals`) or in the adapter's readme, which lists it under Configuration.

**Rough edges:** several keys do not follow the naming standard (the upper-cased enum forwarding for hashes and serializers, and the `HMAC*` keys; the user accessor keys are now `"http"` and `"environment"`), and some adapters inline the key string instead of exposing a constant. Fixing these is tracked in `TODO.md` ("Backlog: Naming Consistency"); because keys appear in configuration files this is a breaking change to plan for.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 2 — `TryAdd*` everywhere](./02-tryadd-everywhere.md) · [Pattern 4 — `ISelectedService<T>` — config-selected provider →](./04-selected-service.md)
<!-- nav -->
