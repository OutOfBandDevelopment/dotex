# UiDefinitions WPF view (2026-10-29)

`src/Framework/OoBDev.UiDefinitions.Wpf` (`net10.0-windows`) with `OoBDev.UiDefinitions.Wpf.Tests` (8 Unit tests, passing). Second part of phase 7.

## Decisions

- Pure XAML view: data templates per view model, no code-behind logic (owner rule: MVVM with command binding).
- Choice options became bindable items (`ChoiceOptionViewModel` with its command and `IsSelected`) in `OoBDev.UiDefinitions.ViewModels`, so radio buttons and check boxes bind with no converters.

## Not done

Vector plot, and colour picker widgets, the `OoBDev.DevTerm.Wpf` shell, a look at the view on screen. Tracked in `BACKLOG.md`.
