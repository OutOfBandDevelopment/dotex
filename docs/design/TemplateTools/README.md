# Template and transform tools - shootout result

**Last Updated:** 2026-11-05
**Status:** spike written from the code; no code moved (see [OPEN-QUESTIONS.md](../../../OPEN-QUESTIONS.md) Q3)

Backlog item: stack review of `OoBDev.Xslt.Cli` and `OoBDev.TemplateEngine.Cli` (owner, 2026-10-30).

## What each tool does today

**Table 1 — Tool comparison**

| | `OoBDev.Xslt.Cli` (`oobdev-xslt`) | `OoBDev.TemplateEngine.Cli` |
|---|---|---|
| Engine | `XsltTransformer` with extension objects (path, environment, file, trx, string, xml, XPath 2.0 functions) | `ITemplateEngine` (providers: XSLT, Handlebars) |
| Input types | Xml, Html, Json, Yaml, CSharp, VB, MSBuildStructuredLog, DacPac, Path, by extension, through XPath navigators | Json (`JsonDocument`) and Xml (`XDocument`) only |
| Batch features | wildcard in and out, exclude, merge of all inputs into one document | wildcard in, `[file]` in the output name |
| Sandbox | file functions limited to a folder (`SandboxedTransformer`) | none |
| Hosting | plain `Main` | generic host with a hosted service |
| Tests | 2 Unit classes | none |

## Findings

1. The two tools overlap only on "apply a template to a data file". The XSLT tool is the richer one: its navigators turn code, logs and DACPACs into XML, and merge mode feeds the documentation generation.
2. `XsltTemplateProvider` in the engine does not use the extension objects or the navigators (it loads `XslCompiledTransform` with default settings and takes only JSON/XML data), so the engine cannot yet run the BinaryDataDecoders templates.
3. Handlebars cannot read navigator inputs without a conversion step; XPath over the code-analysis navigators has no Handlebars equivalent.

## Recommended shape

A, three steps, each its own commit:

1. **Done 2026-11-05:** `XsltTemplateProvider` takes `XsltTemplateOptions.Extensions` (the extension objects) and already accepts any `IXPathNavigable` as data; the sandbox stays in the extension objects (`PathExtensions(root)`, `FileExtensions(root)`).
2. Make `TemplateEngine.Cli` accept the `Xslt.Cli` switches (`--input-type`, `--merge`, `--exclude`, `--sandbox`) and reuse `SandboxedTransformer`'s input mapping from a Framework library.
3. (Dropped 2026-11-06: the report templates were rejected, so there is no documentation run to port.) Retire `Xslt.Cli` with a conversion note (`docs/how-tos/`). Keep the dotnet tool packaging with its `.props` and `.targets` if the owner still wants one.

Step 2 moves `SandboxedTransformer` and the input mapping out of a Tools project into a library, so it needs a confirmed migration plan first (rule `layering-and-migration-plans`).

## Not verified

No code was run or changed in this spike. The comparison is read from the sources above; a timing or output-parity shootout needs steps 1 and 2 first.
