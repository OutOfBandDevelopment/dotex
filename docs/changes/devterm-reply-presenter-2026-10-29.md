# DevTerm reply presenter and home folder

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- `OoBDev.DevTerm.Core` gains `LineReplyPresenter` (FIFO and pattern reply correlation, terminator handling, bounded buffer), `IReplyTracker`, `ReplyMatch` and `DevTermHome`.
- 13 tests (4 ported, 9 new for reply correlation); `OoBDev.DevTerm.Core.Tests` 171 passing.
- Not done: `PresenterCatalog` and the plugin loader (the catalog depends on live plugins), control servers.
