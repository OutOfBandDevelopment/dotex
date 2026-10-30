---
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "src/Directory.*.props"
---

# C# coding standards

- net10.0, nullable enabled, `ImplicitUsings` disabled (explicit `using`), XML docs on public APIs.
- Layers: Common (contracts) <- Framework (depends only on Common) <- Extensions <- ExternalServices <- Applications (`src/Applications`, nothing references it). Adapters reference Abstractions only. Reusable, host-free code is Framework; see rule `layering-and-migration-plans`.
- Pattern: `IService` -> `IServiceProvider` -> `IServiceProviderFactory`; DI via `TryAdd*`; the registration class is always `ServiceCollectionExtensions`.
- Naming: `I{Name}`, `{Name}` (no suffix), `{Name}Provider`, `{Name}Factory`, `{Name}Extensions`. Provider keys are kebab-case constants in the adapter's `{Vendor}Globals`; never a global registry.
- Prefer platform primitives (`TimeProvider`, `Microsoft.Extensions.*`, `[LoggerMessage]`) over hand-built abstractions. Options validation uses `AddValidatedOptions<T>()`. Provider selection uses `TryAddConfiguredKeyedService<T>`.
- Package versions live only in `src/Directory.Packages.props` (central package management). Keep packages current for .NET 10.
- Each project has `README.{Project}.md` (upper-case `README`). Abstractions projects need no tests; a project with testable implementation gets a test library.
- The framework is unreleased: rename or remove outright, with no obsolete forwarders.
- Rejected: third-party IoC/DI containers, third-party logging libraries, Polly. Defaults: SHA-512 hashing, OpenTelemetry for telemetry, MSTest.
- One top-level type per file, named after the type; split a multi-type file when you touch it.
- Configuration goes through `IOptions<T>` with a typed options class per section, bound in the composition root; no raw `IConfiguration["Key"]` reads in services.
- Comments: none that restate the name or the code. Write one only for a why that is not obvious (a constraint, a workaround, a rule). Public APIs keep XML docs.
- No Python for agent work (owner, 2026-10-21 in the `_storage` project, applied here): edit with the Edit and Write tools and use shell, PowerShell or a .NET tool for automation. Python only inside isolated containers.
- Temporary files and folders come from `ITempFileFactory` (owner, 2026-10-30): disposable and cleaned up automatically, never `Path.GetTempFileName` or hand-made temp paths.
- Do not edit generated or build output (`bin/`, `obj/`).
- DevTerm hardware and UI constraints found the hard way (Moq and Span parameters, serial `BaseStream.ReadAsync` cancellation, DTR/RTS defaults, command-line configuration needs `--x true`, Terminal.Gui v2, HID read thread, WPF tests not in parallel) are in the path-scoped rule `devterm-constraints`.
- JavaScript and TypeScript projects are `.esproj` projects (owner, 2026-10-30) so Visual Studio and the normal build pipeline manage them; their tests run with `npm test` from that build.
