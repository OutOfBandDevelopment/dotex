# OoBDev.Examples.Controls.Blazor

Control example for the UiDefinitions form language in a Blazor Server host. `ControlsPage` generates the form from `ControlsSampleSettings` (`FormDefinitionGenerator`), binds it with `FormBinding` and renders it with `FormView` from `OoBDev.UiDefinitions.Blazor`.

Run: `dotnet run --project src/Examples/OoBDev.Examples.Controls.Blazor`, then open the printed address.

The page shows the form beside a preview card (name, mode badge, enabled state, level bar, count dots, notes) and a bound-model table, so every control has a visible effect. `Program.cs` calls `MapStaticAssets()`; without it `blazor.web.js` is a 404 and the page never becomes interactive.

Tests: `OoBDev.Examples.Controls.Blazor.Tests` has bUnit Unit tests for the preview and readout, and a `DevLocal` journey test that runs the example as a process and drives it in Edge through Playwright, saving the screenshots for the [controls guide](../../../docs/applications/example/controls-guide/README.md).

`ControlsSampleSettings` is the shared source `src/Examples/OoBDev.Examples.Controls.Shared/ControlsSampleSettings.cs`, linked into the WPF, Tui and Blazor examples.
