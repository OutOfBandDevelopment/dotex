# Build and Project File Conventions

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← Naming and Project Layout](./01-naming-and-layout.md) · [Documentation Practices →](./03-documentation-practices.md)
<!-- nav -->

## Centralized in `Directory.Build.props`

Shared metadata and behavior live once in `src/Directory.Build.props` (and `.targets`), not in each project.

**Table 3 — What the shared build files do**

| Concern | Mechanism |
|---------|-----------|
| Company, copyright, authors, repository URL | properties in `Directory.Build.props` |
| `RootNamespace` | project name with spaces to `_` and `.Abstractions` removed |
| Packing | `IsPackable` and `GeneratePackageOnBuild` true except for `*.Tests` |
| Package readme | `README.{ProjectName without "OoBDev."}.md` if it exists, else `README.md` |
| License | `LICENSE` packed when present one level above the solution |
| Solution root | `SolutionDir` discovered by walking up to five levels for a `*.sln` |
| Tests | `TestAssemblyInfo.cs` compiled into every `*.Tests`; `.runsettings` wired via `RunSettingsFilePath`; results to `TestResults/` |
| Versioning | `GitVersion.MsBuild` (see [CI and versioning](./06-cicd-and-versioning.md)) |
| Central package management | **on** (`ManagePackageVersionsCentrally=true`); versions live only in `src/Directory.Packages.props` |
| Analyzers | present but **commented out** |

## Per-project rules

- `TargetFramework` `net10.0` (the only exceptions are the SQL CLR / DacFx vector projects).
- `Nullable` **enabled**, `ImplicitUsings` **disabled**: every file lists its `using` directives.
- Package versions are written **inline** in each `csproj` (GitVersion and MSTest included).
- Test projects reference `Abstractions` + implementation + `OoBDev.TestUtilities`, and set `IsTestProject` true.
- The Common layer uses `ProjectReference Include` globs with `Remove` filters instead of listing projects, see [the Common aggregator](../01-architecture/04-common-layer-aggregator.md).
- README files are packed with the project and their presence is required (see [documentation](./03-documentation-practices.md)).

## Conditional compilation

`#if DEBUG` makes optional registration parameters required in Debug builds ([pattern 9](../02-design-patterns/09-if-debug-explicit-arguments.md)). Because the compiled API differs per configuration, build the whole solution in one configuration.

## Commands

```
dotnet build src/
dotnet test src/ --filter TestCategory=Unit
dotnet test src/ --collect:"XPlat Code Coverage"
```

---

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← Naming and Project Layout](./01-naming-and-layout.md) · [Documentation Practices →](./03-documentation-practices.md)
<!-- nav -->
