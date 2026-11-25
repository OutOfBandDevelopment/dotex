# UI definitions and device manifests
**Status:** Built: `OoBDev.UiDefinitions` with ViewModels and the Wpf, WinForms, Tui, Avalonia and Blazor renderers, each with a test project (reconciled with `src` 2026-11-25)

A framework-agnostic, serializable model for declaring a control panel once so every front end renders it generically, plus the no-code device manifest and the control-module model built on it. Moved from the DevTerm design folder on 2026-11-04; code in `OoBDev.DevTerm.UiDefinitions`, `OoBDev.DevTerm.DeviceManifests` and `OoBDev.Scpi.Panels`. Application: [DevTerm](../../applications/devterm/README.md).

| Document | Covers |
|----------|--------|
| [ui-definitions.md](ui-definitions.md) | The UI definition model, value constraints, charts and the forms engine |
| [device-manifests.md](device-manifests.md) | One-file device configuration: command and response schema plus a UI definition |
| [device-control-modules.md](device-control-modules.md) | Plugins that bundle control commands with telemetry decode and presentation |
- [expression-picker-paths-and-cel.md](expression-picker-paths-and-cel.md) - expression picker paths and CEL
- [manifest-editor-expression-builder.md](manifest-editor-expression-builder.md) - expression builder in the manifest editor
- [ksy-importer.md](ksy-importer.md) - binary frames and the .ksy importer
- [format-schema-files.md](format-schema-files.md) - generated JSON Schema files for the formats
- [avalonia-spike-plan.md](avalonia-spike-plan.md), [avalonia-spike-findings.md](avalonia-spike-findings.md) - Avalonia renderer spike for the view models, with captured screenshots in [avalonia-spike/](avalonia-spike/README.md)
