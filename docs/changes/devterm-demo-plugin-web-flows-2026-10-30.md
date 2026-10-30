# Demo device plugin and Web flows (2026-10-30)

## What changed

- The Web flow `Panel_SettingTheControlsAndPressingApply` failed because the `demo` panel plugin (`DevTerm.Devices.Demo` in dev-term) had not been migrated; dev-term copied it to `plugins/demo` for every test project.
- New project `OoBDev.DevTerm.Devices.Demo` (panel contribution, control surface, UI definition, plugin module, `plugin.json`, readme), added to `OoBDev.slnx`.
- `OoBDev.DevTerm.Web.Tests` builds it first and copies it to `plugins/demo` (same pattern as the sample plugin in the Core tests).
- Four Unit tests in `OoBDev.DevTerm.Devices.Tests` (availability, definition, the `SET LED=1 LEVEL=7` line, level clamp).
- Result: the Web user-flow class passes (11 tests, Edge through Playwright).
- The 61 failing DevLocal Web screenshot tests failed with `repo root not found`: three test helpers looked for a `DevTerm.slnx` marker that does not exist here and now look for `GitVersion.yml`. Web screenshot and layout tests: 55 pass.

## Not done

- Guide images are not recommitted: a run rewrites them with noise, which the `image-mask sync` routing in the backlog will decide.
- The Wpf and Tui flows do not load the plugin (the Tui flow says it has no panel).
