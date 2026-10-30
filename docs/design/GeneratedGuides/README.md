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

## Image comparison

The `image-mask` dotnet tool ([`OoBDev.ImageMask.Cli`](../../../src/Tools/OoBDev.ImageMask.Cli/README.ImageMask.Cli.md), owner suggestion 2026-10-30) normalizes volatile regions with redaction blocks listed in a JSON zones file, so only real changes count.

- **Committed images stay unmasked** (owner, 2026-10-30), so the user guides read naturally. Masks are used only to decide whether a fresh capture differs from the committed image.
- `image-mask sync --new capture.png --committed guide.png --zones zones.json` replaces the committed image only when the two differ once masked, so a run that only moves a clock leaves no churn in git.
- `image-mask diff` fails a test on a real difference and can write a diff image; `image-mask mask` writes a redacted copy for inspection.
- Zones (x, y, width, height, optional name, file-name wildcard and color) live in one `zones.json` beside each guide's images. Text captures (`.txt`) keep their regex masking in the test that writes them.

## Decisions

- A guide per mode is generated even when mostly duplicated (Tui, WPF and Web each get their own); filtering one markdown source per type is not available, so generation does the work.
- Tui screenshot tests write straight into `docs/applications/devterm/user-guide/images` (done).

## Open points

- The WPF journeys run only on a desktop session.
- The screenshot tests still write the capture straight over the committed image; routing them through `image-mask sync` (zones file per guide) is open.
