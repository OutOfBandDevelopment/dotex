# Migration TODO - BinaryDataDecoders

**Project:** Incoming/BinaryDecoders
**Size:** ~500 files, ~50,000 LOC
**Source:** Incoming/BinaryDecoders/
**Status:** ⏸️ BLOCKED - Awaiting 14+ critical decisions
**Priority:** HIGH (after decisions made)

---

## Overview

**What This Is:**
Massive binary data processing, protocols, and hardware communication library with:
- Binary primitive types and endianness support
- Network protocols (NMEA, etc.)
- Archive formats (TAR, CPIO, ZIP, ISO 9660)
- Hardware device communication (8+ devices)
- Classic cryptography (educational)
- Drawing/geometry utilities
- Barcode generation
- Expression calculator
- Windows Forms UI components
- Retro computing support (Apple II)

**Migration Philosophy:**
ALL features will be migrated - phases indicate priority order, not feature selection.

---

## Current Status

**Investigation:** ✅ COMPLETE - Feature mapping, migration plan, critical questions documented
**Decisions:** ⏸️ PENDING - 14+ questions across 4 priority levels
**Migration:** 🚫 BLOCKED - Cannot proceed without decisions

---

## Blocking Decisions

**See:** [BinaryDataDecoders Critical Questions](docs/migration/binarydatadecoders-critical-questions.md)

### Immediate Priority (Phase 1 - Foundation)
- [ ] **Endianness API design** - Extension methods, static methods, or both?
- [ ] **BinaryPrimitives naming** - `ReadInt32BigEndian()` style preferred?
- [ ] **UI Collections location** - Create `OoBDev.Extensions.UI.Collections`?

### High Priority (Phase 2 - High-Value Features)
- [ ] **CodeAnalysis use case** - What are you building with Roslyn extensions?
- [ ] **Archive formats** - Which formats needed (TAR, CPIO, ZIP)?
- [ ] **ExpressionCalculator audit** - Is current implementation sufficient?

### Medium Priority (Phase 3 - Protocols)
- [ ] **NMEA Protocol** - GPS hardware integration or data file parsing?
- [ ] **Drawing/Geometry** - Migrate or use existing libraries (SkiaSharp, ImageSharp)?
- [ ] **Barcode** - Which formats? Use ZXing.Net or custom?

### Lower Priority (Phase 4 - Specialized)
- [ ] **Hardware devices** - Which of the 8 devices are actively used?
- [ ] **CLI tools** - Which of the 4 tools should be migrated?
- [ ] **ISO 9660, Apple II, Classic Crypto** - Active use cases?
- [ ] **Windows Forms, UWP** - Modernization approach?

---

## Migration Phases (After Decisions)

### Phase 1: Foundation Enhancement
- [ ] Endianness support enhancements
- [ ] Utility enhancements
- [ ] BinaryPrimitives expansion
- [ ] Core infrastructure

### Phase 2: High-Value Features
- [ ] Archive formats (TAR, CPIO, ZIP, ISO 9660)
- [ ] Code analysis (Roslyn extensions)
- [ ] Expression calculator
- [ ] Drawing/geometry (if keeping custom)
- [ ] Barcode generation (if keeping custom)

### Phase 3: Protocols & Communication
- [ ] NMEA GPS protocol
- [ ] Other communication protocols
- [ ] Network utilities

### Phase 4: Specialized Features
- [ ] Hardware device support (8 devices)
- [ ] Classic cryptography (with security warnings)
- [ ] Apple II retro computing
- [ ] Fencing equipment protocols
- [ ] Windows Forms components
- [ ] UWP components (if applicable)

### Phase 5: Tools & CLI
- [ ] CLI tools migration (4 tools)
- [ ] Developer utilities
- [ ] Documentation and examples

---

## Project Structure (Future)

```
src/
├── Framework/
│   ├── OoBDev.IO.BinaryPrimitives/        # Core binary types
│   ├── OoBDev.IO.Archives/                # Archive formats
│   ├── OoBDev.Expressions/                # Expression calculator
│   └── OoBDev.Drawing/                    # Geometry (if custom)
├── Extensions/
│   ├── OoBDev.Extensions.CodeAnalysis/    # Roslyn extensions
│   ├── OoBDev.Extensions.UI.Collections/  # Windows Forms UI
│   └── OoBDev.Extensions.Barcode/         # Barcode (if custom)
├── ExternalServices/
│   └── Hardware/                          # Device communication
│       ├── OoBDev.Hardware.GPS/           # GPS devices
│       ├── OoBDev.Hardware.Fencing/       # Fencing equipment
│       └── ...
└── Tools/
    └── BinaryData tools/                  # CLI utilities
```

---

## Key Features by Priority

### Priority 1: Foundation (IMMEDIATE)
- Binary primitive types
- Endianness support
- Core utilities

### Priority 2: High-Value (HIGH)
- Archive formats (TAR, CPIO, ZIP, ISO 9660)
- Code analysis (Roslyn)
- Expression calculator
- Drawing/geometry
- Barcode generation

### Priority 3: Protocols (MEDIUM-HIGH)
- NMEA GPS protocol
- Communication protocols
- Network utilities

### Priority 4: Specialized (MEDIUM)
- Hardware devices (8 devices):
  - De5000 Bluetooth scanner
  - Rigol oscilloscope
  - GPS devices
  - Fencing equipment
  - Others
- Classic cryptography (educational)
- Apple II support
- Retro computing

### Priority 5: UI & Tools (LOW-MEDIUM)
- Windows Forms validation controls
- UWP components
- CLI tools (4 tools)

---

## What Gets Migrated

✅ Core features (obviously)
✅ Highly specialized features (ISO 9660, hardware devices)
✅ Niche features (Apple II, retro computing, fencing equipment)
✅ Educational features (classic cryptography with warnings)
✅ Incomplete features (migrate as-is, track TODOs)
✅ UI components (Windows Forms validation controls)

**Only Delete:**
❌ Stub projects with zero implementation (e.g., Rigol)
❌ Silverlight-only or obsolete platform code
❌ Features with no .NET 10.0 equivalent

---

## LOC Summary

- Total: ~50,000 LOC
- Phase 1: ~5,000 LOC
- Phase 2: ~15,000 LOC
- Phase 3: ~10,000 LOC
- Phase 4: ~15,000 LOC
- Phase 5: ~5,000 LOC

---

## Dependencies

**Blocks:**
- Everything - decisions required before ANY migration

**Requires:**
- User decisions on 14+ critical questions
- Architectural decisions on API design
- Use case clarifications

---

## Next Steps

1. **Review critical questions document:**
   - [BinaryDataDecoders Critical Questions](docs/migration/binarydatadecoders-critical-questions.md)

2. **Make decisions** for at least Phase 1 (Immediate Priority)

3. **Start with Phase 1** using recommended defaults if needed:
   - Endianness: Both extension methods AND static methods
   - BinaryPrimitives: Follow .NET naming conventions
   - UI Collections: Yes, create separate project

4. **Defer Phase 2-4 decisions** until Phase 1 complete if needed

---

**Related Documentation:**
- [BinaryDataDecoders Feature Mapping](docs/migration/binarydatadecoders-feature-mapping.md)
- [BinaryDataDecoders Migration Plan](docs/migration/binarydatadecoders-migration-plan.md)
- [BinaryDataDecoders Critical Questions](docs/migration/binarydatadecoders-critical-questions.md)

---

**Effort:** 8-12 weeks (after decisions, phased approach)
**Risk:** HIGH - Massive codebase, architectural decisions required
