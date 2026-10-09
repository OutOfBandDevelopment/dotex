# LlmCodeGen — Testing Strategy

[← API design](api-design.md) · [Overview](README.md)

**Table 3 — Test plan**

| Area | Category | Cases |
|------|----------|-------|
| `ResponseFileExtractor` | Unit | bold name plus fence; back-ticked name; `// path` first line; unnamed block; invalid characters; `../x`; rooted path; multiple files; no fences |
| Options | Unit | missing required values fail validation; defaults applied |
| `FolderPromptRunner` | Simulate | fake `IChatProvider` and temp folders: prompt, response and extracted files written; existing response skipped; `Overwrite` reruns; cancellation stops between folders |
| Provider selection | Unit | `ollama` and `groq` keys resolve the right adapter; unknown key reports a clear error |
| Live run | Integration / LiveIntegration | Ollama container (phi3) against a sample folder; Groq on demand with a fresh key |

Target 85% line coverage for the extractor and runner. No test reads a real key; live Groq stays out of CI.

[← API design](api-design.md) · [Overview](README.md)
