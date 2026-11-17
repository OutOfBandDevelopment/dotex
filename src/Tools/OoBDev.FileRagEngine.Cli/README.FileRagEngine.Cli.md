# OoBDev.FileRagEngine.Cli

Dotnet tool (`file-rag`) that runs a prompt template over the files of a folder tree and saves the language model's answer next to a mirrored output tree.

## Use

```bash
file-rag --input ./src --output ./out --template review.hbs --llm ollama --include-prompt true
```

**Table 1 — Options (`FileRagEngineOptions`; options are passed as `--name value`, a boolean needs an explicit `true`)**

| Option | Meaning |
|--------|---------|
| `--input` | Required; the root folder to scan (all sub folders) |
| `--output` | Required; the root of the mirrored output tree |
| `--template` | Required; the template file applied per chunk by `ITemplateEngine` (the data is `files`: name and content) |
| `--llm` | Key of the `IMessageCompletion` provider (for example the Ollama adapter); without it the default registration is used, and none is an error |
| `--raw` | Also write the full model response as `.json` |
| `--include-prompt` | Also write the rendered prompt as `.prompt.<ext>` |

## Behaviour

- Every folder is processed on its own; `bin` and `obj` folders are skipped. Binary and archive extensions (pdf, dll, exe, images, zip, gz, epub, rtf, doc, docx, dacpac, bacpac, bin, svg, ico and a few more) are not read. A file with another extension is read only when its first 8 KB look like text (`TextContent.FileLooksLikeText`: no NUL bytes, few control characters).
- Files of a folder are sent in chunks of five. The first chunk's answer is written to `<output>/<folder>/<template name>`, later chunks to `<name>.<offset>.<ext>`.
- A folder whose answer file already exists is skipped, so an interrupted run resumes.
- A failing chunk writes `<name>.error.<ext>` with the exception, the run continues, and an `AggregateException` is thrown at the end.
