# Controls example guide

The control examples render one settings class (`ControlsSampleSettings`) through the `OoBDev.UiDefinitions` form renderers. Each framework has a journey test (category `DevLocal`) that drives the real controls and saves the screenshots below; the committed image changes only when the masked capture differs (`image-mask sync`). Design: [GeneratedGuides](../../../design/GeneratedGuides/README.md).

## Contents

- [WPF](#wpf)
- [WinForms](#winforms)
- [Tui](#tui)
- [Blazor](#blazor)
- [How the images are made](#how-the-images-are-made)

## WPF

The window starts with the Advanced group hidden. Ticking **Show advanced** reveals it.

![WPF, initial](images/wpf-1-initial.png)

*Figure 1 — WPF, initial state*

![WPF, advanced shown](images/wpf-2-advanced-shown.png)

*Figure 2 — WPF after ticking Show advanced*

## WinForms

Typing in the filter box narrows the rows.

![WinForms, initial](images/winforms-1-initial.png)

*Figure 3 — WinForms, initial state*

![WinForms, filter typed](images/winforms-2-filter-typed.png)

*Figure 4 — WinForms with a filter typed*

The settings dialog is the same class in a second window. Ticking Show advanced reveals the Notes row.

![WinForms settings, initial](images/winforms-3-settings-initial.png)

*Figure 5 — WinForms settings dialog, initial state*

![WinForms settings, advanced shown](images/winforms-4-settings-advanced.png)

*Figure 6 — WinForms settings dialog after ticking Show advanced*

## Tui

Focus **Show advanced** and press Space.

![Tui, initial](images/tui-1-initial.png)

*Figure 7 — Tui, initial state*

![Tui, advanced shown](images/tui-2-advanced-shown.png)

*Figure 8 — Tui after pressing Space on Show advanced*

## Blazor

Every control writes the settings object; the preview card and the bound-model table read it back.

![Blazor, initial](images/blazor-1-initial.png)

*Figure 9 — Blazor, initial state*

![Blazor, changed](images/blazor-2-changed.png)

*Figure 10 — Blazor after choosing High, showing Advanced, typing a note and clearing Enabled*

## How the images are made

| Framework | Test | Capture |
|-----------|------|---------|
| WPF | `OoBDev.Examples.Controls.Wpf.Tests/ControlsJourneyTests.cs` | window rendered off-screen to a bitmap |
| WinForms | `OoBDev.Examples.Controls.WinForms.Tests/ControlsJourneyTests.cs` | `DrawToBitmap` of the form shown off-screen |
| Tui | `OoBDev.Examples.Controls.Tui.Tests/ControlsJourneyTests.cs` | headless screen buffer drawn cell by cell |
| Blazor | `OoBDev.Examples.Controls.Blazor.Tests/ControlsJourneyTests.cs` | the example runs as a process, Edge drives it through Playwright (inconclusive when Edge cannot launch) |

**Table 1 — Journey tests and capture methods**

Shared helpers are in `src/Examples/OoBDev.Examples.TestUtilities`. Run with `dotnet test src/Examples/<project> --filter TestCategory=DevLocal`.
