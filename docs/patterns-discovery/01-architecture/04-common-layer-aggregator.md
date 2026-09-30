# Architecture § 4 — The Common Layer Is an Aggregator (Convention over Reference)

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 3 — The Abstractions / Implementation Split](./03-abstractions-implementation-split.md) · [Architecture § 5 — Composition Root →](./05-composition-root.md)
<!-- nav -->

`OoBDev.Common.csproj` does not list projects. It **globs** them, then subtracts categories:

```xml
<ProjectReference Include="..\..\Framework\OoBDev.*\OoBDev.*.csproj" />
<ProjectReference Remove="..\..\Framework\**\OoBDev.*.Tests.csproj" />
<ProjectReference Remove="..\..\Framework\**\OoBDev.*.Abstractions.csproj" />
<ProjectReference Remove="..\..\Framework\**\OoBDev.*.Hosting.csproj" />
<ProjectReference Remove="..\..\Framework\**\OoBDev.AspNetCore.*.csproj" />
```

The suffix conventions **are** the architecture: `.Abstractions`, `.Hosting`, `.Tests`, `.AspNetCore.*`, `.DB`, `.Net481`
determine which roll-up a project lands in.

**Table 2 — Common roll-up projects**

| Roll-up | Pulls in | Entry point |
|---------|----------|-------------|
| `OoBDev.Common` | Framework + Extensions implementations | `TryCommonExtensions(cfg, SystemExtensionBuilder?)` |
| `OoBDev.Common.Abstractions` | every `*.Abstractions` | *(no registration; type-only)* |
| `OoBDev.Common.AspNetCore` | `OoBDev.AspNetCore.*` | `TryCommonAspNetCoreExtensions(cfg, AspNetCoreExtensionBuilder?, JwtExtensionBuilder?)` |
| `OoBDev.Common.Extensions` | every ExternalServices adapter | `TryCommonExternalExtensions(cfg, IdentityExtensionBuilder?, ExternalExtensionBuilder?)` |
| `OoBDev.Common.Hosting` | every `*.Hosting` | `TryCommonHosting(cfg, HostingBuilder?)` |
| `OoBDev.Common.Complete` | the five above + all `ReadMe.*.md` as package content | *(meta-package)* |

`OoBDev.Common/ServiceCollectionExtensions.cs` then offers `TryAllCommonExtensions(...)` which calls the four entry points in order. An application therefore needs **two lines** (`TryAllCommonExtensions`, `UseAllCommonMiddleware`) to get the whole stack, and can opt out per feature through the builder records (e.g. `HostingBuilder.DisableMessageQueueing`).

**Consequence to be aware of:** adding a new `OoBDev.Foo` project under `Framework/` automatically enters the `Common` package graph – but its *registration* must still be added by hand to the matching `Try*` method. Reference is by convention; wiring is explicit.

---

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 3 — The Abstractions / Implementation Split](./03-abstractions-implementation-split.md) · [Architecture § 5 — Composition Root →](./05-composition-root.md)
<!-- nav -->
