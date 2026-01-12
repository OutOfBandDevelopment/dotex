# BinaryDataDecoders Migration - Critical Questions

**Date:** 2026-01-12
**Status:** Awaiting Decisions
**Purpose:** Document all questions requiring answers before proceeding with migration

---

## Overview

This document lists all critical questions that must be answered before migrating features from BinaryDataDecoders to the OoBDev framework. Questions are organized by migration phase and priority.

**Migration Status:**
- ✅ **Phase 0:** Complete (6 bug fixes applied)
- ⏸️ **Phases 1-5:** Blocked pending decisions below

---

## Phase 1: Foundation Enhancement

### 1.1 Endianness Support - API Design

**Context:** BinaryDataDecoders has `Utilities/Endian.cs` with runtime detection and conversion methods.

**Questions:**

1. **Should we enhance existing `EndianType.cs` or create new classes?**
   - Option A: Enhance `OoBDev.System/BinaryPrimitives/EndianType.cs` (UPDATE)
   - Option B: Create new `EndianConverter` class alongside `EndianType`
   - Option C: Create extension methods for `BinaryReader`/`BinaryWriter`

2. **What API surface do you prefer?**
   - Option A: Static methods (`EndianConverter.ToLittleEndian(value)`)
   - Option B: Extension methods (`value.ToLittleEndian()`)
   - Option C: BinaryReader extensions (`reader.ReadInt32BigEndian()`)
   - Option D: All of the above

3. **Should we include runtime endianness detection?**
   - Used for platform-specific optimizations
   - Most modern systems are little-endian
   - Is this needed for your scenarios?

**Recommendation:** Option D (all approaches) for maximum flexibility

---

### 1.2 BinaryPrimitives Extensions - Naming Convention

**Context:** Need to add endian-aware `BinaryReader`/`BinaryWriter` extension methods.

**Questions:**

1. **Method naming convention?**
   - Option A: `ReadInt32BigEndian()` / `ReadInt32LittleEndian()`
   - Option B: `ReadInt32BE()` / `ReadInt32LE()` (shorter)
   - Option C: `ReadBigEndianInt32()` / `ReadLittleEndianInt32()` (prefix style)

2. **Should we support bit-level reading?**
   - `ReadBits(count)` - Read N bits
   - `ReadBitField<T>(offset, length)` - Read bit field
   - Used for packed binary protocols

3. **Where should these live?**
   - Option A: `OoBDev.System/BinaryPrimitives/BinaryReaderExtensions.cs`
   - Option B: `OoBDev.System/IO/BinaryReaderExtensions.cs`

**Recommendation:** Option A (ReadInt32BigEndian style) - matches .NET naming conventions

---

### 1.3 UI/MVVM Collections - Project Location

**Context:** BinaryDataDecoders has `ObservableDictionary` and other MVVM collections for WPF/Windows Forms.

**Questions:**

1. **Where should UI collections live?**
   - Option A: `OoBDev.Extensions.UI.Collections` (new project)
   - Option B: `OoBDev.Extensions.WPF.Collections` (WPF-specific)
   - Option C: `OoBDev.System.Collections.Observable` (Framework layer)
   - Option D: Multiple projects (WPF, WinForms, Blazor specific)

2. **Which collections should we migrate?**
   - [ ] `ObservableDictionary<TKey, TValue>` - INotifyPropertyChanged + INotifyCollectionChanged
   - [ ] Other observable collections (list which ones you need)

3. **Should they be .NET 9.0 WPF-only or support Blazor/MAUI?**
   - WPF-only: Simpler, smaller surface
   - Multi-framework: More complexity, broader use

**Recommendation:** Option A (OoBDev.Extensions.UI.Collections) - framework-agnostic, then specialized wrappers if needed

---

## Phase 2: High-Value Features

### 2.1 CodeAnalysis - Roslyn Integration Strategy

