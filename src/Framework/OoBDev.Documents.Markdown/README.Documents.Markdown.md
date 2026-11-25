# OoBDev.Documents.Markdown

Converts documents to page-anchored Markdown with front matter, and plans OCR in resumable pieces. Design: [DocumentToMarkdown](../../../docs/design/DocumentToMarkdown/README.md).

| Type | Role |
|------|------|
| `DocumentMarkdownConverter` | Text and PDF to Markdown; a PDF averaging under 40 characters a page returns `NeedsOcr`; a file named .pdf without `%PDF` returns `NotPdf` |
| `OcrMarkdown` | Writes Markdown from OCR text split at `IDocumentOcr.PageMark`; copies an OCR result for identical content |
| `PdfSegmenter`, `SegmentedOcr` | Pack chapters into page ranges, OCR each range, keep finished pieces so a retry resumes, keep page numbers true to the source |
| `MarkdownPages` | The `<a id="pN">` anchors, page headings and front matter |
| `ChunkText` | Page text to embedding chunks (1500 characters, 150 overlap, minimum 30) |
| `ExtractPaths` | Markdown path under an extract folder from a stored catalog path and hash |

The contracts live in `OoBDev.Documents.Abstractions`: `IDocumentOcr`, `OcrFailedException`, `IPdfTextReader`, `IPdfPageTools`. The framework carries no PDF package: a PDF library adapter implements the two PDF interfaces, and `OoBDev.Docling` implements `IDocumentOcr`. `IFileMarkdownReader` (also there) is the seam for other formats: `DocumentMarkdownConverter` takes the registered readers and uses the one whose extension matches the file before its plain-text fallback; the front matter `kind` is the extension and `extractor` the reader name. `OoBDev.MysticMind` supplies the HTML reader and `OoBDev.Documents.Markdown.Office` the word-processing and e-book reader (it composes the registered conversion handlers).

Not built yet: spreadsheet and presentation formats (xlsx, pptx and the legacy OLE formats), the PDF library adapter.

## Language hint

`LanguageDetector.Detect(text)` returns an ISO 639-1 code (en, de, fr, es, it, nl, pt) from stop-word shares, or `null` for short text, other scripts or no clear lead.
