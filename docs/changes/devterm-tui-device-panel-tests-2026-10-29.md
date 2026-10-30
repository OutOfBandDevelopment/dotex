# Tui tests for the device panels restored (2026-10-29)

`TuiTestRunner.BuiltInPanels` and `BuiltInInstruments` now hold the real K8055 and Busylight contributions and the SCPI provider (they were empty while the panels were unported). Ported from dev-term: `CliModeScpiProfileTests` (2), `TuiPluginPanelsTests` (1) and `TuiPanelLayoutTests` (58: K8055 with and without live data, Busylight, every SCPI profile, the 34401A preview).

Tui Unit: 357 passed before the layout tests were added, plus 58 layout, 3 others. Not ported: the manifest-panel layout case (no manifest panel mode in the Tui yet), `ControlPanelModeFitTests`, `ControlPanelScreenshotTests`.
