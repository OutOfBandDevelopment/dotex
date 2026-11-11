# OoBDev.DocumentConverter.Cli

Dotnet tool (`file-convert`) that converts one file to another format with the document conversion handlers of the framework (for example HTML to PDF through [OoBDev.WkHtmlToPdf](../../ExternalServices/WkHtmlToPdf/OoBDev.WkHtmlToPdf/README.WkHtmlToPdf.md)).

## Use

```bash
file-convert --input report.html --output report.pdf
file-convert --input https://example.org/page.html --output page.pdf
```

**Table 1 — Options (`DocumentConverterOptions`; options are passed as `--name value`)**

| Option | Meaning |
|--------|---------|
| `--input` | Required; a file path or an `http`/`https` URL |
| `--output` | Required; the file to write |

Source and target types are found by matching the file extensions against the registered `IDocumentType` entries; an unknown extension throws `InvalidOperationException` ("Unable to identify input file type" or output). A missing input file throws "File not found". When no handler supports the pair the tool logs `no conversion` and writes nothing. The host is built with the common and common-external registrations (`TryCommonExtensions`, `TryCommonExternalExtensions`), so the available conversions depend on which handlers those include. `-h`, `--help` or `-?` prints the usage text (which names the tool `file-convert`).
