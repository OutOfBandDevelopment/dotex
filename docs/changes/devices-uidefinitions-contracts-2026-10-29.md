# UiDefinitions contracts move to System.Abstractions (2026-10-29)

Owner decision (restructure plan, "Plugins and device panels"): pure contracts (DTOs, interfaces, enumerations) live in `OoBDev.System.Abstractions`, logic stays in the feature library.

## What moved

- `src/Framework/OoBDev.System.Abstractions/UiDefinitions/` now holds `IControlSurface`, `ICommandPreview`, `CommandPreviewFormat`, `IStructuredPresenter` (from DevTerm Core) and the `UiDefinition` model: `UiDefinition`, `UiSection`, `UiControl`, `UiCondition`, `ValueConstraint`, `ValueKind`, `ValueValidationResult` and the control types with their enums. The namespace stays `OoBDev.UiDefinitions`.
- `OoBDev.UiDefinitions` keeps the logic (expressions, validation, states, serializer, forms) and references `OoBDev.System.Abstractions`.

## Consequence: namespace shadowing

`OoBDev.System.*` shadows `System.*` inside any `OoBDev.*` namespace that sees `OoBDev.System.Abstractions`. Fixed with `global::System.*` in `Expression`, `DynamicModelFactory` and the UiDefinitions tests. The XAML views cannot use `global::` in generated code, so the WPF views moved to the namespace `UiDefinitionsWpf` (the assembly is still `OoBDev.UiDefinitions.Wpf`), the same way `DevTerm.Wpf` uses `DevTermApp`; the WPF tests use `UiDefinitionsWpf.Tests`.

## Verified

Unit: UiDefinitions 194, ViewModels 53, Wpf 14, Scpi.Panels 6, DevTerm.Wpf 8, DevTerm Core 1037 (8 skipped), all passing; Tui and Web runs recorded in the commit series.

## Device configuration contracts

`IDeviceConfigEditor`, `DeviceConfigTarget`, `DeviceConfigIssue`, `DeviceConfigChange` and `DeviceConfigWriteResult` moved from DevTerm Core to `OoBDev.System.Abstractions/IO/Devices` (namespace `OoBDev.System.IO.Devices`), so device libraries can ship their configuration editors. `DeviceConfigSession` (logic) stays in Core. Unit: Core 1035, Tui 355, Web 43, all passing.

## Scpi.Panels folded into Scpi

`OoBDev.Scpi.Panels` and its tests moved into `OoBDev.Scpi` (`Panels/`) and `OoBDev.Scpi.Tests` (`Panels/`); the namespace `OoBDev.Scpi.Panels` is unchanged, `OoBDev.Scpi` now references `OoBDev.UiDefinitions`, and the two projects left `OoBDev.slnx`. Scpi Unit tests: 52 passing (46 plus the 6 panel tests). `.Panels` is the sub-namespace convention for every protocol's UI definition (K8055 and Busylight are next, see the backlog).

## KeyValue plugin merged into Core

`KeyValuePresenter` is now a built-in text presenter (`OoBDev.DevTerm.Presenters.Text`, registered by `TryAddTextPresenters`, name `keyvalue`); `OoBDev.DevTerm.Plugins.KeyValue` and its test project are gone, and its tests live in `Core.Tests/Presenters/Text`. The Tui connection form and the registration test list `keyvalue`. `OoBDev.DevTerm.Plugins.Sample` stays a separate assembly (the loader tests need a real external plugin); renaming it to a test fixture is in the backlog. Unit: Core 1039, Tui `ConfigureModeTests` 35.
