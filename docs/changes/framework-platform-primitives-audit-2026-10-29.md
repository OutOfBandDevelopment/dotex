# Platform primitives audit (2026-10-29)

Scanned `src` for hand-built providers that duplicate a platform type. The only provider left is `IGuidProvider` / `GuidProvider` (`OoBDev.System`), which has no platform equivalent (.NET has `TimeProvider` but nothing for `Guid`); it stays as the seam that lets tests fix generated ids (used by the Handlebars `guid` helper). No `DateTime.Now`/`UtcNow` calls remain in non-test code. The backlog item is closed.
