# OoBDev.Examples.Controls.WinForms

A small Windows Forms app that shows the framework's custom Windows Forms controls (so far `ValidTextBox` from `OoBDev.Extensions.Windows.Forms`): one box per sample pattern (hex byte, whole number, IPv4 address) with a status label fed by `ValidateChanged`, and one box whose `Filter` is typed in a second box. An invalid typed pattern clears the filter instead of throwing.

`SettingsForm` renders the shared `ControlsSampleSettings` through `OoBDev.UiDefinitions.WinForms` (the app opens both forms). A new example app is added for a UI framework whenever it gets custom controls to show (owner request, 2026-11-06). Plan: [migration-plan.md](../../../docs/design/example-ui/migration-plan.md).

```
dotnet run --project src/Examples/OoBDev.Examples.Controls.WinForms
```

## Verified

5 Unit tests (and 2 DevLocal journeys with screenshots) in `OoBDev.Examples.Controls.WinForms.Tests` (the form is created without showing it): one box per sample plus the typed-filter box, the starting validity and colors, a typed filter revalidating its box, and an invalid pattern clearing the filter. The app has not been looked at on screen.
