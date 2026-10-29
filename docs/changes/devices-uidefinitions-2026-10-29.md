# Device decoding: UiDefinitions (2026-10-29)

Phase 6 of the [device decoding plan](../migration/device-decoding-migration-plan.md): dev-term `UiDefinitions` is now `src/Framework/OoBDev.UiDefinitions` with `OoBDev.UiDefinitions.Tests` (189 Unit tests, all passing). Types and scope: `src/Framework/OoBDev.UiDefinitions/README.UiDefinitions.md`.

## Decisions

- Namespaces `DevTerm.UiDefinitions` became `OoBDev.UiDefinitions`; implicit usings were replaced with explicit ones.
- Multi-type files were split to one type per file (for example `LiveDisplayState.cs` into the state types, `VectorControl.cs` into the enums).
- The two generated regexes got a one second match timeout (MA0009).
- Tests use the `Unit` category string instead of dev-term's `TestCategories` helper; the dev-term test utilities project is not needed.

## Not done

- `MA0016` (concrete `List<T>` properties used by the serializers) and `MA0051` (the expression parser and form generator methods are over 60 lines) were left as ported; splitting the parser is a refactor for later, tracked in `BACKLOG.md`.
- The tests cover the model, serializer, validator, expression language, live state and forms; coverage was not measured.
