# UiDefinitions WPF view (2026-10-29)

`src/Framework/OoBDev.UiDefinitions.Wpf` (`net10.0-windows`) with `OoBDev.UiDefinitions.Wpf.Tests` (8 Unit tests, passing). Second part of phase 7.

## Decisions

- Pure XAML view: data templates per view model, no code-behind logic (owner rule: MVVM with command binding).
- Choice options became bindable items (`ChoiceOptionViewModel` with its command and `IsSelected`) in `OoBDev.UiDefinitions.ViewModels`, so radio buttons and check boxes bind with no converters.

## Vector plot (added 2026-10-29)

`LiveDisplayViewModel` exposes `PlotTrail`, `PlotX`, `PlotY` and `PlotColor` (a 100 by 100 box, origin top left, axes spanning the control range; XYZ uses an oblique projection). `LiveDisplayView` draws the box, axes, trail polyline and a point marker in a `Viewbox`; `CentreConverter` offsets the marker. 5 view model tests and 3 view tests added (30 and 11 in total).

## Not done

Colour picker widgets, a look at the view on screen. Tracked in `BACKLOG.md`.
