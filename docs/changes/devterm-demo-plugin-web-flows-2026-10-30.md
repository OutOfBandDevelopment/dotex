# Demo device plugin and Web flows (2026-10-30)

## What changed

- The Web flow `Panel_SettingTheControlsAndPressingApply` failed because the `demo` panel plugin (`DevTerm.Devices.Demo` in dev-term) had not been migrated; dev-term copied it to `plugins/demo` for every test project.
- New project `OoBDev.DevTerm.Devices.Demo` (panel contribution, control surface, UI definition, plugin module, `plugin.json`, readme), added to `OoBDev.slnx`.
- `OoBDev.DevTerm.Web.Tests` builds it first and copies it to `plugins/demo` (same pattern as the sample plugin in the Core tests).
- Four Unit tests in `OoBDev.DevTerm.Devices.Tests` (availability, definition, the `SET LED=1 LEVEL=7` line, level clamp).
- Result: the Web user-flow class passes (11 tests, Edge through Playwright).

## Not done

- The 61 DevLocal Web screenshot tests were not rerun (they need the guide-image routing work in the backlog).
- The Wpf and Tui flows do not load the plugin (the Tui flow says it has no panel).
