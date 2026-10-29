# Cross-check: BinaryDataDecoders against the other harvest projects

**Created:** 2026-10-22 · **Status:** proposal for owner review; extends the [recommendation plan](incoming-recommendation-plan.md). Owner rule (2026-10-22): the goal is the best feature coverage possible, not the first implementation found, so every feature is compared across all sources and the best parts are merged.

Method: for each feature, search every harvest repository for the same feature (type names, protocol keywords), then compare size, scope and tests. Line counts are `.cs` lines without `bin` and `obj`. This is a size-and-scope comparison from reading file lists and a few files; no source was built or run, and hardware behavior is not verified. Where a row says *verify*, a file-level diff is needed before work starts.

Repositories searched: `mine/binary-data-decoders` (BDD), `oobdev/dev-term`, `mine/proving-grounds`, `mine/embedded-bakery`, `mine/coding-challenges`, `mine/mycloud-monitor`, `learning/*` (arty, challenge-accepted, intel-8088-emu, tektronix-2230, event-sourcing, sync-mesh, fohjin-cqrs, postsql-playground), `_storage`, `_notes`.

## Contents

1. [Findings per feature](#1-findings-per-feature)
2. [Features with no alternative](#2-features-with-no-alternative)
3. [Changes to the plan](#3-changes-to-the-plan)

## 1. Findings per feature

**Table 1 — Best source per feature**

| Feature | Sources found | Comparison | Recommendation (merge base first, then add the rest) |
|---|---|---|---|
| NMEA GPS | BDD `Nmea` (317 lines of real decoder; the other ~6,000 lines are generated WMI notes in `Notes/`, which are not migrated); dev-term `Devices.Nmea` (486 lines, 25 tests) | dev-term covers GGA, RMC, GSA, GSV and VTG with tests; BDD has no tests and no VTG | Base on dev-term's decoder logic and tests, shaped into the BDD decoder/definition pattern; keep any BDD type that dev-term lacks (*verify* by diff). Reject the `Notes/` WMI dumps (generated, not feature code) |
| Radex One | BDD (759), dev-term (772 plus 773 test lines), notes reverse-engineering write-up | same size; dev-term adds a checksum finding and the HID-versus-serial fix from real hardware | Base on BDD, apply dev-term fixes, add dev-term's tests |
| K8055 | BDD (148), dev-term (374, 499 test lines) | dev-term is larger and tested | Base on dev-term logic (*verify* BDD request/response coverage is a subset) |
| Busylight | BDD (144, includes a `Class1` placeholder), dev-term (369, 400 test lines), proving-grounds `BusylightProvider` (48), WebHID port in the notes | dev-term is larger and tested | Base on dev-term; drop the `Class1` placeholder |
| Zoom H4n | BDD (154), dev-term (418, 349 test lines), proving-grounds `H4nCli` (261), `H4nProvider` (142), `ZoomH4n.md` (363 lines of protocol notes) | proving-grounds holds the richest protocol notes and a CLI; dev-term the tested decoder | Base on dev-term decoder, merge the command coverage from the proving-grounds CLI and keep `ZoomH4n.md` as the protocol document (*verify* command sets) |
| Fencing (Favero, Saint George) | BDD (438), proving-grounds `OoBDev.ScoreMachine` (Favero 221, SG 145, Common 305, Manager 316, Tests 164, emulator, TCP and web hosts, NetTv, HID scanner, Busylight node) | the ScoreMachine is a superset: same decoders plus publisher, emulator, TCP/web hosts and tests | Base on ScoreMachine decoders and tests; migrate the emulator (useful for tests without hardware) and publisher; the web and NetTv hosts are applications, see D8 |
| LanC | BDD (304), proving-grounds `LancMonitor` (402), Arduino relay sketches, `SonyLanc.md` | monitor is larger than BDD's command tables | Base on BDD command tables, add the monitor logic (*verify*); keep Arduino sketches as docs only |
| Code 39 barcode | BDD `Barcodes/Code39.cs` (248), `HandyClasses/Code39.cs` (247), HomeSeer `ImageTools.Core/Barcodes/Code39.cs` (249); `BarcodeData.xlsx` in the HomeSeer copy | three near-identical copies (the two proving-grounds copies differ by 15 diff lines) | Take the BDD copy; check the 15-line difference and the xlsx data for test vectors |
| Drawing (Z-order curve, multi-scale tiles, PNG pack) | BDD (697 for all of Drawing), HomeSeer `ImageTools.Core` (MultiScaleImages, PngPacker), `WhitedUS.Libs/CompressedFiles/PngPack` | HomeSeer copy is the same code | Take BDD; HomeSeer `ImageTools.Cli` and `ImageTools.Web` are application wrappers, see D8 |
| Tar and Zip | main (`Archives/Tar`, `Archives/Zip`), BDD, proving-grounds `WhitedUS.Libs` (TarGZip, PKZip headers), `_storage` `ArchiveWalker` (150) | main and BDD cover the formats; the older `WhitedUS.Libs` is the likely ancestor | Keep main; check `_storage` `ArchiveWalker` for formats main lacks (*verify*); reject `WhitedUS.Libs` copies (older ancestor) |
| ISO 9660 | main, BDD, `_storage` `DiskImageReader` (46) | `_storage` copy is smaller | Keep main; reject the `_storage` copy (smaller, no extra features seen) |
| Classic ciphers | main (Caesar, Enigma, Lorenz, PlayFair), `learning/challenge-accepted` (Caesar, Vigenère with tests) | challenge-accepted adds Vigenère | Add Vigenère (*verify* main lacks it) with its tests under the educational warning |
| Expression calculator | main, BDD | nothing else found (proving-grounds hits are unrelated tokenizers) | Keep main |
| Apple II | main, BDD | no other source found | Keep main |

## 2. Features with no alternative

CodeAnalysis (semantic and syntax navigators), DacPac navigator and StructuredLog navigator exist only in BDD; no harvest repository contains Roslyn, DacFx or binlog code (searches for `SemanticModel`, `DacPac`, `DacFx`, `StructuredLog` found nothing). They migrate from BDD as planned. The PackMan, Xslt and Net.ServiceHost CLIs also have a single source.

## 3. Changes to the plan

- D2 becomes per device (see the plan): the base is the best-covered, tested implementation, not always BDD.
- New decision **D8**: the ScoreMachine and HomeSeer applications contain both library code and applications (web, TCP, NetTv, Arduino). Library parts are harvested; applications are not.
- New decision **D9**: add Vigenère from challenge-accepted.
- Rejected with reason: BDD `Nmea/Notes/*.cs` (generated WMI dumps), `WhitedUS.Libs` compression copies (older ancestor of the main implementation), `_storage` ISO reader (smaller than main's).
- A file-level diff (the *verify* rows) runs at the start of each migration step, so the merge is based on evidence.

**Table 2 — New decisions**

| # | Question | Options | Recommendation |
|---|---|---|---|
| D8 | ScoreMachine and HomeSeer applications | **A** harvest library and emulator code only; **B** also migrate the web and TCP hosts as an example; **C** nothing beyond the BDD scope | **A**: best coverage without importing applications |
| D9 | Add Vigenère from challenge-accepted | **A** yes, educational package; **B** no | **A**: more cipher coverage at a few dozen lines |

## 4. Device decoding phase 0 (2026-10-28)

The five devices that exist in both `mine/binary-data-decoders` and dev-term were compared in [device-parity.md](device-parity.md); dev-term is the base. Merges M1 to M5 are listed there. Rejected with reason: the BinaryDataDecoders Busylight `Class1` hardware loop (experiment code), the K8055 request and response structs (dev-term decodes the same offsets), the NMEA classes and notes (dev-term covers GGA, RMC, GSA, GSV and VTG), the Radex One write-settings request (setting fields zeroed, incomplete) and the Zoom H4n request enum and decoder (same codes and status bits as dev-term, which adds the handshake).
