# Feature request: XSLT transformer and extension function improvements

- **Status:** done (built 2026-11-17, see [change](../../changes/2026-11-17.md#xslt-transformer-and-file-function-improvements))
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

## Security and operations

File functions read and write paths from a stylesheet: keep them behind the existing path restrictions. None otherwise.

## Priority

- **Impact:** blocking transforms and unhandled errors in templates.
- **Needed by:** no date.
- **Workaround today:** none.

## Open questions

Decided: error by default (`FileNotFoundException`, `XmlException`), empty set behind `emptyOnUnreadable`.

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments). Design document to follow when built.

## Outcome

`IXsltTransformer.TransformAsync` reads the input with an async reader, runs the transform and writes through a temp file that replaces the output (a failed run leaves no partial output). `FileExtensions.ReadXml` throws `FileNotFoundException` for a missing file and `XmlException` for non-XML by default, or returns an empty document and logs a warning when created with `emptyOnUnreadable: true`; `WriteToFile` logs through an injected `ILogger` (default no-op) instead of `Debug.WriteLine`. Not pursued, comments removed: the file in the input navigator and the odd navigation (the merge path reloads the merged XML and works), and the XPath 2.0 `apply` function (the commented draft never worked; no consumer). XSLT itself has no async API, so the transform runs on a pool thread. Unit tests: 4 added (Xsl and Xml 45 passed, 1 skipped).
