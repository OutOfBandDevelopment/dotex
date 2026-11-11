# OoBDev.FixSourceLinks.Cli

Dotnet tool (`fix-sourcelink`) that rewrites source-link URLs in XML reports back to local file paths. Use it for reporting tools that cannot read source links, for example Cobertura coverage files produced with `UseSourceLink` on. Alternatively keep source links out of the report by setting `UseSourceLink` to `false` in `.runsettings`.

## Use

```bash
fix-sourcelink --include "**/coverage.cobertura.xml" --search TestResults --target .
```

**Table 1 — Options**

| Option | Meaning |
|--------|---------|
| `--include` | Semicolon-separated globs of the files to fix; nothing matches without it |
| `--exclude` | Semicolon-separated globs to skip |
| `--search` | Folder searched for files (default: the current folder) |
| `--target` | Folder the rewritten paths point into (default: the search folder) |

For every `filename` attribute in a matching XML file whose value is an `https://` URL with a `path` query parameter, the tool replaces the value with the full path of that `path` under `--target` and saves the file only when something changed. It prints each file and change; when no file matches it prints `No Matches!` and lists the search folder. Errors are written per file and processing continues. `-h`, `--help` or `-?` prints the usage text.
