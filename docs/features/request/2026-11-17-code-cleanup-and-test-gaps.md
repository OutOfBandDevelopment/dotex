# Feature request: code cleanup and test gaps

- **Status:** triaged
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** `OoBDev.System`, `OoBDev.TestUtilities`, MongoDB, Handlebars and enum tests

## Need

Small cleanups and test gaps that were left as comments. They are kept as a request so each can be done or declined.

## Proposed behavior

1. `ObjectConverter`: "a bit nutty and should be tossed": replace or remove it.
2. `DataConverter`: add the `TryParse` path.
3. `StreamEx`: decide whether reading the stream leaves it open (`StreamReader` disposal).
4. `TestContextExtensions`: clean up the helper at the marked spot.
5. `MongoQueryFixes`: add a real test.
6. `EnumExtensionsTests` (2) and `HandlebarsTemplateProviderTests`: "fix this better" workarounds.

## Acceptance criteria

- Given each item, when done, then a test covers the new behavior and the comment is gone; or the item is declined with a reason.

## Existing code checked

`BACKLOG.md` has no matching lines.

Source comments (replaced by a reference to this request):

- `src/Framework/OoBDev.System/ComponentModel/ObjectConverter.cs`: "this is a bit nutty and should be tossed".
- `src/Framework/OoBDev.System/ComponentModel/DataConverter.cs`: "try parse".
- `src/Framework/OoBDev.System/StreamEx.cs`: "should this leave the underlying stream open?".
- `src/Framework/OoBDev.TestUtilities/TextContextExtensions.cs`: "clean this up".
- `src/ExternalServices/MongoDb/OoBDev.MongoDB.Tests/MongoQueryFixes.cs`: "there should really be some real test here".
- `src/Framework/OoBDev.System.Tests/Reflection/EnumExtensionsTests.cs` (2) and `src/ExternalServices/Handlebars/OoBDev.Handlebars.Tests/Templating/HandlebarsTemplateProviderTests.cs`: "fix this better".

## Security and operations

None.

## Priority

- **Impact:** maintainability only.
- **Needed by:** no date.
- **Workaround today:** none.

## Open questions

None.

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments). Design document to follow when built.
