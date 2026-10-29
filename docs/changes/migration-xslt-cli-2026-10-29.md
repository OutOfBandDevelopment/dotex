# BinaryDataDecoders Xslt CLI (2026-10-29)

Ported the Xslt CLI as `src/Tools/OoBDev.Xslt.Cli` (tool command `oobdev-xslt`) with `OoBDev.Xslt.Cli.Tests` (24 Unit tests, all passing).

## Decisions

- Options are parsed with `Microsoft.Extensions.Configuration.CommandLine` and a switch map, in `XsltCliOptions`; a bare `-m` or `--merge` is given the value `true` because the provider drops a valueless switch.
- `SandboxedTransformer` and `InputTypes` are public so they can be tested and reused; `ByExtention` was renamed `ByExtension` (breaking rename, allowed).
- Navigators come from the migrated `OoBDev.*` projects (HTML, YAML, JSON, path, C#, VB, binlog, DacPac).

## Fixed on the way

- `XsltTransformer.ReadAsXml` never disposed its `XmlReader`, which kept the input file locked after a transform; it now uses `using`.

## Not run

The tool was not packed or installed as a dotnet tool, and no DacPac or binlog transform was run (only the mapping is tested).
