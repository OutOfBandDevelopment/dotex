# DevTerm text presenters

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- New `OoBDev.DevTerm.Presenters.Text` (Extensions): `ascii` (line-buffered, CR/LF/CRLF and max-length handling, resettable), `utf8` (stateful decoder), `hex`, `decimal`, `octal`, `binary`; `TryAddTextPresenters()`.
- `OoBDev.DevTerm.Core` gains `IPresenterInput`, `IResettablePresenter` and `PresenterDataExtensions.ToContiguousSpan`.
- 36 unit tests pass (the dev-term suite ported, with the bug-regression cases, plus registration). Test categories collapse to `Unit`.
- Not done: other presenters (KSY frame, SCPI), which follow with manifests.
