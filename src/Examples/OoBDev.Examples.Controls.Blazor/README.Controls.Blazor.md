# OoBDev.Examples.Controls.Blazor

Control example for the UiDefinitions form language in a Blazor Server host. `ControlsPage` generates the form from `ControlsSampleSettings` (`FormDefinitionGenerator`), binds it with `FormBinding` and renders it with `FormView` from `OoBDev.UiDefinitions.Blazor`.

Run: `dotnet run --project src/Examples/OoBDev.Examples.Controls.Blazor`, then open the printed address.

Tests: `OoBDev.Examples.Controls.Blazor.Tests` (bUnit, Unit tier) renders the page and toggles the advanced section. Not run in a browser.

`ControlsSampleSettings` is a copy of the WPF and Tui examples' class; a shared source is a backlog item.
