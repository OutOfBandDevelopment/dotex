# LlmCodeGen — API Design

[← Architecture](architecture.md) · [Overview](README.md) · [Testing strategy →](testing-strategy.md)

## Options

```csharp
public record LlmCodeGenOptions
{
    public const string SectionName = "LlmCodeGen";

    [Required] public required string InputPath { get; init; }
    [Required] public required string OutputPath { get; init; }
    [Required] public required string Template { get; init; }   // bundled name or file path
    [Required] public string Provider { get; init; } = "ollama"; // keyed IMessageCompletion: ollama or groq-cloud (alias groq)
    [Required] public required string Model { get; init; }
    public string SearchPattern { get; init; } = "*.*";
    public bool Overwrite { get; init; }
    public bool ExtractFiles { get; init; } = true;
}
```

Bound with `AddOptions<LlmCodeGenOptions>().Bind(...).ValidateDataAnnotations()`. Provider endpoint, model and key stay in the adapters' own sections (`OoBDev:Ollama`, Groq), so credentials are never tool options.

## Command line

`llmcodegen --input <dir> --output <dir> --template <name|path> --model <model> [--provider ollama|groq] [--pattern <glob>] [--overwrite] [--extract false]`

Arguments override `appsettings.json`, which overrides defaults.

## Extractor

```csharp
public static class ResponseFileExtractor
{
    public static IReadOnlyList<ExtractedFile> Extract(string response);
}

public sealed record ExtractedFile(string RelativePath, string Content);
```

Recognized shapes are the two the prototype handled: a bold or back-ticked file name line followed by a fenced block, and a fenced block whose first line is `// path`. A block with no name becomes `unknown{n}.txt`; a name with invalid characters becomes `error{n}.txt` with the name kept as a first-line comment. Rooted paths and `..` segments are rejected into `error{n}.txt` as well, so a write can never leave the output folder.

## Output layout

For input folder `a/b`: `<output>/a/b/<template>` (prompt), `<template>.response.md`, `<template>.response.json` (provider metadata where available) and the extracted files beside them.

[← Architecture](architecture.md) · [Overview](README.md) · [Testing strategy →](testing-strategy.md)
