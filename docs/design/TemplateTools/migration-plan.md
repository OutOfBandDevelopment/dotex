# Fold `Xslt.Cli` into the template engine: plan

Status: confirmed by the owner on 2026-11-05 (OPEN-QUESTIONS Q3, option A). All four steps are done (2026-11-06); the BinaryDataDecoders documentation run was dropped because its report templates were rejected on 2026-11-04 (see below). Spike result: [README](README.md).

**Table 1 — Steps**

| Step | Content | Moves | Location after |
|------|---------|-------|----------------|
| 1 (done) | XSLT provider inside `OoBDev.System` | nothing | `OoBDev.System` |
| 2 (done) | Input-type mapping and `SandboxedTransformer` move out of the CLI into a library; the engine provider already takes extension objects (`XsltTemplateOptions.Extensions`) | `SandboxedTransformer` and `InputTypes` from `src/Tools/OoBDev.Xslt.Cli` | new `src/Extensions/OoBDev.System.Text.Templating.Xslt` (+ `.Tests`), namespace `OoBDev.System.Text.Templating.Xslt`, Extensions layer because the navigators come from the HTML, YAML and code-analysis extensions; references `OoBDev.TestUtilities` for `TrxExtensions` as the CLI did (moving `TrxExtensions` is a later cleanup) |
| 3 (done) | `TemplateEngine.Cli` runs a `.xslt` or `.xsl` template through `SandboxedTransformer` with the switches input type, exclude, sandbox and merge (`FileTypes` replaced by `InputTypes`); `XsltTemplateProvider` itself still takes `IXPathNavigable` only. The BinaryDataDecoders documentation generation is not ported: its 13 `templates/reports/*.xslt` were rejected on 2026-11-04 ([surroundings review](../../migration/binarydatadecoders-surroundings-review-2026-11-04.md)) because they only fed the generated docs, and the source step was already marked broken; the CLI tests cover the XML and merge paths | tests from `OoBDev.Xslt.Cli.Tests` | `OoBDev.TemplateEngine.Cli` and its tests |
| 4 (done) | Retire `Xslt.Cli` with a conversion note for its templates and scripts | `src/Tools/OoBDev.Xslt.Cli` and `.Tests` deleted | note in `docs/how-tos/` |


