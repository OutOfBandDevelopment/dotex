# LlmCodeGen — Testing Strategy

[← API design](api-design.md) · [Overview](README.md)

Existing tests (`OoBDev.LlmCodeGen.Cli.Tests`): `ResponseFileExtractorTests` (8 Unit: no fences, bold name, back-ticked name, `// path` first line, unnamed blocks, unsafe names as a data-row test covering `../`, rooted, drive, invalid character and empty segment, several files, name separated by prose), `LlmCodeGenOptionsTests` (2 Unit: required values, defaults) and `FolderPromptRunnerTests` (5 Simulate: prompt, response and files per folder; existing response skipped unless `Overwrite`; extraction off; traversal name stays inside the output; cancellation; plus 1 Unit for an unknown template). Rows marked Planned are not written yet and are in `BACKLOG.md`.

**Table 3 — Test plan**

| Area | Category | Cases |
|------|----------|-------|
| `ResponseFileExtractor` | Unit | bold name plus fence; back-ticked name; `// path` first line; unnamed block; invalid characters; `../x`; rooted path; multiple files; no fences |
| Options | Unit | missing required values fail validation; defaults applied |
| `FolderPromptRunner` | Simulate | fake `IMessageCompletion` and temp folders: prompt, response and extracted files written; existing response skipped; `Overwrite` reruns; cancellation stops between folders |
| Provider selection | Unit | `LlmCodeGenServiceTests`: `ollama`, `groq` and `groq-cloud` resolve the right adapter in any letter case; an unknown key reports a clear error |
| Live run | Integration (Ollama, done) / LiveIntegration (Groq, planned) | Ollama container (phi3) against a sample folder; Groq on demand with a fresh key |

Target 85% line coverage for the extractor and runner. No test reads a real key; live Groq stays out of CI.

[← API design](api-design.md) · [Overview](README.md)
