# Feature request: DevTerm rendering presenters and raster export service

- **Status:** new
- **Requested by:** owner (retire dev-term, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm` (presenters, Stream Monitor)

## Need

HP-GL converts to SVG and draws in WPF and Web; PostScript and PCL draw in WPF and Web only through an installed Ghostscript or GhostPCL. The Tui cannot draw SVG, and export to PNG or JPG has no shared service.

## Proposed behavior

- Rendering presenters for HP-GL, PostScript and PCL (plots, telemetry) in every front end where possible; in the Tui decide whether to open the converted file externally (a terminal cannot show a picture).
- Decision 2026-10-13: raster export (SVG to PNG and JPG) is one core service every front end calls.
- Rejected 2026-10-03, keep rejected unless asked: a built-in PCX decoder and PCL raster preview.

## Source (read-only, to be retired)

dev-term `docs/design/presenters.md`, `docs/design/features/stream-content-detection.md`; dotex copies: [presenters](../../design/Presenters/presenters.md), [raster export](../../design/Presenters/raster-export.md), [stream tools](../../design/StreamTools/stream-content-detection.md).

## Acceptance criteria

- Given a captured plot, then each front end shows it or says why it cannot.
- Given an SVG, then the export service returns PNG or JPG of a requested size.

## Existing code checked

HP-GL to SVG exists (`OoBDev.Documents.Hpgl`, Stream Monitor auto-convert). Searched `src` for `IRasterExporter` and `RasterExport`: nothing.

## Security and operations

Rendering external files runs external tools: pass arguments safely, bound time and size.

## Priority

- **Impact:** plotter and oscilloscope hardcopy workflows.
- **Needed by:** no date
- **Workaround today:** open the saved SVG elsewhere

## Open questions

Which raster library fits the dependency rules (no large native package)?

## Triage notes

Origin: dev-term BACKLOG (Plugin architecture, Stream Monitor leftovers, UX decisions).
