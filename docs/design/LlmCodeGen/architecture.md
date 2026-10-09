# LlmCodeGen — Architecture

[← Requirements](requirements.md) · [Overview](README.md) · [API design →](api-design.md)

## Components

**Table 2 — Reused and new pieces**

| Piece | Source | Role |
|-------|--------|------|
| `IMessageCompletion` | `OoBDev.AI.Abstractions` | Sends the prompt to a named model, returns the text |
| Ollama and Groq adapters | `OoBDev.Ollama`, `OoBDev.GroqCloud` | Register keyed `IMessageCompletion` (`ollama`, `groq-cloud`); the Ollama adapter gained an optional `ApiKey` |
| `PromptRenderer` | new, Handlebars.Net | Renders the prompt from the folder's files; resolves bundled template names |
| `FolderPromptRunner` | new | The loop: scan, render, ask, save, extract |
| `ResponseFileExtractor` | new | Pure parser from response text to `(relativePath, content)` pairs |
| Host and options | new | Generic host, `LlmCodeGenOptions` |

## Flow

```plantuml
@startuml
skinparam shadowing false
actor User
participant "FolderPromptRunner" as R
participant "PromptRenderer" as T
participant "IMessageCompletion\n(ollama or groq-cloud)" as L
participant "ResponseFileExtractor" as X
database "Output folder" as O

User -> R : run
loop each folder with files
  alt response file already exists
    R -> R : skip
  else
    R -> T : render(template, files)
    T --> R : prompt
    R -> O : write prompt
    R -> L : GetCompletionAsync(model, prompt)
    L --> R : response text
    R -> O : write response
    R -> X : extract(response)
    X --> R : files
    R -> O : write files (inside output only)
  end
end
@enduml
```

*Figure 1 — one folder*

## Placement

A tool under `src/Tools/OoBDev.LlmCodeGen.Cli` with a `README.LlmCodeGen.Cli.md`, packed as a dotnet tool like the other `*.Cli` projects. The extractor and runner sit in the tool project because nothing else needs them; move them to a Framework library only if a second consumer appears.

[← Requirements](requirements.md) · [Overview](README.md) · [API design →](api-design.md)
