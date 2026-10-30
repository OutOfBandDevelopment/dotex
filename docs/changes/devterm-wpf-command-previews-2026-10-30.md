# DevTerm WPF device panel command previews (2026-10-30)

When a device panel's control surface implements `ICommandPreview`, `ShellViewModel.OpenDevicePanel` appends `Sends: <preview>` to the tooltip (`Description`) of every command button (`DevicePanelPreviews`).

- Previews use no field value (`null`), so a button whose parameter comes from a field shows the command without it.
- Test: `OpenDevicePanelCommand_SurfaceWithPreview_PutsTheSendsTextInTheButtonTooltip`; the WPF test project has 39 Unit tests passing.
- `EnabledWhen` interlocks are verified at shell level (`OpenDevicePanelCommand_EnabledWhenToggleOn_EnablesTheButton`, 40 tests passing).
- Left: previews that follow field values and the Notes section.
