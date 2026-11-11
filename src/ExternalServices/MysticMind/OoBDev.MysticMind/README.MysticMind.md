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

## Tests

`OoBDev.MysticMind.Tests` has one `Unit` test method for the conversion; the unsupported-type path is not covered.
