# BinaryDataDecoders — Decision Sheet

**Created:** 2026-10-09 · **Purpose:** one line per open decision, with a recommendation, so each can be answered yes or no.

> **Parent:** [Critical Questions](./binarydatadecoders-critical-questions.md) · [TODO-decisions.md](../../TODO-decisions.md) · [OPEN_QUESTIONS.md](../../OPEN_QUESTIONS.md) (Q9)

Rules applied to the recommendations: every feature is migrated (phases are priority order, see [CLAUDE.md](../../CLAUDE.md)); breaking changes are fine while the framework is unreleased; prefer platform primitives and first-party code over third-party libraries; obsolete platforms are the only deletions.

## Contents

- [How to answer](#how-to-answer)
- [Phase 1 — Foundation](#phase-1--foundation)
- [Phase 2 — High-value features](#phase-2--high-value-features)
- [Phase 3 — Protocols](#phase-3--protocols)
- [Phase 4 — Specialized](#phase-4--specialized)
- [Phase 5 — Future](#phase-5--future)

## How to answer

Write **Y** (accept the recommendation) or the change you want in the *Answer* column. Rows marked *needs you* depend on a use case only you know; the recommendation is the default if you do not answer.

## Phase 1 — Foundation

**Table 1 — Phase 1 decisions**

| # | Decision | Recommendation | Needs you | Answer |
|---|----------|----------------|-----------|--------|
| 1 | Endianness: enhance `EndianType` rather than add a parallel class | Yes, enhance in place | | |
| 2 | Endianness API surface | All three: static helpers, value extension methods, `BinaryReader`/`BinaryWriter` extensions | | |
| 3 | Include runtime endianness detection | Yes, expose `BitConverter.IsLittleEndian` through `EndianType` (no custom probing) | | |
| 4 | `BinaryPrimitives` naming | `ReadInt32BigEndian` style, matching `System.Buffers.Binary` | | |
| 5 | UI collections location | New `OoBDev.Extensions.UI.Collections`, framework-agnostic | | |

## Phase 2 — High-value features

**Table 2 — Phase 2 decisions**

| # | Decision | Recommendation | Needs you | Answer |
|---|----------|----------------|-----------|--------|
| 6 | CodeAnalysis (Roslyn extensions): migrate | Yes, as an Extensions-layer package; also the home for the OoBDev analyzers | Intended use | |
| 7 | ExpressionCalculator: audit then migrate | Yes; migrate as-is, track gaps in TODO | | |
| 8 | Archive formats | TAR and CPIO migrated as-is (first-party); ZIP via `System.IO.Compression`, no new dependency | Need for 7z/RAR | |
| 9 | Archive operations | Read, write, streaming and format detection for TAR/CPIO | | |
| 10 | Archive implementation | Keep managed implementation; no SharpCompress | | |
| 11 | Bit-level operations | Migrate as-is | Protocol-parsing use case | |

## Phase 3 — Protocols

**Table 3 — Phase 3 decisions**

| # | Decision | Recommendation | Needs you | Answer |
|---|----------|----------------|-----------|--------|
| 12 | NMEA: scope | Sentence parsing from streams and files first; serial hardware glue after | GPS hardware or files | |
| 13 | NMEA: sentence set | Everything present (GGA, RMC, GSA, GSV and others), checksum validation required | | |
| 14 | Drawing and geometry | Migrate the geometry types; use SkiaSharp (already in the repo) for rendering instead of rewriting | | |
| 15 | Barcode | Keep the BinaryDataDecoders encoders; use ZXing.Net only for formats it has that they lack | Formats needed | |

## Phase 4 — Specialized

**Table 4 — Phase 4 decisions**

| # | Decision | Recommendation | Needs you | Answer |
|---|----------|----------------|-----------|--------|
| 16 | ISO 9660 | Migrate, separate package | | |
| 17 | Classic cryptography | Migrate to `OoBDev.Security.Cryptography.Classic` with security warnings, educational only | | |
| 18 | Apple II / retro | Migrate to `OoBDev.Retro.Apple2`, separate package | | |
| 19 | Hardware devices (8) | Migrate all, separate `OoBDev.Extensions.Hardware.*` packages; mark untested-on-hardware in each readme | Which are in active use | |
| 20 | Rigol (stub, no implementation) | Delete | | |
| 21 | CLI tools (4) | Migrate all as dotnet tools; share a host project | Which are used | |
| 22 | Windows Forms validation controls | Migrate to `OoBDev.Extensions.Windows.Forms`, net10.0-windows | | |
| 23 | UWP code | Delete (deprecated, no net10.0 equivalent) | | |
| 24 | .NET Framework-only code | Port to net10.0 where it compiles, otherwise delete | | |
| 25 | Silverlight, Compact Framework | Delete | | |
| 26 | Unity-specific code | Assess per file; keep only what compiles standalone | | |

## Phase 5 — Future

**Table 5 — Phase 5 decisions**

| # | Decision | Recommendation | Needs you | Answer |
|---|----------|----------------|-----------|--------|
| 27 | DeepZoom WPF viewer | Defer until migration completes; MVVM with real command types | | |
| 28 | DeepZoom JS/TS viewer | Defer; pick a framework with view-model binding | | |

---

*The earlier tracker counted 39 open items; they collapse into the 28 rows above because several shared a single question. Rows 6, 8, 11, 12, 15, 19 and 21 are the ones that need your use case.*

[↑ Critical Questions](./binarydatadecoders-critical-questions.md)
