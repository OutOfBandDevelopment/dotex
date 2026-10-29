# BinaryDataDecoders re-review

**Date:** 2026-10-29

The owner answered Q1 on 2026-10-29: the earlier recommendation plan was out of date and BinaryDataDecoders had to be reviewed again before integration. [binarydatadecoders-rereview-2026-10-29.md](../migration/binarydatadecoders-rereview-2026-10-29.md) compares the newest source (`e590b420`, 78 project folders) with current main by type name, by file and by file size, and replaces the BinaryDataDecoders parts of the old plan and decision sheet (both carry a superseded note).

Result: the eight device packages and most of the core libraries are in main. Seven items are not: Numerics, NMEA helpers, Drawing, Code analysis navigators, Fencing, LanC, and the PackMan and Xslt command lines. Each is a line in `BACKLOG.md`, in the order of Table 4 of the review. Rigol, EByte, the IO Controller CLI, the service host and the sample projects are not migrated, with reasons in Table 3.

**Also recorded:** the Q3 answer (option A before the Avalonia spike; dev-term shells become tools) in `BACKLOG.md` and the device decoding migration plan.

**Verified:** documentation only. `OoBDev.Docs.Tests` on `docs/migration`: 4 passed. The comparison is by name and file, not behaviour; the remaining checks are listed in the review and in `BACKLOG.md`.
