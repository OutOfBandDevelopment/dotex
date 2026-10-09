# Naming and Project Layout

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← Index](./README.md) · [Build and Project File Conventions →](./02-build-and-csproj.md)
<!-- nav -->

## Project names

**Table 1 — Project naming conventions**

| Kind | Pattern | Example |
|------|---------|---------|
| Contracts | `OoBDev.{Feature}.Abstractions` | `OoBDev.Caching.Abstractions` |
| Default implementation | `OoBDev.{Feature}` | `OoBDev.Caching` |
| Hosting glue | `OoBDev.{Feature}.Hosting` | `OoBDev.MessageQueueing.Hosting` |
| Vendor adapter | `OoBDev.{Vendor}.{Feature}` | `OoBDev.Redis.Caching` |
| Tests | `{Project}.Tests` | `OoBDev.Caching.Tests` |
| Roll-up | `OoBDev.Common.{Area}` | `OoBDev.Common.AspNetCore` |

*The suffix is what the build keys on:* `.Abstractions` is stripped from `RootNamespace`, `.Tests` turns off packing, and the Common glob projects include or remove projects by suffix. See [the Abstractions split](../01-architecture/03-abstractions-implementation-split.md) and [the Common aggregator](../01-architecture/04-common-layer-aggregator.md).

## Type names

**Table 2 — Type naming conventions**

| Role | Convention | Example |
|------|-----------|---------|
| Interface | `I{Name}` | `ICachingProvider` |
| Implementation | `{Name}`, no `Impl` suffix | `CachingManager` |
| Provider | `{Name}Provider` | `RedisCachingProvider` |
| Factory | `{Name}Factory` | `CacheableFactory`, `IOllamaApiClientFactory` |
| Registration | `TryAdd{Capability}Services`, `TryAdd{Layer}Extensions` | `TryAddCachingServices` |
| Registration class | `ServiceCollectionExtensions` (standard everywhere; `ServiceCollectionEx` retired 2026-10-09) | see [known warts](./07-known-warts.md) |
| Vendor registrar | `{Vendor}{Feature}Registrar` (internal) | `RedisCachingRegistrar` |
| Builder | `{Layer}ExtensionBuilder` / `{Area}Builder` (record) | `ExternalExtensionBuilder` |
| Options | `{Thing}Options` (record or class) | `OllamaApiClientOptions` |
| Provider keys | `{Vendor}Globals.ProviderKey` (or `MessageProviderKey`) kebab-case constants | `"rabbit-mq"`, `"ollama"` |
| Attributes | `{Name}Attribute`, used without the suffix | `[IsCacheable]`, `[MessageQueue]` |
| Test methods | `Method_Scenario_ExpectedBehavior` | see [testing](./04-testing-practices.md) |

## Folder layout inside a project

```
OoBDev.Caching/
├── README.{Project}.md        packed readme
├── OoBDev.Caching.csproj
├── AssemblyInfo.cs            InternalsVisibleTo for the Tests project
├── ServiceCollectionExtensions.cs     the public registration entry point
├── Factories/                 one folder per role, plural
├── Managers/
└── Providers/
```

The tests project mirrors the folders (`Factories/`, `Managers/`, `Providers/`) and adds `Examples/`, `IntegrationServices.cs` and `GlobalSuppressions.cs` where needed.

## Layer placement checklist

1. Pure contracts with no third-party types → `Abstractions` project in **Framework** (or **Common** if truly foundational).
2. Vendor SDK involved → **ExternalServices**, referencing only the `*.Abstractions` project.
3. Optional or niche → **Extensions**.
4. Needs the whole graph (composition) → an **application**, or a Common roll-up.

---

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← Index](./README.md) · [Build and Project File Conventions →](./02-build-and-csproj.md)
<!-- nav -->
