# oobdev-migrationhelper

Renames a project prefix in folder names, file names and file contents.

## Options

Run the tool with `--help` to list its options.

```console
$ oobdev-migrationhelper --help
oobdev-migrationhelper --path <folder> [--from BinaryDataDecoders] [--to OoBDev]
  Renames folders and files that start with the --from prefix and replaces the prefix inside every text file under --path.
  It changes files in place, so run it on a clean working tree only.
  --path  folder to rewrite (required)
  --from  prefix to replace (default BinaryDataDecoders)
  --to    new prefix (default OoBDev)
```
