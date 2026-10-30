# DevTerm WPF manifest editor (2026-10-30)

An **Edit manifest...** button opens `ManifestEditorWindow` over `ManifestEditorDialogViewModel`, which wraps the shared `ManifestEditorViewModel` (the same editing logic the terminal front end uses). The window has a toolbar (New, Open, Save, Save as, Check, Undo, Redo, Import .ksy, Use recording), an outline of the manifest parts with Add, Remove, Up and Down, the selected part's form and a live preview of the panel. Preview controls send nothing; the last one pressed shows "Would send: ...". Read-only indicator fields of a form (for example a command's "Sends" text) show as a readout under the form, because the form view models have no indicator row.

Seams: `IManifestEditorDialog` with `WpfManifestEditorDialog`, and `IManifestFileDialogs` with `WpfManifestFileDialogs` (open, save, .ksy and recording pickers). Unsaved-change and overwrite prompts go through `IConfirmPrompt`. Commands are `ManifestEditorCommand` (state-aware, refreshed on selection and edits) and `ShowManifestEditorCommand`.

- Tests (`ManifestEditorDialogTests`): outline and preview on a new manifest, selecting a part shows a form, add enables undo, undo restores the outline, Save as writes the picked path, a cancelled picker writes nothing, the shell shows the editor, the button is disabled without a dialog. The WPF test project has 87 Unit tests passing.
- Not done: the window was not viewed running; the expression pickers (`ExpressionPickerWindow` and the per-field text pickers) are a separate item.
