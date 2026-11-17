# LlmCodeGen — Requirements

[← Overview](README.md) · [Architecture →](architecture.md)

## Goals

1. Run one prompt per source folder from a Handlebars template and keep the prompt, the raw response and the extracted files.
2. Choose the LLM provider by configuration (`ollama` or `groq`), with no code difference between them.
3. Bundled templates for documentation, unit tests and Angular to React conversion (the three the prototypes carried), plus any template path given on the command line.
4. Re-runs skip folders that already have a response, so an interrupted run resumes.
5. Extract generated files from the response into the output tree, never outside it.

## Non-goals

- Streaming or multi-turn chat (that is the SemanticKernel chat surface).
- Retrieval over a vector store (that is `FileRagEngine.Cli`).
- Patching the input tree in place; output always goes to a separate folder.

## What the prototypes lacked

**Table 1 — Gaps carried into this design**

| Gap | Resolution |
|-----|------------|
| Hard-coded input, output, endpoint and model | Options from arguments, environment and `appsettings.json` |
| File extraction commented out | Implemented and unit tested as a pure function |
| No path safety on extracted names | Names are normalized and must stay inside the output folder |
| Two copies of the loop | One loop, provider chosen by key |
| No cancellation, no logging | `CancellationToken` and `ILogger` |

## Untrusted input and output

Source files go into the prompt and the response comes back as files, so both sides are untrusted (prompt injection: text in a source file that tells the model to do something else).

- **The model only produces text.** The tool never executes, imports or shells out to generated content; it only writes files. A new feature that acts on model output (running a command, calling an API) needs its own design line and an owner decision.
- **Writes stay inside the output folder.** Names from the response are resolved against the output root and a traversal or absolute name is rejected (`FolderPromptRunnerTests.RunAsync_TraversalNameInResponse_StaysInsideOutput`).
- **Reviewed, not trusted.** Output is a draft for a person to review in version control; it is never merged or deployed by the tool.
- **No secrets in prompts.** Templates take source folders only; credentials stay in provider options, never in a template or a prompt file.
- **Cost accounting is still open.** Token counts per run are not recorded yet; the response metadata file is where they would go.

## Acceptance

- Running against a small sample folder with a fake `IMessageCompletion` writes the prompt, the response and the extracted files.
- A response containing `../` or an absolute path cannot write outside the output folder.
- Switching `LlmCodeGen:Provider` between `ollama` and `groq` needs no other change.

[← Overview](README.md) · [Architecture →](architecture.md)
