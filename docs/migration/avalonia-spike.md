# Avalonia spike for the device UI

**Status:** Planned, owner task 2026-10-26. Runs after the first WPF device panel works end to end. Part of phase 7 of the [device decoding migration plan](device-decoding-migration-plan.md). The owner answered Q3 on 2026-10-29; the toolkit pick for the cross-platform renderer waits for this spike.

## Scope

Preference: the most MVVM, with real `ICommand` types where possible. Time-boxed spike on a throwaway branch: render sample `UiDefinition` panels (toggle, slider, choice, numeric, chart, form) through the view-model layer in an Avalonia window. Every command is a real `ICommand` class with injected dependencies (no lambdas, no code-behind). Fixtures have no device, transport or hardware reference, because the UI is separate from the devices.

## Testing

This is a rendering test. WSL with WSLg is enough for the manual Linux check (`DevLocal`). Automated checks use Avalonia headless and a Linux container with a virtual display.

## Check

- .NET 10 support and licence
- runs on Windows and Linux (Linux in a container per the testing rule), and macOS where reachable
- headless rendering for tests
- package size
- how closely the view models map to Avalonia bindings
- what an Eto.Forms or GTK renderer would need by comparison

## Output

A short findings note in `docs/migration/` with a recommendation. The owner decides.
