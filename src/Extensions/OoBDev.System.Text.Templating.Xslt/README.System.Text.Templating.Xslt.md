# OoBDev.System.Text.Templating.Xslt

Runs an XSLT template over inputs of many types. `SandboxedTransformer` maps an input file to an XPath-navigable document by `InputTypes` (XML, HTML, JSON, YAML, C#, VB, MSBuild binlog, DACPAC, a directory path, or by file extension) and transforms one file, a wildcard or a merged set. The file, path, environment, string and XML functions available to the template reach only the sandbox folder.

It lives in Extensions because the input types need the HTML, YAML and code-analysis navigators. The command-line host is `OoBDev.TemplateEngine.Cli` (it runs `.xslt` and `.xsl` templates through the transformer; `OoBDev.Xslt.Cli` was folded into it and removed, see [the plan](../../../docs/design/TemplateTools/migration-plan.md)).

Tests: `OoBDev.System.Text.Templating.Xslt.Tests` (Unit).
