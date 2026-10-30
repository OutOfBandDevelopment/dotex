# WPF field-following previews (2026-10-30)

## What changed

- `ButtonViewModel` takes an optional `ICommandPreview`; its `Sends` and `ToolTip` properties recompute from the current parameter field values and raise a change whenever the form refreshes, so the tooltip follows the fields (before, `Sends:` was computed once with no value).
- `FormViewModel` and `ControlViewModelFactory` forward the preview; `InvokeCommand.Parameter()` is internal so the preview sees exactly what a press would send.
- The shell, the Manifest Editor preview and `FormView.xaml` use it; `DevicePanelPreviews` (a one-shot tooltip rewrite) is gone.
- Tests: three Unit tests in `ButtonPreviewTests` (field edit, description plus Sends, no preview); `DevicePanelTests` asserts `ToolTip`. ViewModels tests 59 pass, Wpf Unit 134 pass.

## Notes section

- `FormViewModel.Notes` and `HasNotes` expose the definition description; `FormView.xaml` shows it as a final "Notes" expander (open by default). Three Unit tests in `FormNotesTests`.

## Not done

- The Notes expander does not remember whether it was collapsed (dev-term kept that per definition).
- The connection editor window already had the Refresh button and the external-change refresh; `ConnectionEditorWindowTests` now covers the window (a profile saved by another store appears without pressing Refresh).
