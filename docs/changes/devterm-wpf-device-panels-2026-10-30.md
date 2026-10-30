# DevTerm WPF device panels (2026-10-30)

WPF slice 5, first part: `SurfacePanelBuilder` (DevTerm Core, `Panels`) turns a plugin's `UiDefinition` and its `IControlSurface` into a form-ready `SurfacePanel`, using a generated model (`DynamicModelFactory`) as the SCPI panels do (Q4 option C, no Framework change). Toggles, sliders, numbers, choices and text fields send their value to the surface when it changes (toggle as "1"/"0"); a button is an `ICommand` that sends `CommandId ?? Id`. Indicators, bar graphs, charts and vectors are left out of the form until display view models exist.

The shell lists the contributed `IDevicePanelContribution` panels (`DevicePanels`) as buttons; `OpenDevicePanelCommand` is enabled when `DevicePanels.UnavailableReason` allows it for the selected session (connected, suitable transport, presenter selected) and shows the panel in the main area. `PanelViewModel` now disposes what it owns, so closing a panel detaches the surface subscription.

Tests: `SurfacePanelBuilderTests` (3 Unit: displays dropped, toggle and choice sends, button send) and `DevicePanelTests` (1 Unit: a connected session opens the panel and a toggle reaches the surface); WPF test project 33 passed. Not yet done: live display controls, confirm messages, colour buttons, command preview tooltips, `EnabledWhen` interlocks, notes; the XAML was not viewed running.
