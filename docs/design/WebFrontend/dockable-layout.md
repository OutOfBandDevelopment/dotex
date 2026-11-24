# Web front end: dockable layout

[↑ Web front end](README.md) · [Request](../../features/request/2026-11-10-devterm-dockable-layout-and-blazor-frontend.md) · [WPF `DockLayout`](../DevTermParity/README.md)

**Status:** design; slice 1 (the `DockLayout` model) and slice 2 (`DockHost` at `/dock`, `DockNodeView`, `DockLayoutStore` over `localStorage`, `Activate` and `Close`; 11 unit tests, no component render test because bUnit is not referenced, no live browser pass) built 2026-11-23, slice 3 (HTML5 drag of a tab onto a pane or its edges, resize buttons between panes; `DockLayout.IndexOf`, 12 tests) built the same day, slice 4 built as export and import on the `/dock` page (a JSON box, not the Manifest page: the layout is not a manifest); the generated user guide with screenshots is still open. **Last Updated:** 2026-11-23. Assumptions are marked; the owner can overturn them without losing work (the layout model is independent of the rendering).

## Contents

- [Need](#need)
- [Decisions](#decisions)
- [Architecture](#architecture)
- [Layout document](#layout-document)
- [Testing strategy](#testing-strategy)
- [Slices](#slices)

## Need

The Blazor pages of `OoBDev.DevTerm.Web` (Profiles, Connections, Panel, Playback, Monitor, Routing, Themes, Converters, Manifest, Plugins) are separate routes behind a nav bar. The WPF shell docks sessions and the Stream Monitor around the main window and restores the layout on restart. The web front end needs the same: several panels visible at once (a session beside its monitor), a layout that survives a reload, and no new JavaScript dependency.

## Decisions

| Decision | Choice | Why |
|----------|--------|-----|
| Docking library | None: a small first-party layout model plus CSS grid | Third-party dependencies are avoided (owner-decisions); AvalonDock is WPF only |
| Assumption: scope of v1 | Split panes (rows and columns) with tabs per pane and resizable splitters; no floating windows | Floating windows need pointer capture and z-order handling and add little in a browser tab |
| Layout persistence | One JSON document per user in `localStorage` (key `devterm.layout.v1`), also exportable and importable as a file | Same restore-on-restart behaviour as `DockLayout` without a server store; no personal data on the server |
| Rendering | Blazor components (`DockHost`, `DockSplit`, `DockPane`) render the tree; panels stay the existing page components, hosted through `DynamicComponent` | Reuses every page without a rewrite |
| Drag and drop | HTML5 drag events on tab headers, drop zones on pane edges | Plain browser API, testable with bUnit-style event calls |
| Security | Unchanged: the same loopback-only host and token; the layout holds panel ids and sizes only | A layout cannot name a component outside the registered panel list |

## Architecture

@startuml
skinparam componentStyle rectangle
component "DockHost" as host
component "DockSplit (row or column)" as split
component "DockPane (tab strip)" as pane
component "Panel registry\n(id, title, component type)" as reg
component "DockLayoutStore\n(localStorage)" as store
host --> split
split --> split
split --> pane
pane --> reg : DynamicComponent
host --> store : load, save on change
@enduml

*Figure 1 — Dock components*

`DockLayout` (the model, in `OoBDev.DevTerm.Web`, no UI types) is a tree of nodes. Operations are pure functions on the tree: `Split(pane, edge, panelId)`, `Move(panelId, pane, index)`, `Close(panelId)`, `Resize(split, ratios)`, `Validate(layout, registry)`. The components only render the tree and call these operations, so almost everything is unit testable without a browser.

## Layout document

```json
{
  "version": 1,
  "root": {
    "kind": "split", "direction": "row", "ratios": [0.6, 0.4],
    "children": [
      { "kind": "pane", "active": "panel", "tabs": ["panel", "connections"] },
      { "kind": "pane", "active": "monitor", "tabs": ["monitor"] }
    ]
  }
}
```

Unknown panel ids and an unknown version are dropped on load (`Validate`), falling back to the default layout (one pane with every page as a tab). Ratios are renormalised to sum to 1 with a minimum pane size.

## Testing strategy

| Category | Tests |
|----------|-------|
| Unit | `DockLayout` operations (split, move, close of the last tab collapses the split, resize clamps), JSON round trip, `Validate` drops unknown ids and a bad version |
| Unit | `DockLayoutStore` through an `IJSRuntime` fake: load, save on change, corrupt value falls back |
| Simulate | Component render of a layout with the registry: the right tabs and active panel appear |
| Manual | Two browsers on a real device through the tunnel (the request's acceptance criterion); recorded as a hardware pass |

## Slices

1. `DockLayout` model, operations, JSON and tests.
2. `DockHost`, `DockSplit`, `DockPane` components, default layout, `localStorage` store.
3. Drag and drop between panes and edges; splitter resize.
4. Layout export and import on the Manifest page; user guide with captured screenshots.
