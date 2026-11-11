# Converting `oobdev-xslt` calls to `templateengine`

`oobdev-xslt` (`OoBDev.Xslt.Cli`) was retired on 2026-11-06. Its transformer lives in the library `OoBDev.System.Text.Templating.Xslt` (`SandboxedTransformer`, `InputTypes`), and `templateengine` runs any template that ends in `.xslt` or `.xsl` through it. The XSLT files themselves need no change.

**Table 1 — Switch mapping**

| `oobdev-xslt` | `templateengine` | Note |
|---------------|-------------------------|------|
| `-i`, `--input` | `--input` | same wildcard rules |
| `-t`, `--template` | `--template` | the path of the XSLT file |
| `-o`, `--output` | `--output` | `*` and `[file]` both stand for the input name without extension |
| `-e`, `--exclude` | `--exclude` | |
| `-x`, `--input-type` | `--input-type` | default is now `ByExtension` (was `Xml`); name `Xml` to keep the old behavior for other extensions |
| `-s`, `--sandbox` | `--sandbox` | default is still the output folder |
| `-m`, `--merge` | `--merge true` | a boolean needs an explicit value; there are no short switches |

Example: `oobdev-xslt -i "src/*.cs" -x CSharp -t report.xslt -o out/*.md -m` becomes `templateengine --input "src/*.cs" --input-type CSharp --template report.xslt --output out/[file].md --merge true`.

Scripts that call the old tool by name switch to `templateengine` (tool command `templateengine` in the package). From code, reference `OoBDev.System.Text.Templating.Xslt` and call `new SandboxedTransformer(sandbox).TransformAll(...)`.

Design and plan: [TemplateTools](../design/TemplateTools/README.md).
