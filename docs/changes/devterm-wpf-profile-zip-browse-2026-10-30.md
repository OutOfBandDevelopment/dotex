# DevTerm WPF: Browse and Save as for the profile zip (2026-10-30)

The connection editor's import and export path got Browse and Save as buttons. `IProfileZipDialogs` (`WpfProfileZipDialogs` over the Windows file dialogs, registered with `TryAddSingleton`) is the seam; `ProfileZipBrowser` exposes the two real `ICommand` types and `WpfConnectionEditorDialog` hands it to the window. Cancelling keeps the typed path.

Verification: WPF Unit suite 120 passed (3 new in `ProfileZipBrowserTests`). The window was not viewed running.
