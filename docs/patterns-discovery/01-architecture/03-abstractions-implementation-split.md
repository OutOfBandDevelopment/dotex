# Architecture § 3 — The Abstractions / Implementation Split

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 2 — The Five Source Layers](./02-five-source-layers.md) · [Architecture § 4 — The Common Layer Is an Aggregator (Convention over Reference) →](./04-common-layer-aggregator.md)
<!-- nav -->

Almost every capability ships as a pair (sometimes a triple):

```
OoBDev.Caching.Abstractions   interfaces, attributes, records           → depends on OoBDev.System.Abstractions
OoBDev.Caching                default implementation + TryAdd… method    → depends on Abstractions + OoBDev.System
OoBDev.Caching.Tests          MSTest + Moq                              → depends on both + OoBDev.TestUtilities
OoBDev.MessageQueueing.Hosting  IHostedService glue (only when needed)  → depends on the implementation
```

Mechanics that make this ergonomic (all in `src/Directory.Build.props`):

* `RootNamespace` strips `.Abstractions`, so the interface `ICachingProvider` lives in namespace `OoBDev.Caching` regardless of which assembly holds it. Consumers change *package* references, never `using` lines.
* `InternalsVisibleTo` (csproj item or `AssemblyInfo.cs`) exposes internals to the matching `*.Tests` project instead of making things public for testing.
* `IsPackable` / `GeneratePackageOnBuild` are inferred from the `.Tests` suffix.

**Rule of thumb observed:** if something is a *contract others implement* (provider, factory, handler, attribute) it goes in `*.Abstractions`; if it is a *default behavior* it goes in the implementation project.

---

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 2 — The Five Source Layers](./02-five-source-layers.md) · [Architecture § 4 — The Common Layer Is an Aggregator (Convention over Reference) →](./04-common-layer-aggregator.md)
<!-- nav -->
