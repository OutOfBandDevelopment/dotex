# Feature request: Document extraction extras (embedded images, page selection, OCR engine choice)

- **Status:** new
- **Requested by:** owner (SharedFramework duplication review, 2026-11-25)
- **Date:** 2026-11-25
- **Area:** `OoBDev.Documents.Markdown`, `OoBDev.Documents.Abstractions` (`IDocumentOcr`)

## Need

Text, tables and metadata are extracted, but embedded images cannot be pulled out, a caller cannot ask for only some pages, and OCR is fixed to Docling with no preprocessing.

## Proposed behavior

Add an extraction option for page ranges, an image extraction result (bytes, page, position) and an OCR engine selector with an optional preprocessing step (deskew, threshold). Example: extract pages 3 to 5 as Markdown together with the images on them.

## Acceptance criteria

- Given a document and pages 3 to 5, when extracted, then only those pages are returned.
- Given a document with two embedded images, when image extraction is on, then two image results carry their page numbers.
- Given two registered OCR engines, when one is named, then that one runs.

## Existing code checked

`Documents.Markdown` readers (HTML, Office, xlsx, pptx), `ApacheTikaClient`, `IDocumentOcr`, `IDocumentLayoutOcr`, `DoclingOcr` and `SegmentedOcr`: none returns images or takes a page range. See [the duplication review](../../design/SharedFramework/duplication-review.md).

## Security and operations

Same rights as extraction; image extraction can enlarge memory use, so options cap the size and count.

## Priority

- **Impact:** retrieval over scanned or image-heavy documents is weaker.
- **Needed by:** no date.
- **Workaround today:** none.

## Open questions

Which second OCR engine. Recommendation: a Tesseract container, since Docling already runs as one.

## Triage notes

Raised from the duplication review 2026-11-25; not yet triaged.
