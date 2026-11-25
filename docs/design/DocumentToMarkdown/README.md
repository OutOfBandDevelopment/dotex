# Document to Markdown and OCR

Status: **design written 2026-11-12; placement confirmed by the owner 2026-11-12 (Framework project without a PDF package, a Docling adapter, PdfPig only in a separate adapter approved later), first slice built 2026-11-16 (Framework project, contracts, Docling adapter, docling-serve in the integration stack with an Integration test); HTML built 2026-11-16 (`IFileMarkdownReader` seam, ReverseMarkdown reader in `OoBDev.MysticMind`); word-processing and e-book formats built 2026-11-16 (`OoBDev.Documents.Markdown.Office` composes the registered Tika and MysticMind handlers; unit tests only, no run against the Tika container); spreadsheets, presentations and the PdfPig adapter not built.** Request: [OCR and document text extraction](../../features/completed/2026-11-11-ocr-document-text-extraction.md). Related existing designs: [OcrService](../DocumentServices/OcrService/requirements.md) (images, context based, Tesseract and cloud providers) and [ExtractionService](../DocumentServices/ExtractionService/requirements.md) (text and metadata).

## Contents

1. [Need](#need)
2. [Superset comparison](#superset-comparison)
3. [Design](#design)
4. [Migration plan](#migration-plan)
5. [Testing strategy](#testing-strategy)
6. [Not in the first slice](#not-in-the-first-slice)

## Need

Turn documents (PDF, office formats, text, scans) into page-anchored Markdown with front matter, send scanned PDFs to an OCR service, resume long documents in page ranges, and never change the source file.

## Superset comparison

**Table 1 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| dotex `OoBDev.Documents.ExternalTools` | Runs registered command line converters as conversion handlers (Ghostscript style) | Possible host for a Tesseract or `pdftotext` provider; no Markdown model |
| dotex `DocumentServices` designs | `IOcrService` (image to text, confidence, languages, providers) and `IExtractionService`, all design only, no code | Keep as the image-level contract; the catalog work is the document-level layer above it |
| Catalog `ExtractConverter` (72 lines) | Text, PDF (PdfPig text per page, under 40 characters per page means "needs OCR"), office, RTF, EPUB, HTML to Markdown with front matter and `<a id="pN">` anchors | Base for a `DocumentMarkdownConverter` |
| Catalog `IDocumentOcr` and `DoclingOcr` (80 lines) | Docling-serve client: post, poll in 20 s waits, fetch Markdown; heavy options then a light retry; page mark `<<<PAGE>>>` | Base for an OCR provider; plain `HttpClient`, so an adapter under ExternalServices |
| Catalog `PdfSegments` and the segment loop in `OcrStageRunner` | Plan page ranges, write a piece, keep each finished piece's text so a retry resumes, pad or merge pages so numbers stay true | Base for `PdfSegmenter` and a resumable `SegmentedOcr`; needs a PDF page splitter |
| Catalog `OcrMarkdown`, `ExtractPaths`, `ChunkText`, `HtmlMarkdown`, `HtmlText` | Front matter writer, content-hash paths, chunking, HTML to Markdown | Base for the Markdown writer and HTML converters |
| Catalog stage runners, work queue, twin reuse | Database and lease wiring | Catalog-specific; see [work queue](../../features/completed/2026-11-11-database-stage-work-queue.md) |

The request's open question (default converter) is answered as recommended: a provider interface first, Docling as the first provider, Tesseract through `ExternalTools` second.

## Design

- `IDocumentOcr` (`Extractor`, `RecognizeAsync(path)` returning Markdown with a page mark between pages) with the catalog's failure contract: transport faults and timeouts retry, `OcrFailedException` is final. It sits beside, not inside, `IOcrService`, which works on images.
- `DocumentMarkdownConverter` returns a status (converted, unchanged, needs-ocr, not-pdf, not-office, empty) and writes Markdown with front matter; the source is only read.
- `SegmentedOcr` resumes long documents from a segment folder and keeps page numbers true.
- Page regions with coordinates (request) are a later slice; Docling's JSON output carries them.
- Retry and park rules belong to the work queue, not to these libraries; they report outcomes only.
- Nothing is logged except ids and counts.

## Migration plan

**Table 2 — Projects**

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| Converter, segmenter, front matter, `IDocumentOcr` | `C:\repo\_storage\src\Storage.Workers` and `Storage.Services` (read-only reference) | new `src/Framework/OoBDev.Documents.Markdown` | `OoBDev.Documents.Markdown` | packable project |
| Docling client | `Storage.Services/Documents/DoclingOcr.cs` | new `src/ExternalServices/OoBDev.Docling` | `OoBDev.Docling` | packable adapter, references only Abstractions |
| Tests | none for these | `...Markdown.Tests` and `...Docling.Tests` (HttpClient fakes, `Unit`; a live run is `DevLocal`) | matching | MSTest |

**Table 3 — References that change**

| Project | Change |
|---------|--------|
| `OoBDev.Documents.Markdown` | references `OoBDev.Documents.Abstractions`, `OoBDev.System.Abstractions`; a PDF text library only if the owner approves |
| Storage catalog | drops its copies and references the packages (outside this repository) |

## Testing strategy

Converter: text and Markdown pass through, a PDF with enough text converts with one anchor per page, a thin PDF returns needs-ocr, a non-PDF named .pdf returns not-pdf, an existing Markdown is unchanged. Segments: plan boundaries, resume after a failed second piece, page count padded or merged to the plan. Docling client with a fake handler: post, poll twice, fetch; failure status raises `OcrFailedException`; the light options run after a heavy failure; timeout raises `TimeoutException`. Golden sample accuracy is a `DevLocal` test that records host and date. Target 85 to 90 percent coverage.

## Built after the first slice

- `.xlsx`/`.xlsm` and `.pptx`/`.pptm` readers (2026-11-18) in `OoBDev.Documents.Markdown.Office`: they read the Open XML zip with `System.IO.Compression` and `System.Xml.Linq` (no package, no Tika). Spreadsheets give a table per sheet (stored values, shared strings, first row as header, `|` escaped, formulas as last saved); presentations give a section per slide in `sldIdLst` order (title placeholder as the heading, other paragraphs as bullets, speaker notes). `TryAddOpenXmlMarkdown()` registers them. Not done: number formats and dates (serials stay numbers), merged cells, charts, images, tables inside slides as tables.

- Region coordinates (2026-11-18): `IDocumentLayoutOcr` (extends `IDocumentOcr`) returns `OcrLayout(Markdown, Regions)`; `OcrRegion` carries page, label, text and a box as fractions of the page (top-left origin, so it survives any render scale and matches the [annotation layer](../AnnotationLayer/README.md)). `DoclingOcr` implements it by also asking for `json` and reading `json_content` with `DoclingLayout` (texts, tables and pictures; bottom-left boxes flipped; boxes clamped to the page). Tested with fixture JSON written from the docling document schema; not yet compared with a live docling-serve response.

## Not in the first slice

- Language detection (2026-11-18): `LanguageDetector.Detect(text)` in `OoBDev.Documents.Markdown` guesses en, de, fr, es, it, nl or pt from stop-word shares (at least 20 words, a clear lead, otherwise `null`). It is a hint for the front matter, not a classifier: other scripts and languages answer `null`; a model-based detector can replace it behind the same call. `MarkdownPages.FrontMatter` writes a `language:` line when it answers (converter and `OcrMarkdown`), and leaves it out otherwise.
- The queue, lease and twin reuse (catalog database).
- Rendering stages (`RenderStageRunner`, the renderer container) and summarize.
- Legacy OLE spreadsheet and presentation readers (`.xls`, `.ppt`): `.xlsx` and `.pptx` are built (2026-11-18, below); the binary `.xls` and `.ppt` go through the conversion chain (Tika handlers `TikaXlsToHtmlConversionHandler` and `TikaPptToHtmlConversionHandler`, built 2026-11-19, unit tested only: no sample file, so no run against the Tika container). Word-processing and e-book files are built (`OoBDev.Documents.Markdown.Office`, owner 2026-11-16: compose the Tika and MysticMind handlers, no new package).
