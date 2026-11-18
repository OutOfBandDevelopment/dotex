# OoBDev.Docling

`IDocumentOcr` (from `OoBDev.Documents.Abstractions`) over [docling-serve](https://github.com/docling-project/docling-serve)'s asynchronous API. Design: [DocumentToMarkdown](../../../docs/design/DocumentToMarkdown/README.md).

## Use

```csharp
services.TryAddDoclingOcr(configuration);                    // Docling:*
var ocr = sp.GetRequiredService<IDocumentOcr>();             // or keyed "docling"
var markdown = await ocr.RecognizeAsync("scan.pdf", ct);     // pages separated by IDocumentOcr.PageMark
```

Pair it with `SegmentedOcr` and `OcrMarkdown` from `OoBDev.Documents.Markdown` to read long documents in resumable pieces and write the page-anchored Markdown.

## Rules

- The PDF is posted to `v1/convert/file/async`, the task is polled (`wait` seconds per poll) until it succeeds, then `v1/result/{task}` is read.
- Tesseract at 3x scale with accurate tables and formula enrichment first; when the task or result reports failure the same file is retried once with the light options (2x scale, fast tables, no formulas).
- A transport fault or a response that is not JSON is `HttpRequestException`; no result inside `TimeoutSeconds` is `TimeoutException` (the caller retries both); a failed task or a result that is not a success after the light retry is `OcrFailedException` (final).

## Layout

`DoclingOcr` also implements `IDocumentLayoutOcr`: `RecognizeLayoutAsync` adds `json` to `to_formats` and returns the Markdown with `OcrRegion`s (page, label, text, box as fractions of the page, top-left origin) read from `json_content` by `DoclingLayout.Parse`. Registered by `TryAddDoclingOcr` as `IDocumentLayoutOcr`.

## Options (`Docling`)

| Key | Default | Meaning |
|-----|---------|---------|
| `BaseUrl` | required | Server root |
| `TimeoutSeconds` | 1800 | Total time one conversion may take |
| `PollWaitSeconds` | 20 | Seconds one status poll waits on the server |
| `OcrLanguages` | empty | Comma-separated Tesseract languages sent as `ocr_lang` (for example `eng`); empty uses the server default, which fails when a default language is not installed (the CPU image ships English only) |

## Not in the first slice

Single page images (the catalog's `IPageOcr`), the docling container in the integration stack, and an Integration test against it.

## Tests

`OoBDev.Docling.Tests` (Unit, fake HTTP handler): post, two polls and fetch; light retry after a heavy failure; both failing; failed result with errors; timeout; server down; missing task id; non-JSON; result without a document; registration and options validation.
