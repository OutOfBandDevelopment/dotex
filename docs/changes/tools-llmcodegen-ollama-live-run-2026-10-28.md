# LlmCodeGen Ollama live run (2026-10-28)

`OllamaLiveRunTests` (Integration, `OoBDev.LlmCodeGen.Cli.Tests`) runs `FolderPromptRunner` against the test stack's Ollama (`OLLAMA_URL`, `OLLAMA_MODEL`, default phi3) on a throwaway sample folder and checks the prompt file and a non-empty response file. The provider is resolved through `LlmCodeGenService.ResolveCompletion`, so keyed registration is covered as well.

Verified: 1 passed against the Aspire Ollama on port 11435. The Groq live run stays open (key revoked).
