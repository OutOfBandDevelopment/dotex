# Feature request: OCR and document text extraction stage (scans and PDFs to Markdown)

- **Status:** done (Docling provider, Office readers, page regions, language hint; retry and park rules are the [work queue](2026-11-11-database-stage-work-queue.md); golden sample built 2026-11-23, [change](../../changes/2026-11-23.md))
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`)
- **Date:** 2026-11-11
- **Area:** `OoBDev.Documents`, `Documents.ExternalTools`, `System.Text.Markdown`

## Need

The catalog turns scanned pages and PDFs into Markdown with page regions, parks documents that fail repeatedly, and keeps the original untouched. A search of the request list and project names found no OCR piece.

## Proposed behavior

A document-to-Markdown provider interface with an OCR implementation (external tool or container), page-level output with region coordinates, language detection, retry and park rules, and a golden sample comparison for quality.

## Acceptance criteria

- Given a scanned PDF, when converted, then Markdown with one section per page is produced and the source is unchanged.
- Given a page that fails three times, when retried, then it is parked and its dependents wait.
- Given the golden sample, when run, then text accuracy is recorded with host and date.

## Existing code checked

- `Documents.ExternalTools` (4 files) is the nearest piece; not read. The catalog notes mention Docling as the converter.

## Source references (NAS storage catalog application)

Paths are in the catalog repository `C:epo_storage`. They are the code to merge from when this feature is taken in; the catalog will drop its copy and consume the framework version afterward (owner 2026-11-11).

- Stages: `src/Storage.Workers/Stages/OcrStageRunner.cs`, `PageOcrStageRunner.cs`, `ExtractStageRunner.cs`, `PdfInfoStageRunner.cs`, `RenderStageRunner.cs`, `SummarizeStageRunner.cs`; helpers `ExtractConverter.cs`, `ExtractPaths.cs`, `ChunkText.cs`, `HtmlMarkdown.cs`, `HtmlText.cs`.
- Container: `containers/docling-ocr/` (README and backup scripts), `containers/renderer/server.py`.
- Documents and pages API: `src/Storage.Api/Documents/`; row model `src/Storage.Catalog.Infrastructure/DocumentRow.cs`.

## Security and operations

Inputs are private files (photos, scans), so nothing is logged beyond identifiers and counts, and no source file is modified. Runs as a worker with a lease and a runtime ceiling (see [`2026-11-11-database-stage-work-queue.md`](2026-11-11-database-stage-work-queue.md)).

## Priority

- **Impact:** the owner wants these features to live in the framework and be reintegrated into the catalog application (owner 2026-11-11).
- **Needed by:** no date
- **Workaround today:** the catalog application owns the implementation.

## Open questions

- Which converter is the default (Docling, Tesseract, another)? Recommended: provider interface first, Docling as the first provider.

## Triage notes

2026-11-12: superset comparison done and the design written: [DocumentToMarkdown](../../design/DocumentToMarkdown/README.md); placement confirmed 2026-11-12: split projects, PdfPig in a separate adapter approved later.

2026-11-11: triaged. Accepted as a backlog idea from the storage and photo-library review; the code named above was not read in full, so the first step is the superset comparison before any design. Face and pet matching also needs an owner decision on personal data. Backlog: BACKLOG.md, Consolidation requests.
