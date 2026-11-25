# OoBDev.Documents.Markdown.Office

Reads word-processing and e-book files (`.doc`, `.docx`, `.odt`, `.rtf`, `.epub`, and the binary `.xls` and `.ppt` through Tika) as Markdown for `DocumentMarkdownConverter`. Design: [DocumentToMarkdown](../../../docs/design/DocumentToMarkdown/README.md).

| Type | Role |
|------|------|
| `OfficeFileMarkdownReader` | An `IFileMarkdownReader`: maps the extension to a content type and asks `IDocumentConversion` for `text/markdown`; the registered handlers do the work |
| `ServiceCollectionExtensions.TryAddOfficeMarkdown()` | Registers the reader once beside the other readers |

The project references only `OoBDev.Documents.Abstractions`: it carries no office library and no service address. The chain comes from the registered handlers, normally the Tika handlers (`OoBDev.Apache.Tika`, office to HTML, needs the Tika container) and the MysticMind handler (`OoBDev.MysticMind`, HTML to Markdown). A chain that cannot reach Markdown makes `IDocumentConversion` throw `NotSupportedException`; one that returns false gives an empty result, which the converter reports as `Empty`.

```csharp
services.TryAddDocumentServices();      // IDocumentConversion and the chain builder
services.TryAddApacheTikaServices(...); // docx to HTML
services.TryAddMysticMindServices();    // HTML to Markdown
services.TryAddOfficeMarkdown();
```

`IFileMarkdownReader.Read` is synchronous, so the conversion is awaited on the caller's thread. Spreadsheets and presentations are read directly: `XlsxFileMarkdownReader` (`.xlsx`, `.xlsm`: a table per sheet, first row as the header, stored values) and `PptxFileMarkdownReader` (`.pptx`, `.pptm`: a section per slide in presentation order, the title placeholder as the heading, paragraphs as bullets, speaker notes). They use only `System.IO.Compression` and `System.Xml.Linq`, need no conversion chain and are registered with `TryAddOpenXmlMarkdown()`. Dates and number formats are not interpreted; legacy `.xls` and `.ppt` are not read.

Tests: unit tests in `OoBDev.Documents.Markdown.Office.Tests`; the real chain is exercised by `OfficeMarkdownReaderTests` in `OoBDev.Example.Tests` (Integration, needs the Tika container).
