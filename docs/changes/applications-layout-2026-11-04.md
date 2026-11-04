# Applications layout (2026-11-04)

Closes the "Applications layout" backlog item (owner, 2026-10-30; confirmed 2026-11-03). Plan: [applications-layout-plan.md](../design/DevTermParity/applications-layout-plan.md).

## Done

- The 13 DevTerm projects (Core, Devices, Devices.Demo, TestUtilities, Tui, Web, Wpf, Core.TestPlugin and their test projects) moved with `git mv` from `src/Applications/` to `src/Applications/DevTerm/`. Names, namespaces and packages are unchanged.
- The `..\..\Framework|Extensions|HardwareDevices|Tools` references in six `csproj` files gained one `..\`; `OoBDev.slnx` paths and its solution folder (`/Applications/DevTerm/`) were updated.
- Path prefixes updated in the `devterm-constraints` rule, the `hardware-test` skill and the application docs. `docs/changes/*` keep the old paths as history.

## Verified

`dotnet test` on the moved test projects: `Core.Tests` 1065 passed (8 skipped), `Devices.Tests` 77 passed, `Wpf.Tests` 194 passed, `Web.Tests` 104 passed (1 skipped). `Tui.Tests` builds; its tests ran earlier the same day (385 passed) before the move and were not re-run after it. Building the whole `OoBDev.slnx` fails restore with NU1201 (`OoBDev.System.IO.Bluetooth.Windows` is `net10.0-windows` but referenced from net10.0 projects such as `OoBDev.Common`); that is not caused by the move and is not fixed here.
The guide screenshots rewritten by the Web and Wpf runs were reverted (nondeterminism is tracked in `BACKLOG.md`).
