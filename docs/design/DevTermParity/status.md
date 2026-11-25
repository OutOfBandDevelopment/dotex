# DevTerm parity: status and remaining work

**Status:** Current, 2026-11-05 · [Parity matrix](README.md) · [Restructure plan](restructure-plan.md) · [Architecture](../../applications/devterm/architecture.md)

This page holds the detail that used to sit in `TODO.md` and `BACKLOG.md`: what is done, and what is left, for the DevTerm merge. The backlog lines link here and stay short.

## Contents

- [Done](#done)
- [WPF shell slices](#wpf-shell-slices)
- [Test gaps](#test-gaps)
- [Other leftovers](#other-leftovers)

## Done

- Restructure steps 0 to 6 ([restructure plan](restructure-plan.md)): telemetry and `ICurrentUserAccessor.User`, pipe-based `IDeviceAdapter`, transports as device adapters, Serial in `System.IO.Ports`, TCP, RFC 2217 and VXI-11 in `System.IO.Tcp`, `OoBDev.DevTerm.Core` merged in `src/Applications`, HID, USBTMC, BLE, MQTT and brokers as `System.IO.*` libraries, and the Wpf, Tui and Web shells in `src/Applications`. Change documents: `docs/changes/2026-10-29.md#telemetry-library-and-user-accessor-2026-10-29`, `2026-10-29.md#pipe-based-device-adapter-2026-10-29`, `2026-10-29.md#transports-become-device-adapters-2026-10-29`, `2026-10-29.md#devterm-web-shell-and-connection-editor-2026-10-29`, `2026-10-29.md#devterm-tui-shell-2026-10-29`.
- Engine: session core, text presenters, configuration, loopback transport, `ConnectionEditorViewModel`, the `OoBDev.DevTerm.Devices` composition bundle (devices compose through DI, so `BundleDevicePlugins` was dropped).
- Web shell and Tui shell (355 Unit tests in the Tui project at the time).
- The `KeyValue` plugin is the built-in `keyvalue` text presenter.

## WPF shell slices

dev-term `DevTerm.Wpf` is about 8,800 lines. Each slice is ported with view model tests. Every xaml window is captured for the user guide and checked in both themes by `UiLayoutReviewTests`; the main window with a generated SCPI control panel open is reviewed too (2026-11-04).

1. **Session workflow.** Done: `SessionViewModel`, tab host, loopback session, sessions from saved profiles, a basic connection editor (transport, serial port and fields, host and TCP port, profile list, save, connect). The editor shows the fields and detected-device pickers of each transport (ports, network scan, HID, USBTMC, BLE with explore). The Refresh button and the file-watcher refresh work and are tested.
2. **Main window menus.** Done: Echo sent, Clear output, Log to file toggle, log path choice, log notes, and a File, Device and View menu bar (checkable Theme submenu) over the existing commands. The View menu has a Software flow control (XON/XOFF) toggle, enabled for TCP sessions.
3. **Device configuration.** Done: `ConfigureDeviceWindow` (disabled until a plugin registers an `IDeviceConfigEditor`; dev-term itself has no Ebyte editor, only the interface; see the future ideas in `BACKLOG.md`), device profiles inside `ConnectionEditorWindow`, `ManifestEditorWindow`, `InstrumentPickerWindow` (fixed device panels only), `ExpressionPickerWindow` (field buttons beside the form). Browse and Save as for the profile zip path are done. (LXI and BLE scans, serial port list).
4. **Monitors.** Done: stream monitor (preview, export, search, filter, sort, open folder, convert, per-device panes), playback (speed, jump, trim, notes, presenter choice), merged log (never untracks closed sessions), the routing window with rule list editing, sample test and history.
5. **Control panel.** Done: `SurfacePanelBuilder`, shell device panel buttons, live displays fed from the structured presenter (owner chose option C, `DynamicModelFactory`, on 2026-10-30), button tooltip previews that follow the field values, interlocks, and the Notes section (the definition description as a final expander in `FormView`; `FormViewModel.NotesExpanded` is remembered through `SectionExpansionState`, as in the Tui). Analysis: [control-panel-spike.md](control-panel-spike.md).
6. **Theme and colour.** Done: `WpfTheme`, dark controls, theme chooser, `ThemeBuilderWindow`, `ThemeSeedPickerWindow`, `ColorPickerWindow`, `SvgPreview`. The choice persists and `--theme` is honoured through `ThemeStartup`.

## Test gaps

- Tui: an interactive run in a real console is the one gap (see BACKLOG.md); serial hardware tests exist (`RealHardwareSerialCliTests`), TCP is covered by `LoopbackDeviceCliIntegrationTests`, and the Web DevLocal suite passed on 2026-11-05 (57 passed, 1 skipped).
- Web: the Playwright flows, screenshot and layout-review tests pass (55 run together); `image-mask sync` with `zones.json` keeps recaptures from churning the committed images.
- Hardware: the USBTMC path (Rigol), LXI (DG1062Z), Korad serial, TDS2024, Z-Stick and the BLE loop-back through the HC-08 pass on the bench (2026-11-04 and 2026-11-05, see `docs/changes/2026-11-04.md`).

## Other leftovers

- `UserFlowTestsBase` and `IFrontEndDriver` live in `OoBDev.DevTerm.TestUtilities` ([plan](test-utilities-plan.md)).
- The Web host keeps its own AsyncAPI document and viewer (owner, 2026-10-30).

## Later: return to the original repository

Owner, 2026-11-25: DevTerm is to move back to its original repository and use the dotex framework packages from NuGet, but not yet, because it is the integration-test bed for the framework while both are built together. Tracked in `BACKLOG.md` (section 2); a migration plan comes first.