**Context:** BinaryDataDecoders has CodeAnalysis with Roslyn extensions, analyzers, and visitors.

**Questions:**

1. **What is the primary use case?**
   - [ ] Code generation (T4, Source Generators)
   - [ ] Static analysis (Roslyn Analyzers)
   - [ ] Code refactoring tools
   - [ ] Build-time code inspection
   - [ ] Runtime code analysis

2. **Should we create Roslyn Analyzers or just helper libraries?**
   - Option A: Full Roslyn Analyzer packages (publishable to NuGet)
   - Option B: Helper libraries only (for custom analyzers)
   - Option C: Both

3. **Namespace preference?**
   - Option A: `OoBDev.CodeAnalysis.*` (generic)
   - Option B: `OoBDev.Roslyn.*` (specific to Roslyn)
   - Option C: `OoBDev.Extensions.CodeAnalysis.*` (Extensions layer)

4. **Project structure?**
   - Option A: Single project `OoBDev.CodeAnalysis`
   - Option B: Split: `OoBDev.CodeAnalysis` (core) + `OoBDev.CodeAnalysis.CSharp` (language-specific)
   - Option C: Further split: Abstractions, CSharp, Analyzers, Generators

**Recommendation:** Need use case clarification - what do you want to build with it?

---

### 2.2 ExpressionCalculator - Scope Verification

**Context:** OoBDev already has `ExpressionCalculator/`. Need to verify vs BinaryDataDecoders version.

**Questions:**

1. **Should we audit existing vs incoming?**
   - We already fixed `ShiftCommutativeVariablesRight.cs`
   - Are there other missing optimizers or features?
   - Should we do comprehensive comparison?

2. **What's the priority?**
   - Option A: HIGH - Audit now before other migrations
   - Option B: MEDIUM - Audit during Phase 2
   - Option C: LOW - Current implementation works, defer

3. **Do you use these advanced features?**
   - [ ] Custom expression optimizers
   - [ ] ANTLR-based parser
   - [ ] Custom evaluators
   - [ ] Expression tree manipulation

**Recommendation:** LOW priority unless you're actively using advanced features

---

### 2.3 Archive Support - Format Decisions

**Context:** BinaryDataDecoders has TAR, CPIO, and possibly ZIP support.

**Questions:**

1. **Which archive formats do you need?**
   - [ ] TAR (.tar, .tar.gz, .tar.bz2)
   - [ ] CPIO (.cpio)
   - [ ] ZIP (.zip) - or use System.IO.Compression?
   - [ ] 7-Zip (.7z)
   - [ ] RAR (.rar) - read-only?

2. **What operations are required?**
   - [ ] Read-only (extract)
   - [ ] Write (create/modify)
   - [ ] Streaming (large file support)
   - [ ] Format detection

3. **Should we integrate with existing libraries?**
   - Option A: Pure managed implementation (full control)
   - Option B: Use SharpCompress or similar (less code)
   - Option C: Wrapper around native libraries

**Recommendation:** Clarify specific use cases - do you need TAR/CPIO for legacy system access?

---

### 2.4 BinaryData Enhancements - Bit-Level Operations

**Context:** BinaryDataDecoders may have bit-level operations and checksum utilities.

**Questions:**

1. **What checksum algorithms do you need?**
   - [ ] CRC8, CRC16, CRC32, CRC64
   - [ ] Fletcher checksum
   - [ ] Adler-32
   - [ ] Custom checksums

2. **Bit-level operations required?**
   - [ ] Bit reading/writing
   - [ ] Bit packing/unpacking
   - [ ] Bit field manipulation
   - [ ] Binary pattern matching

3. **Should these be part of System or separate?**
   - Option A: `OoBDev.System/BinaryData/` (core functionality)
   - Option B: `OoBDev.Extensions.BinaryData/` (specialized)

**Recommendation:** Need specific use cases - protocol parsing? Data integrity?

---

## Phase 3: Protocol Support

