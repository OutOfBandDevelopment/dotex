# Feature request: XSLT transformer and extension function improvements

- **Status:** triaged
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** `OoBDev.System` (`Xml/Xsl`)

## Need

`XsltTransformer` is synchronous and has open questions in its error handling and input navigation. The file extension functions (`WriteToFile`, file reads) throw on missing files or non-XML input and log through `Debug.WriteLine`. An XPath 2.0 function is unsupported.

## Proposed behavior

An async transform API; defined behavior on errors instead of "do something smart"; the input navigator can include the file; file functions return an empty set (or a documented error) for missing files or non-XML; logging through an injectable logger.

## Acceptance criteria

- Given an async caller, when a transform runs, then no thread is blocked.
- Given a missing file, when a file extension function reads it, then the documented result (empty set or error) is returned.
- Given non-XML input, then the documented result is returned without an unhandled exception.
- Given a path of null, then the message goes to the injected logger.

## Existing code checked

`docs/design/TemplateTools`, `docs/design/TextTemplating`, `README` of `OoBDev.Templating.Xslt`. They describe the feature but not these gaps.

Source comments (replaced by a reference to this request):

- `src/Framework/OoBDev.System/Xml/Xsl/XsltTransformer.cs` (4): "should make this async/await"; "do something smart"; "should probably add the file to the input navigator"; "need to figure out why this won't navigate correctly".
- `src/Framework/OoBDev.System/Xml/Xsl/Extensions/FileExtensions.cs` (3): "change to injectable console logger"; "should this be made safe to return an empty set if file not found?"; "should this be made safe to not throw if input is not XML".
- `src/Framework/OoBDev.System/Xml/Xsl/Extensions/XPath20Functions.cs`: a function "does not seem supported at this time" (commented out).

## Security and operations

File functions read and write paths from a stylesheet: keep them behind the existing path restrictions. None otherwise.

## Priority

- **Impact:** blocking transforms and unhandled errors in templates.
- **Needed by:** no date.
- **Workaround today:** none.

## Open questions

Empty set or error for a missing file? Recommendation: error by default, empty set behind an option.

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments). Design document to follow when built.
