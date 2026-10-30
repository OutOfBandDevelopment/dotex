# DevTerm WPF converter tools window (2026-10-30)

A **Converter tools...** button in the shell opens `ConverterToolsWindow`, which edits the stream monitor's converter tools (Name, Path with Browse, Arguments, Formats, Output extension, DPI) with Add, Remove, Move up and Move down. It follows the dialog seam: `ConverterToolsDialogViewModel` over `ConverterToolsEditor`, `IConverterToolsDialog` with `WpfConverterToolsDialog`, and `ShowConverterToolsCommand`. The shell loads the tools from `ConverterToolsStore`, and OK saves them only when `Validate()` passes; Cancel saves nothing.

- Tests (`ConverterToolsDialogTests`): accept saves, cancel saves nothing, an incomplete tool is rejected with an error, reorder and remove, and the command is disabled without a dialog. The WPF test project has 50 Unit tests passing.
- Not done: the window was not viewed running.
