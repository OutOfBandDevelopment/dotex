# Applications layout plan

Owner decision (2026-11-03): group the DevTerm projects under `src/Applications/DevTerm/`, like ExternalServices groups its projects; names, namespaces and packages stay. `TestUtilities` and `TestPlugin` move with them (DevTerm-specific: they reference `OoBDev.DevTerm.Core`). Rule: [layering-and-migration-plans](../../../.claude/rules/layering-and-migration-plans.md).

## Projects

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| `OoBDev.DevTerm.Core` | `src/Applications/OoBDev.DevTerm.Core` | `src/Applications/DevTerm/OoBDev.DevTerm.Core` | unchanged | library |
| `OoBDev.DevTerm.Core.Tests` | `src/Applications/OoBDev.DevTerm.Core.Tests` | `src/Applications/DevTerm/OoBDev.DevTerm.Core.Tests` | unchanged | test library |
| `OoBDev.DevTerm.Core.TestPlugin` | `src/Applications/OoBDev.DevTerm.Core.TestPlugin` | `src/Applications/DevTerm/OoBDev.DevTerm.Core.TestPlugin` | unchanged | test plugin |
| `OoBDev.DevTerm.Devices` | `src/Applications/OoBDev.DevTerm.Devices` | `src/Applications/DevTerm/OoBDev.DevTerm.Devices` | unchanged | library |
| `OoBDev.DevTerm.Devices.Demo` | `src/Applications/OoBDev.DevTerm.Devices.Demo` | `src/Applications/DevTerm/OoBDev.DevTerm.Devices.Demo` | unchanged | library |
| `OoBDev.DevTerm.Devices.Tests` | `src/Applications/OoBDev.DevTerm.Devices.Tests` | `src/Applications/DevTerm/OoBDev.DevTerm.Devices.Tests` | unchanged | test library |
| `OoBDev.DevTerm.TestUtilities` | `src/Applications/OoBDev.DevTerm.TestUtilities` | `src/Applications/DevTerm/OoBDev.DevTerm.TestUtilities` | unchanged | test library, not packed |
| `OoBDev.DevTerm.Tui` | `src/Applications/OoBDev.DevTerm.Tui` | `src/Applications/DevTerm/OoBDev.DevTerm.Tui` | unchanged | application |
| `OoBDev.DevTerm.Tui.Tests` | `src/Applications/OoBDev.DevTerm.Tui.Tests` | `src/Applications/DevTerm/OoBDev.DevTerm.Tui.Tests` | unchanged | test library |
| `OoBDev.DevTerm.Web` | `src/Applications/OoBDev.DevTerm.Web` | `src/Applications/DevTerm/OoBDev.DevTerm.Web` | unchanged | application |
| `OoBDev.DevTerm.Web.Tests` | `src/Applications/OoBDev.DevTerm.Web.Tests` | `src/Applications/DevTerm/OoBDev.DevTerm.Web.Tests` | unchanged | test library |
| `OoBDev.DevTerm.Wpf` | `src/Applications/OoBDev.DevTerm.Wpf` | `src/Applications/DevTerm/OoBDev.DevTerm.Wpf` | unchanged | application |
| `OoBDev.DevTerm.Wpf.Tests` | `src/Applications/OoBDev.DevTerm.Wpf.Tests` | `src/Applications/DevTerm/OoBDev.DevTerm.Wpf.Tests` | unchanged | test library |

The untracked folders `OoBDev.DevTerm.Plugins.KeyValue` and `.Tests` hold only `bin` and `obj` (no project files); they are not moved and may be deleted locally.

## References that change

| Where | Change |
|-------|--------|
| `..\..\Framework\...`, `..\..\Extensions\...`, `..\..\HardwareDevices\...`, `..\..\Tools\...` in the DevTerm `csproj` files (Core, Devices, TestUtilities, Tui, Web, Wpf) | one more `..\`: `..\..\..\Framework\...` |
| `..\OoBDev.DevTerm.*` sibling references | unchanged (siblings stay siblings) |
| `src/OoBDev.slnx` | 13 `Project Path` entries gain `DevTerm/`; the solution folder `/Applications/` becomes `/Applications/DevTerm/` |
| `src/Directory.Build.props` `SolutionDir` | no change: it probes up to five levels, and the new depth is three |
| `TuiReview.FindRepoRoot` | no change: it walks up to `CLAUDE.md` |
| `.claude/rules/devterm-constraints.md` (`paths` glob), `.claude/skills/hardware-test/SKILL.md` (`dotnet run --project`) | path prefix `src/Applications/DevTerm/` |
| `docs/applications/README.md`, `docs/applications/devterm/README.md`, `docs/design/DevTermParity/restructure-plan.md`, `test-utilities-plan.md` | path prefix; `docs/changes/*` stay as history |

## Steps

1. `git mv` each folder (one commit with the path rewrites, no content changes).
2. Fix the relative references above; update `OoBDev.slnx`, rule, skill and docs.
3. `dotnet build src/OoBDev.slnx`, then the Unit tests of the moved test projects; check that `docs/applications/devterm/user-guide` stays unchanged after a Tui run (restore if only nondeterministic files differ).
4. Change document, index line, remove the `BACKLOG.md` item; commit and push `dev/example-ui-tests`.

## Risks

- Moving a folder with open `bin`/`obj` output: delete them first (they are ignored by git).
- A path baked into a script outside the searched set (`scripts/`, `containers/`): the grep found none; the build and test run is the check.
