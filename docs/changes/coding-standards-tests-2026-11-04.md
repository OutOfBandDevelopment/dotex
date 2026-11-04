# Coding-standards tests (2026-11-04)

## What changed

- New test project `src/Tools/OoBDev.CodingStandards.Tests` (category `Unit`, in `OoBDev.slnx`). It reads the repository's test sources (`TestSourceScanner`) instead of loading assemblies, so a new test project is covered with no registration.
- `TestCategoryStandardsTests` enforces the `testing` rule: every `[TestClass]` with test methods carries a category from `Unit`, `Simulate`, `Integration`, `DevLocal`, `LiveIntegration`, `Hardware`, and no method ends up in two categories. It replaces dev-term's reflection-based test of the same name; the known-gap list starts empty.
- The first run found two real defects, both fixed: `AttachCliModeTests` carried `DevLocal` on the class and `Unit` on one method (the method ran under both filters), and `ColorPickerWindowTests` had no category (it was in no filtered run).

## Rejected

- dev-term's `DeviceDocsStandardsTests` (a device project with a `*PanelContribution.cs` must be named in a spec and a user guide): the device panels no longer sit in `DevTerm.Devices.*` projects; only the Demo plugin does, so the check would assert nothing. Revisit when the device libraries get their DevTerm panel specs.

## Verification

`dotnet test src/Tools/OoBDev.CodingStandards.Tests`: 3 passed. The two edited test classes were not re-run; the edits only change attributes.

## Not done

The `containers/devterm` wiring into `containers/testing` stays on the backlog.
