# Fold `Xslt.Cli` into the template engine: plan

Status: confirmed by the owner on 2026-11-05 (OPEN-QUESTIONS Q3, option A). Step 1 is done; steps 2 and 3 are backlog work. Spike result: [README](README.md).

**Table 1 — Steps**

| Step | Content | Moves | Location after |
|------|---------|-------|----------------|
| 1 (done) | XSLT provider inside `OoBDev.System` | nothing | `OoBDev.System` |
| 2 | Engine XSLT provider takes extension objects and navigators (the 11 input types from `InputTypes.cs`), plus `SandboxedTransformer` | `SandboxedTransformer` and the input-type mapping from `src/Tools/OoBDev.Xslt.Cli` into a library the engine can reference | namespace `OoBDev.System.Text.Templating` (or the library chosen in the step, recorded here before the move) |
| 3 | `TemplateEngine.Cli` switches (input type, merge, sandbox) and an MSTest run of the BinaryDataDecoders documentation generation | tests from `OoBDev.Xslt.Cli.Tests` | `OoBDev.TemplateEngine.Cli` and its tests |
| 4 | Retire `Xslt.Cli` with a conversion note for its templates and scripts | `src/Tools/OoBDev.Xslt.Cli` and `.Tests` deleted | note in `docs/how-tos/` |

The library target in step 2 is the open detail; it is settled in this table (checking the target's references first) before any file moves.
