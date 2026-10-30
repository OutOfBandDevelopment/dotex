# DevTerm schema generator entry point (2026-10-29)

`SchemaGenerator.WriteTo(folder)` writes the three schema files; the Tui host runs it with `--write-schemas <folder>` and exits (listed in the help text). The committed copies in `schemas/devterm` stay guarded by `SchemaDriftTests`.

Verified: `SchemaGeneratorTests` (2) and the schema drift tests pass (8 in the Schema filter); the Tui built and wrote the three files to a temp folder.
