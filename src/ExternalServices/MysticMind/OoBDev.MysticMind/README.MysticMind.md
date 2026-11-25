# OoBDev.MysticMind

HTML to Markdown conversion for the document conversion pipeline, using the [ReverseMarkdown](https://github.com/mysticmind/reversemarkdown-net) `Converter` (the project is named after its author).

## Use

```csharp
services.TryAddMysticMindServices();
```

This registers `IConverterFactory` (builds `new Converter()` with default settings), a transient `Converter` resolved from that factory, and `HtmlToMarkdownConversionHandler` as an `IDocumentConversionHandler`. Replace `IConverterFactory` before this call to change the converter settings. No `IDocumentType` is registered; the Markdown type comes from [OoBDev.Markdig](../../Markdig/OoBDev.Markdig/README.Markdig.md).

**Table 1 — Content types of `HtmlToMarkdownConversionHandler` (compared ignoring case)**

| Direction | Types |
|-----------|-------|
| Sources | `text/html`, `text/xhtml`, `text/xhtml+xml` |
| Destinations | `text/markdown`, `text/x-markdown`, `text/plain` |

`ConvertAsync` throws `NotSupportedException` for any other content type, reads the whole source as text, converts it and writes the Markdown to the destination, leaving both streams open. Unlike the Markdig and HtmlToOpenXml handlers it does not rewind the destination.

## File reader for Markdown conversion

`HtmlFileMarkdownReader` implements `IFileMarkdownReader` (`OoBDev.Documents.Abstractions`) for `.html`, `.htm` and `.xhtml` files, registered by `TryAddMysticMindServices`. `DocumentMarkdownConverter` (`OoBDev.Documents.Markdown`) uses it to turn an HTML file into Markdown with front matter (`extractor: reversemarkdown html`).

## Tests

`OoBDev.MysticMind.Tests` covers the conversion handler (content types) and `HtmlFileMarkdownReaderTests` (registration, file to Markdown); the handler's unsupported-type path is not covered.
