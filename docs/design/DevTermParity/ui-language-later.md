# UiDefinitions as a generic dashboard and form language (later)

**Status:** Idea, owner, 2026-10-29. Nothing is required now. Depends on the UiDefinitions contracts move.

The goal is to write a definition once and get a WPF GUI, a TUI and a web UI from it. Per the `layering-and-migration-plans` rule, start with a spike and a short written result.

## Language shape

Keep the language small and split it into three parts:

- data shape: the model a form binds to, and a JSON schema for it
- actions: buttons and their command ids
- command payloads: parameter fields and expressions

Then add an optional per-control `Source` path and a host-free JSON flattener to `OoBDev.UiDefinitions`, repeating sections for arrays and tables, a tile or grid layout and a data-source refresh. Publish the definition and its JSON schema (an endpoint such as `/ui/definitions/{id}`) so a Vue, React or Angular client can render it, with the DevTerm Web shell as the reference renderer.

## Spikes

Time-boxed. If one gets too complex, native UIs per front end are an acceptable result. Each spike ends with a short written result and a recommendation.

1. Definition-driven Blazor components as a Razor class library that takes a `UiDefinition`, a values source and a command callback.
2. The same components as custom elements for Vue, React and Angular pages.
3. A small JS renderer over the published definition and schema.

**Decision (owner, 2026-11-04):** the device projects get an explicit Example panel per device family (not only DevTerm as the reference), and every control set built for Blazor, WPF, Tui or other hosts gets a small control-example app (controls only; the framework itself is demoed by the existing Example app).
