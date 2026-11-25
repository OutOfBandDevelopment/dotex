# OoBDev.Markdig

Markdown to HTML conversion for the document conversion pipeline, using [Markdig](https://github.com/xoofx/markdig).

## Use

```csharp
services.TryAddMarkdigServices();
```

This registers a transient `MarkdownPipeline` built with `UseAdvancedExtensions()` (tables, footnotes, task lists and the other advanced extensions; replace it by registering your own pipeline first), the `MarkdownToHtmlConversionHandler` as an `IDocumentConversionHandler`, and a `Markdown` `IDocumentType` (extension `.md`, content types `text/markdown` and `text/x-markdown`). The handler is picked by `IDocumentConversion` when a Markdown source meets an HTML destination, for example with [OoBDev.WkHtmlToPdf](../../WkHtmlToPdf/OoBDev.WkHtmlToPdf/README.WkHtmlToPdf.md) for Markdown to PDF.

**Table 1 — Content types of `MarkdownToHtmlConversionHandler` (compared ignoring case)**

| Direction | Types |
|-----------|-------|
| Sources | `text/markdown`, `text/x-markdown`, `text/plain` |
| Destinations | `text/html`, `text/xhtml`, `text/xhtml+xml` |

`ConvertAsync` throws `NotSupportedException` for any other content type, reads the whole source as text, converts it and writes the HTML to the destination, leaving both streams open and rewinding the destination to position 0.

The output is an HTML fragment: Markdig adds no `<html>` or `<body>` wrapper, so wrap it in a template when a full page is needed. `OoBDev.Markdig.Tests` has one data-driven Unit test (`ConvertAsyncTest`) comparing converted output with embedded files; the unsupported-type path is not covered.
