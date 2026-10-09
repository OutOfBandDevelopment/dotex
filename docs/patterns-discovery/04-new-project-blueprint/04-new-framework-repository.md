# Recipe 4 — New Framework Family in a New Repository

<!-- nav -->
[↑ 04 — New Project Blueprint](./README.md) · [← Recipe 3 — New Application](./03-new-application.md) · [Recipe 5 — New Worker or Command-Line Tool →](./05-new-worker-or-cli.md)
<!-- nav -->

To carry the same approach to a new framework (different prefix, different domain).

## Scaffold

**Table 1 — Repository scaffold**

| Item | What to copy or adapt |
|------|-----------------------|
| Layer folders | `Common`, `Framework`, `Extensions`, `ExternalServices`, `Tools`, `Examples` under `src/` |
| `Directory.Build.props` / `.targets` | company, `RootNamespace` rule, packing rules, readme and license packing, test wiring, `SolutionDir` discovery |
| `GitVersion.yml` | versioning mode and branch rules (repository root) |
| `.runsettings` and `TestAssemblyInfo.cs` | test parameters and parallelism |
| `OoBDev.TestUtilities` equivalent | `TestCategories`, `TestContext` extensions and configuration provider |
| `containers/testing` | compose stack, scripts, health checks |
| `.github/workflows` | build, integration, release workflows |
| `.claude/protocols` and `CLAUDE.md` | working agreements, including [documentation rules](../06-design-document-standard.md) |
| `scripts/docs` | validation and index scripts |
| `.vscode` | recommended extensions, PlantUML server, tasks |

## Order of work

1. Create the `System.Abstractions` and `System` equivalents first (providers for time, GUID, current user; `TryAddConfiguredKeyedService`; result envelope; builder pattern). These are the foundation for [patterns 1 to 8, 15 and 16](../02-design-patterns/README.md).
2. Add the first capability using [Recipe 1](./01-new-framework-capability.md) and the first adapter using [Recipe 2](./02-new-vendor-adapter.md).
3. Add the Common roll-up once at least two capabilities exist.
4. Add the example application ([Recipe 3](./03-new-application.md)) as the living proof of composition.

## Decisions to revisit while copying

Before copying blindly, review [Known Warts](../03-practices-and-conventions/07-known-warts.md) and the [industry alternatives](../05-industry-alternatives/README.md): central package management, options validation, analyzers, source-generated logging and keyed-service key constants are the cheapest to get right at the start.

---

<!-- nav -->
[↑ 04 — New Project Blueprint](./README.md) · [← Recipe 3 — New Application](./03-new-application.md) · [Recipe 5 — New Worker or Command-Line Tool →](./05-new-worker-or-cli.md)
<!-- nav -->