### 3.1 NMEA Protocol - Implementation Depth

**Context:** BinaryDataDecoders has NMEA 0183 protocol support (GPS/marine).

**Questions:**

1. **What NMEA sentence types do you need?**
   - [ ] GGA (Fix data)
   - [ ] RMC (Recommended minimum)
   - [ ] GSA (Satellite status)
   - [ ] GSV (Satellites in view)
   - [ ] VTG (Track and speed)
   - [ ] All common types
   - [ ] Custom/proprietary sentences

2. **Read-only or read-write?**
   - Option A: Parse only (read NMEA data)
   - Option B: Format only (generate NMEA data)
   - Option C: Both

3. **Integration with existing pipelines?**
   - Should NMEA use `OoBDev.System.IO.Pipelines` patterns?
   - Integration with serial port communication?

**Recommendation:** Clarify use case - GPS hardware integration? Data file parsing?

---

### 3.2 Drawing/Geometry - Library Decision

**Context:** BinaryDataDecoders has Drawing namespace. .NET has System.Drawing, SkiaSharp, ImageSharp.

**Questions:**

1. **Should we migrate Drawing or use existing libraries?**
   - Option A: Migrate (full control, no dependencies)
   - Option B: Use System.Drawing (Windows-only, legacy)
   - Option C: Use SkiaSharp (cross-platform, modern)
   - Option D: Use ImageSharp (managed, cross-platform)
   - Option E: Minimal abstractions, support multiple backends

2. **What features are in BinaryDataDecoders Drawing?**
   - Barcode generation/reading?
   - DeepZoom tile generation? (already confirmed)
   - Geometry primitives?
   - Image manipulation?

3. **Cross-platform requirement?**
   - Windows-only acceptable?
   - Need Linux/macOS support?

**Recommendation:** Review BinaryDataDecoders Drawing to see what's actually there, then decide

---

### 3.3 Barcode Support - Library vs Implementation

**Context:** Barcode generation/reading may be in Drawing namespace.

**Questions:**

1. **What barcode formats do you need?**
   - [ ] QR Code
   - [ ] Code 39
   - [ ] Code 128
   - [ ] EAN/UPC
   - [ ] PDF417
   - [ ] Data Matrix
   - [ ] Aztec Code

2. **Generate, read, or both?**
   - [ ] Generate barcodes (encoding)
   - [ ] Read barcodes (decoding/OCR)
   - [ ] Both

3. **Should we use existing libraries?**
   - Option A: ZXing.Net (mature, full-featured)
   - Option B: BarcodeLib (simple generation)
   - Option C: Custom implementation (from BinaryDataDecoders)

**Recommendation:** Use ZXing.Net unless BinaryDataDecoders has unique features

---

## Phase 4: Specialized Domains

### 4.1 ISO 9660 Filesystem - Use Case Validation

**Context:** BinaryDataDecoders has ISO 9660 CD/DVD filesystem support.

**Questions:**

1. **What's the use case?**
   - [ ] Reading CD/DVD images (.iso files)
   - [ ] Legacy data recovery
   - [ ] Creating ISO images
   - [ ] Direct optical media access
   - [ ] Educational/archival purposes

2. **Read-only or read-write?**
   - Option A: Read-only (simpler, most common)
   - Option B: Read-write (complete implementation)

3. **Should we support extensions?**
   - [ ] Joliet (long filenames, Unicode)
   - [ ] Rock Ridge (POSIX permissions, symlinks)
   - [ ] El Torito (bootable CDs)

**Recommendation:** Clarify if this is actively needed - ISO 9660 is legacy technology

---

### 4.2 Classic Cryptography - Educational Packaging

**Context:** BinaryDataDecoders has Enigma, Lorenz, Caesar, Vigenère, PlayFair ciphers.

**Questions:**

1. **Confirm educational use only?**
   - These are NOT secure for production
   - Purpose: CTF challenges, education, historical simulation
   - Agree to strong security warnings?

