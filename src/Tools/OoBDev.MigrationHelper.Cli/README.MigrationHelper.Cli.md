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
| `--dry-run` | List what would change and change nothing |

`-h`, `--help` or `-?` prints the usage and exits with 0.

## What it does

1. Renames every folder whose name starts with `--from` (case-sensitive), replacing the prefix.
2. Renames every file whose name starts with `--from` the same way.
3. Reads every file under the path as text and, when the content contains the prefix, replaces every occurrence (namespaces, usings, project references, documentation) and writes the file back. A file containing a null character is treated as binary: it is reported as `Skip:` and left unchanged.

The work is done by `PrefixMigrator.Run` (covered by `OoBDev.MigrationHelper.Cli.Tests`). Each renamed name, rewritten file path or skipped file is printed (`Would ...` in a dry run); read or write failures go to stderr and processing continues.

## Limits

- It changes files in place with no backup (use `--dry-run` first): run it on a clean working tree or a copy and review `git diff`.
- It is plain text replacement, not C# aware, and replaces the prefix anywhere in a file, not only at the start of a name.
- Folders are renamed deepest first, so nested folders that also start with the prefix are all renamed in one run.
