# UI definitions and device manifests

A framework-agnostic, serializable model for declaring a control panel once so every front end renders it generically, plus the no-code device manifest and the control-module model built on it. Moved from the DevTerm design folder on 2026-11-04; code in `OoBDev.DevTerm.UiDefinitions`, `OoBDev.DevTerm.DeviceManifests` and `OoBDev.Scpi.Panels`. Application: [DevTerm](../../applications/devterm/README.md).

| Document | Covers |
|----------|--------|
| [ui-definitions.md](ui-definitions.md) | The UI definition model, value constraints, charts and the forms engine |
| [device-manifests.md](device-manifests.md) | One-file device configuration: command and response schema plus a UI definition |
| [device-control-modules.md](device-control-modules.md) | Plugins that bundle control commands with telemetry decode and presentation |