2. **Should this be a separate package?**
   - Option A: Separate NuGet package `OoBDev.Security.Cryptography.Classic`
   - Option B: Part of main framework with `[Obsolete]` warnings
   - Option C: Don't migrate (too risky)

3. **Documentation requirements?**
   - Strong warnings in README
   - `[Obsolete]` attributes on all classes
   - XML doc warnings
   - Security disclosure in package description

**Recommendation:** Separate package with extensive warnings - valuable for education/CTF

---

### 4.3 Apple II / Retro Computing - Priority

**Context:** BinaryDataDecoders has Apple II disk format and DOS 3.3 filesystem support.

**Questions:**

1. **Is this actively used?**
   - [ ] Legacy data recovery projects
   - [ ] Retro computing hobby
   - [ ] Historical preservation
   - [ ] Not used (low priority)

2. **What formats are needed?**
   - [ ] .DSK disk images
   - [ ] .NIB nibble images
   - [ ] DOS 3.3 filesystem
   - [ ] ProDOS filesystem
   - [ ] Other formats?

3. **Migration priority?**
   - Option A: HIGH - Actively used
   - Option B: MEDIUM - Occasionally used
   - Option C: LOW - Archive/preserve only

**Recommendation:** Need active use case validation

---

### 4.4 Hardware Devices - Priority Ranking

**Context:** BinaryDataDecoders has 8 specialized hardware device libraries.

**Questions:**

1. **Which devices are actively used?** (Rank by priority)
   - [ ] Kuando Busylight (presence indicators)
   - [ ] RadexOne (radiation detection)
   - [ ] Velleman K8055 (experiment board)
   - [ ] Zoom H4n (audio equipment)
   - [ ] Fencing equipment (sport automation)
   - [ ] LANC (camera protocol)
   - [ ] EByte modules (LoRa/IoT)
   - [ ] ZWave (home automation)

2. **For each HIGH priority device:**
   - What operations are needed?
   - USB, serial, or network communication?
   - Read-only or control?

3. **Should these be individual packages?**
   - Option A: One package per device
   - Option B: Single `OoBDev.Extensions.Hardware` with all devices
   - Option C: Group by category (IoT, Media, Lab Equipment, etc.)

**Recommendation:** Only migrate devices you actively use - hardware libraries require testing with physical devices

---

### 4.5 CLI Tools - Deployment Strategy

**Context:** BinaryDataDecoders has 4 CLI tools: IO.Controller.Cli, ServiceHost.Cli, PackMan.Cli, Xslt.Cli.

**Questions:**

1. **Which CLI tools are actively used?**
   - [ ] IO.Controller.Cli - Device control
   - [ ] ServiceHost.Cli - Network service hosting
   - [ ] PackMan.Cli - Package management
   - [ ] Xslt.Cli - XSLT transformations (merge with TemplateEngine.Cli?)

2. **Deployment method?**
   - Option A: .NET Global Tools (`dotnet tool install -g`)
   - Option B: Framework-dependent deployments
   - Option C: Self-contained executables
   - Option D: Docker containers

3. **Should Xslt.Cli merge with existing TemplateEngine.Cli?**
   - Consolidate XSLT into existing template engine?
   - Keep separate?

**Recommendation:** Only migrate actively used tools - each requires packaging and maintenance

---

### 4.6 Windows Forms - Modernization Approach

**Context:** BinaryDataDecoders has Windows Forms validation controls.

**Questions:**

1. **Are you using Windows Forms in new projects?**
   - Or is this legacy support only?
   - Should we modernize for .NET 9.0 WinForms?

2. **What controls/features exist?**
   - Custom validators?
   - Data binding helpers?
   - UI validation components?
   - Other controls?

3. **Integration with WPF or standalone?**
   - Some apps use both WinForms and WPF
   - Should controls be portable?

**Recommendation:** Audit what exists first, then decide migration approach

