# OoBDev.MigrationHelper.Cli

Console tool (not packaged: `IsPackable` is false) that swaps a name prefix in the folder names, file names and text of a code tree, for bringing a copied project into the OoBDev naming.

## Use

```bash
dotnet run --project src/Tools/OoBDev.MigrationHelper.Cli -- --path ../copy --from BinaryDataDecoders --to OoBDev
```

**Table 1 — Options**

| Option | Meaning |
|--------|---------|
| `--path` | Required; the folder to rewrite. Without it the tool prints the usage and exits with 1 |
| `--from` | Prefix to replace (default `BinaryDataDecoders`) |
| `--to` | New prefix (default `OoBDev`) |

`-h`, `--help` or `-?` prints the usage and exits with 0.

## What it does

1. Renames every folder whose name starts with `--from` (case-sensitive), replacing the prefix.
2. Renames every file whose name starts with `--from` the same way.
3. Reads every file under the path as text and, when the content contains the prefix, replaces every occurrence (namespaces, usings, project references, documentation) and writes the file back.

Each renamed name or rewritten file path is printed; read or write failures go to stderr and processing continues.

## Limits

- It changes files in place with no dry run and no backup: run it on a clean working tree or a copy and review `git diff`.
- It is plain text replacement, not C# aware, and replaces the prefix anywhere in a file, not only at the start of a name.
- The folder rename pass enumerates before moving, so renaming a parent folder can invalidate the paths of nested folders that also start with the prefix; run the tool again if a nested folder was missed.
- The binary check is broken: a file with a null character prints `Skip:` but is still written back (backlog item).
