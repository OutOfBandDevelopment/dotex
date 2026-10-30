# DevTerm WPF instrument picker (2026-10-30)

Each registered `IInstrumentPanelProvider` gets a button in the shell. It is enabled when the selected session is open, has a tab and the provider accepts its transport and device ids. `InstrumentPickerWindow` (over `InstrumentPickerDialogViewModel`) lists auto-detect, the generic panel and the provider's profiles. The shell then adds the provider's presenter to the session, runs `DetectAsync` for auto-detect (progress and result go to the session output), opens the panel and shows it as the selected panel. A failure is written to the output as "Could not open the instrument panel". The seam is `IInstrumentPickerDialog` with `WpfInstrumentPickerDialog`; the command is `OpenInstrumentCommand`.

- Tests (`InstrumentPickerDialogTests`): choice list, nothing selected, command disabled without a session, chosen profile opens the panel, auto-detect output lines, provider failure reported. The WPF test project has 77 Unit tests passing.
- Not done: the window was not viewed running; the button is bound to the SCPI provider only through DI, not to a menu.