---

### 4.7 Platform-Specific Code - Modernization Decisions

**Context:** BinaryDataDecoders may have UWP, .NET Framework-specific, or other platform code.

**Questions:**

1. **UWP code - modernize or delete?**
   - Option A: Port to Windows App SDK (WinUI 3)
   - Option B: Port to .NET MAUI
   - Option C: Delete (UWP is deprecated)

2. **.NET Framework-specific code?**
   - Option A: Port to .NET 9.0
   - Option B: Mark as obsolete, keep for reference
   - Option C: Delete

3. **Other platform-specific features?**
   - Silverlight? (DELETE)
   - .NET Compact Framework? (DELETE)
   - Unity? (assess case-by-case)

**Recommendation:** Delete deprecated platforms unless specific value identified

---

## Phase 5: Future Development

### 5.1 DeepZoom Viewers - Technology Choices

**Context:** Plan to create NEW DeepZoom viewer controls (WPF and JavaScript/TypeScript).

**Note:** This is NEW development, not migration. Tile generation already migrated.

**Questions for WPF Viewer:**

1. **WPF technology stack?**
   - Pure WPF controls?
   - Use WebView2 (Chromium) for rendering?
   - Use SkiaSharp for custom rendering?

2. **Tile source providers?**
   - HTTP/HTTPS tile servers?
   - Local filesystem?
   - Embedded resources?
   - Custom providers (database, cloud storage)?

**Questions for JavaScript/TypeScript Viewer:**

1. **Rendering approach?**
   - Option A: Canvas 2D
   - Option B: WebGL (better performance)
   - Option C: Both (fallback)

2. **Build system?**
   - Vite? Webpack? Rollup? ESBuild?
   - TypeScript compiler configuration?

3. **Framework wrappers priority?**
   - [ ] React (HIGH)
   - [ ] Angular (MEDIUM)
   - [ ] Vue (MEDIUM)
   - [ ] Svelte (LOW)
   - [ ] Vanilla JS only

4. **NPM package scope?**
   - `@oodev/deepzoom-viewer`
   - Different name?

**Recommendation:** Defer until migration complete, but good to plan ahead

---

## Summary: Critical Path Decisions

To **unblock migration**, we need decisions on:

### Immediate (Phase 1):
1. ✅ **Endianness API design** - Extension methods preferred?
2. ✅ **BinaryPrimitives naming** - ReadInt32BigEndian style?
3. ✅ **UI Collections location** - OoBDev.Extensions.UI.Collections?

### High Priority (Phase 2):
4. ❓ **CodeAnalysis use case** - What are you building?
5. ❓ **Archive formats needed** - TAR? CPIO? Use cases?
6. ❓ **ExpressionCalculator audit** - Is current implementation sufficient?

### Medium Priority (Phase 3):
7. ❓ **NMEA use case** - GPS hardware? Data files?
8. ❓ **Drawing/Geometry decision** - Migrate or use existing library?
9. ❓ **Barcode requirements** - Formats needed?

### Lower Priority (Phase 4):
10. ❓ **Hardware devices ranking** - Which are actively used?
11. ❓ **CLI tools active usage** - Which to migrate?
12. ❓ **Retro computing priority** - Active use cases?

---

## Next Steps

1. **Review this document** - Identify decisions you can make immediately
2. **Provide answers** - Inline or in separate document
3. **Prioritize** - Which features are HIGH vs MEDIUM vs LOW priority
4. **Start migration** - Begin with decisions that are clear

**Recommendation:** Start with Phase 1 (Foundation) - these are broadly useful regardless of specialized feature decisions.

---

## Related Documents

- [BinaryDataDecoders Feature Mapping](./binarydatadecoders-feature-mapping.md)
- [BinaryDataDecoders Migration Plan](./binarydatadecoders-migration-plan.md)
- [TODO.md](../../TODO.md) - Comprehensive tracking
