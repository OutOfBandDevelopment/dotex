# UiDefinitions view models (2026-10-29)

`src/Framework/OoBDev.UiDefinitions.ViewModels` with `OoBDev.UiDefinitions.ViewModels.Tests` (16 Unit tests, passing). First part of phase 7 of the device decoding plan.

## Decisions

- View models sit over `FormBinding`, so the conversion, validation and condition rules stay in one place for every renderer.
- Commands are real classes (owner rule: MVVM, retestable `ICommand`); the dev-term code-behind `parts.Push` closures are gone.
- A numeric or slider control with `ClampToRange` clamps rather than reports an error; only unparseable text sets `Error`.

## Not done

WPF renderer and shell (`net10.0-windows`), live display view models, Avalonia spike, Terminal.Gui, Blazor (rest of phase 7 and phases 8 to 10). Coverage was not measured.
