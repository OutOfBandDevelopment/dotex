# Pattern 3 — Provider / Factory with keyed services

[↑ Design Patterns](./README.md) · [← 2. `TryAdd*` everywhere](./02-tryadd-everywhere.md) · [4. `ISelectedService<T>` — config-selected provider →](./04-selected-service.md)

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
| Or an **upper-case vendor name** | `"OLLAMA"`, `"Redis"`, `"HTTP"`, `"Environment"` |
| Enum-typed keys forward to upper-case string keys | `TryAddKeyedSingleton(HashTypes.Sha256, (sp,key) => sp.GetRequiredKeyedService<IHash>(key.ToString().ToUpper()))` |
| Vendor SDK objects are created through a small **`I{Vendor}ClientFactory`** so they can be mocked | `IOllamaApiClientFactory`, `IQueueClientFactory`, `IConnectionMultiplexerFactory` |
| Registration logic sits in an internal **`{Vendor}Registrar`** class; the public extension delegates to it | `TryAddRedisCachingServices → new RedisCachingRegistrar().AddServices(services)` |

**Rough edges:** key casing is inconsistent (`"Redis"` vs `"OLLAMA"` vs `"rabbit-mq"`), and there is no central registry of keys. If a new framework is started, define keys as `const string` on the abstraction (`ICachingProvider.Keys.Redis`) or use an enum.

---

[↑ Design Patterns](./README.md) · [← 2. `TryAdd*` everywhere](./02-tryadd-everywhere.md) · [4. `ISelectedService<T>` — config-selected provider →](./04-selected-service.md)
