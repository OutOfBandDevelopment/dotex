---
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "src/Directory.*.props"
---

# C# coding standards

- net10.0, nullable enabled, `ImplicitUsings` disabled (explicit `using`), XML docs on public APIs.
- Layers: Common (contracts) <- Framework (depends only on Common) <- Extensions <- ExternalServices. Adapters reference Abstractions only.
- Pattern: `IService` -> `IServiceProvider` -> `IServiceProviderFactory`; DI via `TryAdd*`; the registration class is always `ServiceCollectionExtensions`.
- Naming: `I{Name}`, `{Name}` (no suffix), `{Name}Provider`, `{Name}Factory`, `{Name}Extensions`. Provider keys are kebab-case constants in the adapter's `{Vendor}Globals`; never a global registry.
- Prefer platform primitives (`TimeProvider`, `Microsoft.Extensions.*`, `[LoggerMessage]`) over hand-built abstractions. Options validation uses `AddValidatedOptions<T>()`. Provider selection uses `TryAddConfiguredKeyedService<T>`.
- Package versions live only in `src/Directory.Packages.props` (central package management). Keep packages current for .NET 10.
- Each project has `README.{Project}.md` (upper-case `README`). Abstractions projects need no tests; a project with testable implementation gets a test library.
- The framework is unreleased: rename or remove outright, with no obsolete forwarders.
- Rejected: third-party IoC/DI containers, third-party logging libraries, Polly. Defaults: SHA-512 hashing, OpenTelemetry for telemetry, MSTest.
- Do not edit generated or build output (`bin/`, `obj/`).
