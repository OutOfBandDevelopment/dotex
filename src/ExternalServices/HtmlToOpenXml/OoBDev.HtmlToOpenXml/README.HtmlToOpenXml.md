# OoBDev.HtmlToOpenXml

HTML to Word (`.docx`) conversion for the document conversion pipeline, using the [HtmlToOpenXml](https://github.com/onizet/html2openxml) package and the Open XML SDK. It runs in process, with no external tool.

## Use

```csharp
services.TryAddHtmlToOpenXmlServices();
```

This registers `HtmlToDocxConversionHandler` as an `IDocumentConversionHandler` and a `Microsoft Word - OpenXML` `IDocumentType` (extension `.docx`, content type `application/vnd.openxmlformats-officedocument.wordprocessingml.document`). A TODO in the registration considers Tika instead, or making this adapter excludable from the common set.

**Table 1 — Content types of `HtmlToDocxConversionHandler` (compared ignoring case)**

| Direction | Types |
|-----------|-------|
| Sources | `text/html`, `text/xhtml`, `text/xhtml+xml` |
| Destinations | the `.docx` content type above (`DESTINATIONS`) |

`ConvertAsync` throws `NotSupportedException` for any other content type. It rewinds the source, reads it as text, creates a Word document in memory, parses the HTML into its body, writes the bytes to the destination and rewinds the destination. For PDF output use [OoBDev.WkHtmlToPdf](../../WkHtmlToPdf/OoBDev.WkHtmlToPdf/README.WkHtmlToPdf.md).

## Tests

`OoBDev.HtmlToOpenXml.Tests` contains no test classes yet (backlog item).
