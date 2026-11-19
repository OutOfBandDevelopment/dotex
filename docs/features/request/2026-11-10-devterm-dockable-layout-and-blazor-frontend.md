# Feature request: DevTerm dockable layout (WPF) and Blazor web front end

- **Status:** triaged (WPF docking built; Blazor open)
- **Requested by:** owner (retire dev-term, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm` (WPF shell, Web host)

## Need

Two shell ideas from dev-term, both early stage:

- **Dockable MDI layout (WPF):** sessions and Stream Monitor windows snap and dock around the main window, Visual Studio style, instead of fixed tabs and floating windows. Needs a docking library choice and layout persistence; the dark theme templates need covering.
- **Blazor front end:** the Web host, `/ws` tunnel and `/panel` page exist; the rest is a Blazor UI over the tunnel, tested with several browsers on a real device.

## Source (read-only, to be retired)

dev-term BACKLOG ("Research", "Proposed Ideas"); dotex: [web frontend](../../design/WebFrontend/web-tunnel-blazor-frontend.md), [multi-session UI](../../design/MultiSessionUi/README.md).

## Acceptance criteria

- Given the docked layout, when the app restarts, then the layout is restored.
- Given a Blazor page, then it drives a real device through the tunnel with two browsers at once.

## Existing code checked

Web host exists; no docking library is referenced in `src`.

## Security and operations

The Web host is loopback-only with token authentication; keep that.

## Priority

- **Impact:** convenience; no one is blocked.
- **Needed by:** no date
- **Workaround today:** tabs and floating windows

## Open questions

Docking library: a third-party one (for example AvalonDock) conflicts with the owner's dependency preference; is it acceptable, or build a small one? The two ideas can be split on triage.

## Triage notes

Origin: dev-term BACKLOG (Research, Proposed Ideas).

2026-11-11: triaged. Decided 2026-11-09: AvalonDock (MIT) for the WPF dockable layout, themed in `DarkControls.xaml`, layout saved as XML. The Web host exists; the WPF docking is built since (`DockLayout`). Design: [WebFrontend](../../design/WebFrontend/README.md), [DevTermParity](../../design/DevTermParity/README.md). Backlog: BACKLOG.md, DevTerm.
