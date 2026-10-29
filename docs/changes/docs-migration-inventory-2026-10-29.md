# Migration notes inventory (2026-10-29)

`docs/migration/README.md` was a January 2026 BinaryDataDecoders status page; it is now an inventory of every migration note with its state. The BotChat mapping and plan (patterns extracted 2026-10-09, sources removed) and the MailKit and SendGrid analysis (both integrations complete, covered by their readmes) were removed. The `Incoming/CHECKLIST.md` links to them were replaced by a note.

The vector comparison note was removed too: `Vector`, `VectorMath` and `VectorComparer` are in `OoBDev.System.Abstractions` with a readme section and 9 more tests (19 in `OoBDev.System.Tests/Math`). Rough edge for the backlog: `VectorComparer` compares the distance with exactly zero, so cosine rounding makes equal vectors unequal.

Left (backlog, owner request of 2026-10-29): review the BinaryDataDecoders, SharedFramework, Oobtainium and vector notes one by one, drop what is done and turn the rest into feature documents. Docs validators: 0 problems over `docs`.
