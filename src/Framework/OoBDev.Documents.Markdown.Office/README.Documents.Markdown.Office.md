# OoBDev.Documents.Markdown.Office

Reads word-processing and e-book files (`.doc`, `.docx`, `.odt`, `.rtf`, `.epub`) as Markdown for `DocumentMarkdownConverter`. Design: [DocumentToMarkdown](../../../docs/design/DocumentToMarkdown/README.md).

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

`IFileMarkdownReader.Read` is synchronous, so the conversion is awaited on the caller's thread. Spreadsheets and presentations (`.xlsx`, `.pptx`) have no Tika handler yet and are not listed.

Tests: unit tests in `OoBDev.Documents.Markdown.Office.Tests`; the real chain is exercised by `OfficeMarkdownReaderTests` in `OoBDev.Example.Tests` (Integration, needs the Tika container).
