# Feature request: Document page operations (render, split, merge)

- **Status:** new
- **Requested by:** owner (SharedFramework duplication review, 2026-11-25)
- **Date:** 2026-11-25
- **Area:** `OoBDev.Documents` and a new PDF adapter under `src/ExternalServices`

## Need

Documents can be converted, packed, OCRed and turned into Markdown, but a binary PDF cannot be rendered to page images or thumbnails, split by page or bookmark, or merged with a table of contents and page numbers.

## Proposed behavior

Add `IDocumentRenderer` (page to PNG or JPEG, thumbnail, optional watermark), `IDocumentSplitter` (by page range, size or bookmark) and `IDocumentComposer` (merge, table of contents, page numbers) over an adapter, following the Rendering, Splitting and Composition designs in `docs/design/DocumentServices`. Example: split a 40-page PDF into 4 files of 10 pages with the source metadata kept.

## Acceptance criteria

- Given a PDF and a page range, when split, then each output has exactly those pages and the source metadata.
- Given two PDFs, when merged with a table of contents, then the pages appear in order with page numbers.
- Given a PDF, when rendered at a size, then one image per requested page has that size.

## Existing code checked

`ChunkText` and `MarkdownPages` split text only; `Documents.Markdown` reads text; `IDocumentConversion` converts formats with Tika and wkhtmltopdf. None handles PDF pages. See [the duplication review](../../design/SharedFramework/duplication-review.md).

## Security and operations

Rights `documents.render` and `documents.compose` on any API; files are processed in `ITempFileFactory` scratch space; CPU cost grows with page count.

## Priority

- **Impact:** no PDF page work without a hand-rolled tool.
- **Needed by:** no date.
- **Workaround today:** external tools through `ExternalToolConversionHandler`.

## Open questions

Which PDF library. Options: PDFsharp (in process), PdfPig (read-only, needs separate approval), an external tool in a container. Recommendation: an external tool in a container, the way Docling and wkhtmltopdf are used.

## Triage notes

Raised from the duplication review 2026-11-25; not yet triaged.
