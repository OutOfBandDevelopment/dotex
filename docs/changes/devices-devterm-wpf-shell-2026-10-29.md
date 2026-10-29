# DevTerm WPF shell skeleton (2026-10-29)

`src/Tools/OoBDev.DevTerm.Wpf` (`net10.0-windows`, WinExe) with `OoBDev.DevTerm.Wpf.Tests` (4 Unit tests, passing). First part of phase 10.

## Decisions

- Panels come from `IPanelFactory`; a device registers one in the composition root (`App.OnStartup`).
- The panel list uses buttons bound to a real `OpenPanelCommand` (a separate type with the shell injected), not a selection handler.

## Not done

Device panels (they need the transports of phases 8 and 9), session management, plugin loading, a look at the window on screen. Tracked in `BACKLOG.md`.
