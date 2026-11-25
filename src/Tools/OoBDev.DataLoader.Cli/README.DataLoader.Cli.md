# OoBDev.DataLoader.Cli

`data-loader` is a dotnet tool over [`OoBDev.DataLoader`](../../Framework/OoBDev.DataLoader/README.DataLoader.md) for deployment loads: it reads a CSV or JSON file, runs it through the loader pipeline and prints the report. Design: [DataLoader](../../../docs/design/DataLoader/README.md).

```text
data-loader --input rows.csv --key id [--format csv|json] [--dataset name] [--duplicates skip|replace|fail] [--stop-on-error true] [--batch-size 100] [--sqlite store.db]
```

| Option | Meaning |
|--------|---------|
| `--input` | The file to load (required). |
| `--key` | The key column (required). |
| `--format` | `csv` or `json`; default from the file extension. |
| `--dataset` | Dataset name in the store; default the file name without extension. |
| `--duplicates` | `skip` (default), `replace` or `fail`. |
| `--stop-on-error` | Stop at the first failed row or batch. |
| `--batch-size` | Rows per write batch; default 1. |
| `--sqlite` | Write to this SQLite file (created when missing) through `OoBDev.DataLoader.EntityFrameworkCore`. Without it the run is a dry run into memory. |

Exit code 0 when clean, 1 when rows were rejected (each is printed as `file:line: message`), 2 for a usage or file error.

The command is `LoaderCommand.RunAsync`, so tests call it without a process. A host with its own database or handlers (lookups, uploads) uses the library directly; this tool covers the generic key-and-values store.
