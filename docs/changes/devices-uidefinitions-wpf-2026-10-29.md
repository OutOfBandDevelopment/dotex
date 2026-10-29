# UiDefinitions WPF view (2026-10-29)

`src/Framework/OoBDev.UiDefinitions.Wpf` (`net10.0-windows`) with `OoBDev.UiDefinitions.Wpf.Tests` (8 Unit tests, passing). Second part of phase 7.

## Decisions

- Pure XAML view: data templates per view model, no code-behind logic (owner rule: MVVM with command binding).
- Choice options became bindable items (`ChoiceOptionViewModel` with its command and `IsSelected`) in `OoBDev.UiDefinitions.ViewModels`, so radio buttons and check boxes bind with no converters.

## Vector plot (added 2026-10-29)

`LiveDisplayViewModel` exposes `PlotTrail`, `PlotX`, `PlotY` and `PlotColor` (a 100 by 100 box, origin top left, axes spanning the control range; XYZ uses an oblique projection). `LiveDisplayView` draws the box, axes, trail polyline and a point marker in a `Viewbox`; `CentreConverter` offsets the marker. 5 view model tests and 3 view tests added (30 and 11 in total).

## Colour picker (added 2026-10-29)

`IColorPicker`, `PickColorCommand`, `ColorPickerViewModel` and its two commands in the view models project; `ColorPickerView`, `ColorPickerWindow` and `WpfColorPicker` in the WPF project. 12 view model tests and 3 view tests added (42 and 14 in total).

## Not done

A look at the colour picker view on screen. Tracked in `BACKLOG.md`.

## Colour picker link option

`FormViewModel` now links a `ColorPickerChoiceOption` (found with `CustomColorChoices`): the choice row's Custom option gets `SelectCustomColorCommand`, which re-applies the last picked colour (`PickColorCommand.Reapply`) or opens the picker when none was picked, and a colour that is not one of the other options shows Custom as selected. The radio template needed no change, because it binds each option item's command. 5 Unit tests added (47 in the view models tests); not looked at on screen.
