# DevTerm WPF device manifest picker (2026-10-30)

A **Device manifest...** button in the shell opens `ManifestPickerWindow`: the manifests discovered by `InstalledManifests.Discover()` (user, then installed), or a typed or browsed path to a manifest file, folder or zip. `ManifestPickerDialogViewModel.TryLoad()` loads the choice with `DeviceManifestLoader` and compiles the response patterns, so a bad manifest is reported inline and the picker stays open. A chosen manifest opens as a panel on the selected session: `ManifestPanel.Attach`, `SurfacePanelBuilder`, command previews, and a `DevicePanelFeed` over the reply presenter; replacing the panel disposes both the surface and the attached presenter (`PanelDisposal`). The seam is `IManifestPickerDialog` with `WpfManifestPickerDialog`.

- Tests (`ManifestPickerDialogTests`): typed path, listed entry, nothing picked, missing path, the shell opening the panel, and the command disabled without a session. The WPF test project has 56 Unit tests passing.
- Not done: `ManifestEditorWindow` (the editor's path-only open mode), and the window was not viewed running.
