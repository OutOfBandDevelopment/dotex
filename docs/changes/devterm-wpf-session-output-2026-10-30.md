# DevTerm WPF session output controls (2026-10-30)

Each session in the WPF shell now has an **Echo sent commands** check box and a **Clear output** button.

- `SessionViewModel.EchoSent`: when on, a sent line is listed in the output as `> text`.
- `SessionViewModel.ClearOutput` (`ClearOutputCommand`): empties `OutputLines`.
- Tests: `SessionViewModelTests` (`Send_EchoSentOn_ListsTheSentTextInTheOutput`, `ClearOutput_LinesListed_EmptiesTheOutput`); the WPF test project has 38 Unit tests passing.
- Not done: flow control (XON/XOFF), logging, theme and view menus. The XAML was not viewed running.
