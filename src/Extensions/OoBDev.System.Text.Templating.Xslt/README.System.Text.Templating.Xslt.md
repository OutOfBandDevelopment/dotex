# OoBDev.System.Text.Templating.Xslt

Runs an XSLT template over inputs of many types. `SandboxedTransformer` maps an input file to an XPath-navigable document by `InputTypes` (XML, HTML, JSON, YAML, C#, VB, MSBuild binlog, DACPAC, a directory path, or by file extension) and transforms one file, a wildcard or a merged set. The file, path, environment, string and XML functions available to the template reach only the sandbox folder.

It lives in Extensions because the input types need the HTML, YAML and code-analysis navigators. The command-line hosts are `OoBDev.Xslt.Cli` and (after the fold-in, see [the plan](../../../docs/design/TemplateTools/migration-plan.md)) `OoBDev.TemplateEngine.Cli`.

Tests: `OoBDev.System.Text.Templating.Xslt.Tests` (Unit).
