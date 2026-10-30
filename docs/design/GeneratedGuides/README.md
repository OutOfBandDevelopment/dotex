# Generated guides

**Status:** Draft · **Date:** 2026-10-30 · **Owner decision:** guides and screenshots maintain themselves from test runs, for every front end and API surface.

## Contents

- [Goal](#goal)
- [Design](#design)
- [Where output lands](#where-output-lands)
- [Open points](#open-points)

## Goal

A user or API guide is never edited by hand to stay current. A test run regenerates it, so a change that breaks a journey or an API contract also changes (or fails) the guide in the same commit.

## Design

**Table 1 — Generators**

| Guide | Source of truth | Generator | Runs as |
|-------|-----------------|-----------|---------|
| User guide, Tui | User journeys (`UserFlowTestsBase` derived tests) | Headless Terminal.Gui capture: PNG and text per step | MSTest, category `Unit` or `DevLocal` where a display is needed |
| User guide, WPF | The same journeys through the WPF automation harness | Window render to PNG | MSTest `DevLocal` (needs a desktop) |
| User guide, Web | The same journeys through Playwright | Browser screenshots | MSTest `DevLocal` |
| API guide, REST | OpenAPI document produced by the application | Script renders an operation guide per tag with request and response samples | MSTest `Unit` |
| API guide, messaging | AsyncAPI document | Channel and message guide | MSTest `Unit` |
| Binary format guide | Kaitai Struct (`.ksy`) files | Field table and layout figure | MSTest `Unit` |
| JavaScript or TypeScript front ends | Same journeys | Playwright or the project's runner | `npm test` in an `.esproj` project |

One journey definition drives every front end, so the guide text per step is shared and only the captures differ. A guide step records its capture file; a missing or stale capture fails the generator test.

## Where output lands

`docs/applications/{app-name}/user-guide/` (images in `images/`) and `docs/applications/{app-name}/api-guide/`. Captures are committed with the work that changes them (rule `work-tracking`).

## Open points

- Failing on an image difference needs a tolerance rule (dev-term's `image_diff` script is the reference; see `scripts/devterm/`).
- Tui tests currently write to `artifacts/doc-images` and a person copies the keepers; change them to write the guide folder directly.
- The WPF journeys run only on a desktop session; decide whether the Web and WPF captures share one guide page or have one each.
