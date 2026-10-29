# OoBDev.DevTerm.Schemas

Exports a JSON Schema for each hand-written DevTerm format (device manifest, UI definition, connection profile) from the C# model, so a schema cannot drift from the code.

```
dotnet run --project src/Extensions/OoBDev.DevTerm.Schemas -- schemas/devterm
```

The committed copies live in `schemas/devterm/`; `SchemaDriftTests` fails when they differ from the generated text.
