# OoBDev - LlmCodeGen

## Summary

A dotnet tool that walks a folder tree, renders one prompt per folder from a Handlebars template, sends it to a language model (Ollama or Groq) and saves the prompt, the response and every file the response contains. Design: [docs/design/LlmCodeGen](../../../docs/design/LlmCodeGen/README.md).

## Usage

```
llmcodegen --input <dir> --output <dir> --template <name|path> --model <model> [--provider ollama|groq] [--pattern "*.ts"] [--overwrite] [--extract false]
```

**Table 1 — Options**

| Option | Default | Meaning |
|--------|---------|---------|
| `--input` | required | Folder scanned recursively; each folder with matching files is one prompt |
| `--output` | required | Where prompts, responses and extracted files go (same relative layout as the input) |
| `--template` | required | Bundled name or the path of a `.hbs` file |
| `--model` | required | Model name sent with the prompt |
| `--provider` | `ollama` | `ollama` or `groq` (the `groq-cloud` key) |
| `--pattern` | `*.*` | File pattern for the files in a prompt |
| `--overwrite` | off | Rerun folders that already have a response |
| `--extract` | `true` | Write the files found in the response |

Settings can also come from environment variables (`LlmCodeGen__Model`). Provider endpoints and keys are the adapters' own settings: `OoBDev__Ollama__Url` (default `http://localhost:11434`) and `OoBDev__GroqCloud__ApiKey` (or the `API_Key_Groq` user variable). Keys are never tool options.

## Bundled templates

**Table 2 — Templates**

| Name | Purpose |
|------|---------|
| `documentation` | One `README.md` per folder |
| `unit-tests` | MSTest classes in the repository style |
| `angular-to-react` | Angular components to React with TypeScript |
| `angular-to-vue` | Angular components to Vue 3 with TypeScript |

A template sees `folder` and `files` (each with `name`, `extension`, `content`). Ask the model to put each file name in bold on the line before its fenced block (`**Foo.cs**`) or as `// path` on the first line of the block, and the tool writes the files for you.

## Output

For each folder: `<template>.prompt.md`, `<template>.response.md` and the extracted files. A name that is rooted, contains `..` or has invalid characters is written to `error{n}.txt` instead, so nothing leaves the output folder. A block with no name becomes `unknown{n}.txt`.

## Example

```
llmcodegen --input ./src/app --output ./out --template angular-to-vue --model phi3
```

Use `--provider groq --model openai/gpt-oss-20b` with a Groq key set for the hosted model.
