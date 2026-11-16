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

The contracts live in `OoBDev.Documents.Abstractions`: `IDocumentOcr`, `OcrFailedException`, `IPdfTextReader`, `IPdfPageTools`. The framework carries no PDF package: a PDF library adapter implements the two PDF interfaces, and `OoBDev.Docling` implements `IDocumentOcr`.

Not built yet: HTML to Markdown, office formats (docx, rtf, epub, xlsx, pptx and the legacy OLE formats), the PDF library adapter.
