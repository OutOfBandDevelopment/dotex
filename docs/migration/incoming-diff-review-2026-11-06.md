# Incoming diff review (2026-11-06)

**Status:** Complete; moved out of `BACKLOG.md` unchanged (hygiene, 2026-11-09).

Apple2, FileSystems (ISO 9660), Archives (Zip and Tar: properties, `var`, nullable annotations, same logic) and Cryptography (Caesar, Vigenere, PlayFair spot-checked: collection expressions and extracted helpers, same behavior; PlayFair is unfinished in both) reviewed 2026-11-06: Apple2 had one bug in main (fixed); the ISO 9660 delta is a refactor (nullable, `init` properties, `ReadDateTime` extracted, `InterlaveGapSize` renamed `InterleaveGapSize`); the incoming copies of both can go with the others.

ExpressionCalculator reviewed too (one more bug fixed in main).

Still to review: Net and ToolKit; FormattableNumber, TaskEx, ObservableDictionary and TypeEx are read (refactors: nullable, helpers, analyzer fixes; no defects); LdapFilterBuilder and LdapSimpleFilter are read (one regression fixed); XsltTransformer, PathNavigatorFactory, XmlSchemaValidatorEx and XFragment are read (refactors and null guards; the duplicate `XFragment` was merged).

In ToolKit, `AsNodeSet(object)` was lost and restored; ObjectXmlExtensions, EngineeringNumber, OneTimeCode, ExtensibleElementNode, PathEx, ReversableEnumerator, ColorConversion, TempFileHandle, BcdEx, LightCalculator, DoubleLinkedList and ApplicationInformation are refactors only.

Drawing (Code39 table equals the incoming one and is tested; JpegSegmentReader and PngPack are deliberate SkiaSharp rewrites with bounds checks) and CodeAnalysis (the semantic-model navigator yields the symbol node instead of the syntax node as a child, a deliberate redesign covered by 12 tests; the syntax-tree factory is a refactor) are read.

ToolKit.Abstractions, ToolKit.Tests, Nmea, Windows.Forms, TestUtilities and the CLI projects are the remaining diff review.

Diff-then-delete section 1, first pass (2026-11-06, file names normalised, usings and doc comments ignored): every incoming file of Apple2, ExpressionCalculator, Archives, FileSystems, Cryptography and Net has a counterpart in main (SECURITY_ATTRIBUTES is `SecurityAttributes`), but the bodies differ (Apple2 91 changed lines over 11 files, ExpressionCalculator 327 over 34, Archives 396 over 14, FileSystems 399 over 4, Cryptography 378 over 9, Net 248 over 24), so nothing is deleted yet; the per-file review of those deltas against `C:epominebinary-data-decoders` is the remaining work.

Fencing SgStateParser reviewed 2026-11-06: the score-and-cards split byte (`_C` is 0x3a) and the EOT check (`Eotr` is 0x04) equal the incoming code; no defect.

Favero parser (same bit decoding; adds a frame-length guard), Fencer and ScoreMachineState (records replace hand-written equality) reviewed too; the Fencing incoming copy can go.

Nmea reviewed (redesign; the geoid height lost from GGA was restored); the incoming copy can go.

ToolKit.Abstractions (all 16 files equal after normalising), TestUtilities (refactors: `TimeProvider`, async `ExecuteAsync`, caller info; `AddResultFile` moved to `TestContextExtensions`) reviewed: no defects.

Windows.Forms: `ValidTextBox` is still commented out in main, decided in [the plan](docs/design/WindowsForms/migration-plan.md).

ToolKit.Tests (the numeric tests live together in `NumberNotationTests`; the rest keep their file names), the device libraries (merged on 2026-11-04, [device-parity.md](docs/migration/device-parity.md)) and the CLI projects ([re-review](docs/migration/binarydatadecoders-rereview-2026-10-29.md) tables) are covered.

The diff review of `Incoming/BinaryDecoders` is complete; the section 1 incoming copies were deleted 2026-11-06 (D1), the rest stay until their migration closes.
