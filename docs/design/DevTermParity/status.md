# DevTerm parity: status and remaining work

**Status:** Current, 2026-10-30 · [Parity matrix](README.md) · [Restructure plan](restructure-plan.md) · [Architecture](architecture.md)

This page holds the detail that used to sit in `TODO.md` and `BACKLOG.md`: what is done, and what is left, for the DevTerm merge. The backlog lines link here and stay short.

## Contents

- [Done](#done)
- [WPF shell slices](#wpf-shell-slices)
- [Test gaps](#test-gaps)
- [Other leftovers](#other-leftovers)

## Done

- Restructure steps 0 to 6 ([restructure plan](restructure-plan.md)): telemetry and `ICurrentUserAccessor.User`, pipe-based `IDeviceAdapter`, transports as device adapters, Serial in `System.IO.Ports`, TCP, RFC 2217 and VXI-11 in `System.IO.Tcp`, `OoBDev.DevTerm.Core` merged in `src/Applications`, HID, USBTMC, BLE, MQTT and brokers as `System.IO.*` libraries, and the Wpf, Tui and Web shells in `src/Applications`. Change documents: `docs/changes/telemetry-user-accessor-2026-10-29.md`, `device-adapter-pipes-2026-10-29.md`, `transports-as-device-adapters-2026-10-29.md`, `devterm-web-shell-2026-10-29.md`, `devterm-tui-shell-2026-10-29.md`.
- Engine: session core, text presenters, configuration, loopback transport, `ConnectionEditorViewModel`, the `OoBDev.DevTerm.Devices` composition bundle (devices compose through DI, so `BundleDevicePlugins` was dropped).
- Web shell and Tui shell (355 Unit tests in the Tui project at the time).
- The `KeyValue` plugin is the built-in `keyvalue` text presenter.

## WPF shell slices

dev-term `DevTerm.Wpf` is about 8,800 lines. Each slice is ported with view model tests. Windows were never viewed running.

1. **Session workflow.** Done: `SessionViewModel`, tab host, loopback session, sessions from saved profiles, a basic connection editor (transport, serial port and fields, host and TCP port, profile list, save, connect). Left: the other per-transport fields, device pickers, profile-button refresh.
2. **Main window menus.** Done: Echo sent, Clear output, Log to file toggle, log path choice, log notes, and a File, Device and View menu bar (checkable Theme submenu) over the existing commands. Left: Software flow control toggle (no session option for it yet).
3. **Device configuration.** Done: `ConfigureDeviceWindow` (disabled until a plugin registers an `IDeviceConfigEditor`; the Ebyte editor is not ported), device profiles inside `ConnectionEditorWindow`, `ManifestEditorWindow`, `InstrumentPickerWindow` (fixed device panels only), `ExpressionPickerWindow` (field buttons beside the form). Browse and Save as for the profile zip path are done. Left: detected-device pickers (LXI and BLE scans, serial port list).
4. **Monitors.** Done: stream monitor (preview, export, search, filter, sort, open folder, convert, per-device panes), playback (speed, jump, trim, notes, presenter choice), merged log (never untracks closed sessions), a basic routing window. Left: routing rule list editing and history.
5. **Control panel.** Done: `SurfacePanelBuilder`, shell device panel buttons, live displays fed from the structured presenter (owner chose option C, `DynamicModelFactory`, on 2026-10-30), button tooltip previews, interlocks. Left: field-following previews and the Notes section. Analysis: [control-panel-spike.md](control-panel-spike.md).
6. **Theme and colour.** Done: `WpfTheme`, dark controls, theme chooser, `ThemeBuilderWindow`, `ThemeSeedPickerWindow`, `ColorPickerWindow`, `SvgPreview`. The theme does not persist until the app passes a preferences store to `ActiveTheme.Initialize`.

## Test gaps

- Tui: `ControlPanelScreenshotTests`, the manifest-panel layout test, serial and TCP real-hardware tests, loopback, stream-monitor and chart screenshots, an interactive Tui run.
- Web: Playwright DevLocal flows were not run; the screenshots still write into `docs/user-guide/images`. 61 DevLocal Web screenshot tests fail when run unfiltered.
- `LxiDeviceScanner` socket scan: deferred, because the scan broadcasts to UDP 111 on every interface and a fake responder needs `LxiDiscovery` to take a target endpoint first.
- Hardware tests for the USBTMC path were skipped (no device attached).

## Other leftovers

- `UserFlowTestsBase` and `IFrontEndDriver` live in `OoBDev.DevTerm.TestUtilities` ([plan](test-utilities-plan.md)); the Web `Panel_SettingTheControlsAndPressingApply` Playwright flow failed once after the move (locator count 0), not compared with before the move.
- The Web host keeps its own AsyncAPI document and viewer (owner, 2026-10-30).
- Cleanup to mention to the owner: the removable Docker volume `oobd-aspire-sqlserver`, and untracked `bin` and `obj` folders under `src/Extensions/OoBDev.DevTerm.*` and `OoBDev.Scpi.Panels*`.
