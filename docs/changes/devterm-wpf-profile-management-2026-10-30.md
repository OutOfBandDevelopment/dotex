# DevTerm WPF profile management (2026-10-30)

`ConnectionEditorWindow` now covers what dev-term's `DeviceProfilesWindow` did: a multi-select profile list (mirrored into `SelectedProfileNames`), Delete selected, Refresh, Export selected, Export all, a save-name box, the import/export zip path with Import and Replace all from zip, and an automatic refresh when the profiles directory changes. `ConnectionEditorPrompts.Wire` makes multi-delete and replace-all ask first through `IConfirmPrompt`; `WpfConnectionEditorDialog` wires it to `WpfConfirmPrompt` and asks Replace/Rename/Skip on import conflicts with a message box.

- Tests (`ConnectionEditorPromptsTests`): both confirmations carry the names or counts. The WPF test project has 79 Unit tests passing.
- Not done: Browse file dialogs for the zip path (type the path), the detected-device pickers; the window was not viewed running.
