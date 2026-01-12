# BinaryDataDecoders to OoBDev Feature Mapping

**Version:** 1.0
**Last Updated:** 2026-01-12
**Source:** BinaryDataDecoders (Incomming/BinaryDecoders)
**Target:** OoBDev (dotex) Framework

---

## Overview

This document provides a comprehensive feature-by-feature mapping between the BinaryDataDecoders codebase and the OoBDev framework. Each feature is classified as:

- **EXISTS** - Feature exists in OoBDev with similar functionality
- **NEW** - Feature does not exist in OoBDev, requires migration
- **UPDATE** - Feature exists but BinaryDataDecoders has improvements/bug fixes
- **DELETE** - Feature is obsolete or inferior to OoBDev implementation

---

## Executive Summary

**BinaryDataDecoders Statistics:**
- Total Projects: 41
- Total C# Files: ~800+
- Core Projects: 26
- Test Projects: 15 (37% test coverage ratio)
- Target Frameworks: net8.0, net9.0

**Migration Status Overview:**
- **NEW Features**: 18 major feature areas
- **UPDATE Required**: 7 areas with critical bugs/improvements
- **EXISTS (Keep OoBDev)**: 5 areas
- **DELETE (No Migration)**: 11 specialized/obsolete areas

---

## Part 1: Foundation & Core Utilities

### 1.1 ToolKit Foundation (123+ Utilities)

**Status:** NEW (with some EXISTS overlap)
**Source:** `BinaryDataDecoders.ToolKit` + `.Abstractions`
**Target:** `OoBDev.System` (merge/enhance)
**Priority:** CRITICAL

#### Feature Breakdown

| Feature Category | Sub-Features | OoBDev Status | Migration Action |
|-----------------|--------------|---------------|------------------|
| **Binary/Numeric** | BCD conversions, Endianness, FormattableNumber | EXISTS (partial) | UPDATE |
| **Collections** | DoubleLinkedList, ObservableDictionary, ReversibleEnumerator | NEW | MIGRATE |
| **I/O** | PathEx with wildcards, PathNavigator, TempFileHandle | UPDATE | MIGRATE + FIX BUGS |
| **MVVM** | ViewModelBase, CommandBase, DelegateCommand | DELETE | N/A (OoBDev is not UI framework) |
| **XML/XPath** | INode tree abstractions, XPath navigators | EXISTS | KEEP OoBDev |
| **Threading** | Task extensions, ParallelQuery helpers | NEW | MIGRATE |
| **Validation** | EnumerableValidator, ValidationHelper | NEW | MIGRATE |

#### Critical Bugs to Fix in OoBDev

**BUG-001: PathEx.cs Lambda Bug (HIGH PRIORITY)**
```csharp
// CURRENT (BROKEN):
wildCards.Any(ps.Contains)

// SHOULD BE:
wildCards.Any(c => ps.Contains(c))
```
**Location:** `/current/src/src/Framework/OoBDev.System/IO/PathEx.cs`
**Impact:** Wildcard path matching completely broken
**File Reference:** BinaryDataDecoders.ToolKit/IO/PathEx.cs:line 115

**BUG-002: StreamDevice Nullable Annotations**
```csharp
// CURRENT (MISSING NULLABLE):
public IDeviceAdapter Device => _device;

// SHOULD BE:
public IDeviceAdapter? Device => _device;
```
**Location:** `/current/src/src/Framework/OoBDev.System.IO.Pipelines/StreamDevice.cs:19-21`
**Impact:** Nullable reference warnings, potential null reference exceptions

**BUG-003: StreamDevice Event Typo**
```csharp
// CURRENT (TYPO):
public event EventHandler<ErrorEventArgs>? MessageTrasmitterError;

// SHOULD BE:
public event EventHandler<ErrorEventArgs>? MessageTransmitterError;
```
**Location:** `/current/src/src/Framework/OoBDev.System.IO.Pipelines/StreamDevice.cs`
**Impact:** API naming consistency

#### Detailed Feature Mapping

**Binary/Numeric Utilities:**
- `BcdEx` - BCD encode/decode → **NEW** (not in OoBDev)
- `BigEndianUShort` → **EXISTS** (OoBDev.System/BigEndianUShort.cs)
- `BigEndianInt32`, `BigEndianUInt32`, etc. → **NEW** (only ushort exists in OoBDev)
- `FormattableNumber<T>` (engineering/scientific notation) → **NEW**
- `ByteEx.Compare` → **NEW** (memory comparison utilities)
- `NumberEx` extensions → **NEW** (IsEven, IsOdd, IsPowerOfTwo, etc.)

**Collections:**
- `DoubleLinkedList<T>` → **NEW** (bidirectional navigation)
- `ObservableDictionary<K,V>` → **NEW** (MVVM-ready dictionary)
- `ReversibleEnumerator<T>` → **NEW** (forward/backward enumeration)
- `EnumerableEx` → **EXISTS** (OoBDev has similar LINQ extensions)

**I/O Utilities:**
- `PathEx.CreateParentIfNotExists` → **UPDATE** (has bug in BDD too)
- `PathEx.Wildcards` → **UPDATE** (fix lambda bug in OoBDev)
- `PathNavigator` (XPath for file system) → **NEW**
- `TempFileHandle` (disposable temp files) → **EXISTS** (OoBDev.System has ITempFile)
- `StreamEx` → **EXISTS** (OoBDev has stream extensions)

**Threading:**
- `TaskEx.WhenAllOrException` → **NEW**
- `ParallelQueryEx` → **NEW**
- Thread-safe collection helpers → **NEW**

**Validation:**
- `EnumerableValidator` → **NEW**
- `ValidationHelper` → **NEW**
- Validation attributes → **NEW**

**Recommendation:**
- **Phase 1:** Fix critical bugs in OoBDev (BUG-001, BUG-002, BUG-003)
- **Phase 2:** Migrate missing endianness types (BigEndianInt32, etc.)
- **Phase 3:** Migrate useful collections (DoubleLinkedList, ObservableDictionary)
- **Phase 4:** Migrate remaining utilities including MVVM components (useful for WPF, Windows Forms, Blazor UI)

---

### 1.2 TestUtilities

**Status:** EXISTS (OoBDev has equivalent)
**Source:** `BinaryDataDecoders.TestUtilities`
**Target:** `OoBDev.TestUtilities`
**Priority:** LOW

#### Feature Comparison

| Feature | BinaryDataDecoders | OoBDev | Action |
|---------|-------------------|---------|--------|
| TestContext extensions | Yes | Yes | KEEP OoBDev |
| Test logger | Yes | Yes | KEEP OoBDev |
| Result capture | Yes | Yes | KEEP OoBDev |
| MSTest framework | Yes | Yes | KEEP OoBDev |

**Recommendation:** KEEP OoBDev implementation. Both are similar and adequate.

---

## Part 2: Code Analysis & Expression Parsing

### 2.1 CodeAnalysis (Roslyn XPath Navigation)

**Status:** NEW
**Source:** `BinaryDataDecoders.CodeAnalysis` + `.StructuredLog`
**Target:** `OoBDev.CodeAnalysis` (new)
**Priority:** HIGH

#### Feature Details

**Capabilities:**
- C# syntax tree navigation via XPath
- Visual Basic syntax tree navigation via XPath
- Semantic model analysis
- MSBuild structured log parsing
- Symbol lookup and navigation

**Key Files:**
- `CSharpNavigator.cs` - C# to XPathNavigable
- `CSharpSemanticNavigator.cs` - Semantic model navigation
- `VisualBasicNavigator.cs` - VB to XPathNavigable
- `StructuredLogNavigator.cs` - MSBuild log queries

**Example Usage:**
```csharp
// Query C# code structure
var navigator = new CSharpNavigator(syntaxTree);
var publicClasses = navigator.Select("//class[@accessibility='public']");
```

**Dependencies:**
- Microsoft.CodeAnalysis.CSharp 4.12.0
- Microsoft.CodeAnalysis.VisualBasic 4.12.0

**OoBDev Status:** Does not exist

**Migration Complexity:** Medium
- Well-architected with clear abstractions
- Follows XPathNavigable pattern (consistent with OoBDev.System)
- Has test coverage
- No conflicts with existing OoBDev code

**Recommendation:**
- **MIGRATE** as new `OoBDev.CodeAnalysis` project
- Follow OoBDev provider/factory pattern
- Place in Framework layer
- Create abstractions project: `OoBDev.CodeAnalysis.Abstractions`
- Implementation project: `OoBDev.CodeAnalysis`
- External service projects:
  - `OoBDev.Microsoft.CodeAnalysis.CSharp`
  - `OoBDev.Microsoft.CodeAnalysis.VisualBasic`
  - `OoBDev.Microsoft.Build.StructuredLog`

---

### 2.2 ExpressionCalculator (ANTLR Parser)

**Status:** UPDATE (CRITICAL - OoBDev has broken stub)
**Source:** `BinaryDataDecoders.ExpressionCalculator`
**Target:** `OoBDev.ExpressionCalculator` (exists but incomplete)
**Priority:** CRITICAL

#### Critical Issue Found

**CRITICAL BUG: ShiftCommutativeVariablesRight is Non-Functional Stub**

**OoBDev Implementation (BROKEN):**
```csharp
// File: /current/src/src/Framework/OoBDev.ExpressionCalculator/Optimizers/ShiftCommutativeVariablesRight.cs
public class ShiftCommutativeVariablesRight<T> : IExpressionOptimizer<T>
    where T : struct, INumber<T>
{
    public IExpression<T> Optimize(IExpression<T> expression)
    {
        return expression; // DOES NOTHING!!!
    }
}
```

**BinaryDataDecoders Implementation (WORKING):**
Has full recursive expression tree traversal with proper variable shifting logic.

**Impact:**
- Expression normalization broken
- Expressions like `a + 2` and `2 + a` not recognized as equivalent
- Expression comparison/optimization fails
- Critical for expression simplification

**Migration Required:**
- Replace OoBDev stub with BinaryDataDecoders working implementation
- Verify all optimizers are functional (check for other stubs)
- Add comprehensive tests

#### Feature Comparison

| Feature | BinaryDataDecoders | OoBDev | Action |
|---------|-------------------|---------|--------|
| ANTLR4 grammar | Yes (ExpressionTree.g4) | Yes | VERIFY MATCH |
| Parser | Yes | Yes | VERIFY |
| Expression tree | Yes | Yes | UPDATE |
| 14 numeric evaluators | Yes | Yes | VERIFY ALL |
| Optimizers | 5 working | 1 BROKEN, 4 unknown | **UPDATE** |
| Variable substitution | Yes | Yes | VERIFY |
| Tests | Comprehensive | Unknown | **ADD TESTS** |

**Optimizers to Verify/Migrate:**

1. **InnerExpressionReducer** - Remove unnecessary parentheses: `((a))→a`
   - **Action:** VERIFY in OoBDev

2. **UnaryNumericExpressionReducer** - Simplify negations: `--a→a`
   - **Action:** VERIFY in OoBDev

3. **IdentityExpressionOptimizer** - Identity operations: `a*1→a`, `a+0→a`
   - **Action:** VERIFY in OoBDev

4. **DeterminedExpressionReducer** - Determined results: `B/B→1`, `B^0→1`
   - **Action:** VERIFY in OoBDev

5. **ShiftCommutativeVariablesRight** - Normalize: `a+2→2+a`
   - **Action:** **REPLACE** (broken in OoBDev)

**Recommendation:**
- **IMMEDIATE:** Replace ShiftCommutativeVariablesRight with working implementation
- Verify all other optimizers are functional
- Add comprehensive test suite from BinaryDataDecoders
- Document expression optimization pipeline

---

## Part 3: File Formats & Archives

### 3.1 Archives (TAR/ZIP)

**Status:** UPDATE
**Source:** `BinaryDataDecoders.Archives`
**Target:** `OoBDev.Archives` (new, partial exists in OoBDev.System)
**Priority:** MEDIUM

#### Feature Comparison

| Format | BinaryDataDecoders | OoBDev.System | Action |
|--------|-------------------|---------------|--------|
| TAR | Full read/write | Header structures only | **UPDATE** |
| ZIP | LocalFileHeader read | LocalFileHeader read | **MERGE** |
| 7z | No | No | N/A |
| RAR | No | No | N/A |

**OoBDev Current State:**
- `/current/src/src/Framework/OoBDev.System/Archives/Zip/ZipFile.cs` - Basic ZIP reading
- `/current/src/src/Framework/OoBDev.System/Archives/Zip/LocalFileHeader.cs` - ZIP header
- `/current/src/src/Framework/OoBDev.System/Archives/Tar/TarHeader.cs` - TAR header only

**BinaryDataDecoders Additions:**
- Complete TAR archive creation and extraction
- Enhanced ZIP support
- Archive entry enumeration
- Stream-based access

**Migration Complexity:** Low
- Well-defined functionality
- No conflicts with existing code
- Can coexist with System.IO.Compression

**Recommendation:**
- **MIGRATE** TAR implementation
- **MERGE** ZIP improvements
- Create `OoBDev.Archives` Framework project
- Maintain compatibility with existing OoBDev.System structures
- Consider using System.IO.Compression as foundation, extend with BDD features

---

### 3.2 FileSystems (ISO 9660)

**Status:** NEW
**Source:** `BinaryDataDecoders.FileSystems`
**Target:** `OoBDev.FileSystems` (new)
**Priority:** LOW

#### Feature Details

**Capabilities:**
- ISO 9660 CD/DVD filesystem reading
- Directory traversal
- File extraction
- Joliet extension support (Unicode)

**Use Cases:**
- Reading ISO images
- Virtual filesystem mounting
- Legacy CD-ROM data access

**OoBDev Status:** Does not exist

**Migration Complexity:** Low
- Self-contained functionality
- No dependencies on OoBDev core
- Clear domain boundaries

**Recommendation:**
- **MIGRATE** if ISO 9660 support is needed
- Low priority - niche use case
- Consider as Extension layer: `OoBDev.Extensions.FileSystems.ISO9660`
- Document as specialized feature

---

## Part 4: Graphics & Imaging

### 4.1 Drawing (Barcodes, Images)

**Status:** NEW
**Source:** `BinaryDataDecoders.Drawing`
**Target:** `OoBDev.Drawing` (new)
**Priority:** MEDIUM

#### Feature Details

**Capabilities:**
1. **Code39 Barcode Generation**
   - Standard Code39
   - Full ASCII Code39
   - Configurable dimensions
   - System.Drawing-based rendering

2. **JPEG Manipulation**
   - JPEG segment reading
   - JPEG mending (repair corrupted files)
   - Metadata extraction

3. **Multi-Scale Images (DeepZoom)**
   - Image tiling for zoom interfaces
   - Z-order curve generation
   - Configurable tile size
   - Multi-resolution pyramid

4. **PNG Utilities**
   - PNG packing
   - Metadata handling

5. **Color Utilities**
   - RGB/HSL conversion
   - Luminosity calculations
   - Contrast measurement

**Dependencies:**
- System.Drawing.Common

**OoBDev Status:** Does not exist

**Migration Complexity:** Medium
- Depends on System.Drawing (cross-platform considerations)
- May need SkiaSharp or ImageSharp alternative for .NET 9.0
- Well-architected code, clean abstractions

**Recommendation:**
- **EVALUATE** based on use cases:
  - If barcode generation needed → MIGRATE with modern imaging library
  - If DeepZoom needed → MIGRATE
  - If JPEG mending needed → MIGRATE
- **MODERNIZE:** Replace System.Drawing with SkiaSharp or SixLabors.ImageSharp
- Create as Extension: `OoBDev.Extensions.Drawing`
- Separate projects:
  - `OoBDev.Extensions.Drawing.Barcodes` - Code39, QR codes (future)
  - `OoBDev.Extensions.Drawing.Images` - JPEG, PNG, DeepZoom

---

## Part 5: Network & Protocols

### 5.1 Network Utilities

**Status:** EXISTS (partial overlap)
**Source:** `BinaryDataDecoders.Net`
**Target:** `OoBDev.System.Net` + `OoBDev.Communications`
**Priority:** LOW

#### Feature Comparison

| Feature | BinaryDataDecoders | OoBDev | Action |
|---------|-------------------|---------|--------|
| Wake-on-LAN | Yes | **Yes** | KEEP OoBDev |
| LDAP filter builder | Yes | **Yes** | KEEP OoBDev |
| Echo server (RFC 862) | Yes | **Yes** | KEEP OoBDev |
| Time server (RFC 868) | Yes | **Yes** | KEEP OoBDev |
| Chargen (RFC 864) | Yes | **Yes** | KEEP OoBDev |
| Daytime (RFC 867) | Yes | **Yes** | KEEP OoBDev |
| Discard (RFC 863) | Yes | **Yes** | KEEP OoBDev |
| MAC address parsing | Yes | Partial | MERGE |

**Recommendation:**
- **KEEP OoBDev** implementations (already migrated)
- **MERGE** any enhanced MAC address utilities if superior

---

### 5.2 NMEA GPS Protocol

**Status:** NEW
**Source:** `BinaryDataDecoders.Nmea`
**Target:** `OoBDev.Protocols.Nmea` (new)
**Priority:** MEDIUM

#### Feature Details

**Capabilities:**
- NMEA 0183 protocol decoder
- Checksum validation
- GGA sentence - Global Positioning Fix Data
- GSA sentence - GPS DOP and Active Satellites
- Field extraction and parsing
- Extensible sentence types

**Example:**
```csharp
$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47
```

**Use Cases:**
- GPS device integration
- Marine navigation
- Vehicle tracking
- Location services

**OoBDev Status:** Does not exist

**Migration Complexity:** Low
- Self-contained protocol decoder
- Clear abstractions
- Good test coverage
- No conflicts

**Recommendation:**
- **MIGRATE** as `OoBDev.Protocols.Nmea`
- Place in Framework layer
- Follow OoBDev patterns:
  - `OoBDev.Protocols.Nmea.Abstractions` - Interfaces
  - `OoBDev.Protocols.Nmea` - Implementation
- Integrate with OoBDev.System.IO (serial port, stream device)
- Document as GPS/Navigation protocol

---

## Part 6: Security & Cryptography

### 6.1 Classic Cryptography

**Status:** NEW
**Source:** `BinaryDataDecoders.Cryptography`
**Target:** `OoBDev.Security.Cryptography` (new)
**Priority:** LOW

#### Feature Details

**Capabilities:**
1. **Caesar Cipher** - Shift-based encryption
2. **Vigenère Cipher** - Polyalphabetic substitution
3. **PlayFair Cipher** - Digraph substitution
4. **Enigma Machine** - Complete WWII cipher simulation
   - 3-5 rotors
   - Reflector
   - Plugboard
   - Historically accurate
5. **Lorenz Cipher** - Tunny cipher machine

**Use Cases:**
- Educational / cryptography teaching
- Historical cipher simulation
- CTF challenges
- NOT for production security (these are broken ciphers)

**OoBDev Status:** Does not exist

**Migration Complexity:** Low
- Self-contained implementations
- Clear educational value
- No production security risk (clearly historic)
- Good test coverage

**Migration Plan:**
- **MIGRATE** for educational and CTF use cases (Phase 4)
- Create `OoBDev.Security.Cryptography.Classic`
- Mark ALL classes as [Obsolete("For educational use only. NOT SECURE.")]
- Document as educational/historical only
- Add comprehensive XML doc warnings about security
- Package separately from core framework
- Include tests that demonstrate both operation AND breaking

---

## Part 7: Retro Computing & Specialized

### 7.1 Apple II Support

**Status:** NEW
**Source:** `BinaryDataDecoders.Apple2`
**Target:** `OoBDev.Retro.Apple2` (new)
**Priority:** VERY LOW

#### Feature Details

**Capabilities:**
- AppleSoft BASIC detokenizer
- DOS 3.3 disk image reading
- Apple II text encoding
- Legacy file format support

**Use Cases:**
- Retro computing preservation
- Legacy data recovery
- Educational/historical

**OoBDev Status:** Does not exist

**Migration Complexity:** Low
- Self-contained
- No dependencies on OoBDev core

**Migration Plan:**
- **MIGRATE** for retro computing and digital preservation (Phase 4)
- Create `OoBDev.Retro.Apple2` in Extensions layer
- Maintain full DOS 3.3 and AppleSoft BASIC support
- Document use cases (preservation, education, data recovery)
- Package separately for specialized audience
- Include comprehensive tests with real disk images

---

### 7.2 Hardware Device Support (9 Projects)

**Status:** NEW (specialized)
**Priority:** CASE-BY-CASE

#### Projects

1. **BinaryDataDecoders.Kuando.Busylight** - USB RGB presence indicator
2. **BinaryDataDecoders.Quarta.RadexOne** - Radiation detector
3. **BinaryDataDecoders.Velleman.K8055** - Experiment board I/O
4. **BinaryDataDecoders.Zoom.H4n** - Audio recorder control
5. **BinaryDataDecoders.ElectronicScoringMachines.Fencing** - Fencing equipment
6. **BinaryDataDecoders.LanC** - Sony LANC camera control
7. **BinaryDataDecoders.Rigol** - Oscilloscope (stub)
8. **BinaryDataDecoders.EByteElectronicTechnology** - LoRa/RS485 modules
9. **BinaryDataDecoders.Net.ZWave** - Z-Wave home automation (if exists)

**Migration Status:**

| Device | Priority | Action | Phase | Notes |
|--------|----------|--------|-------|-------|
| Kuando Busylight | LOW | MIGRATE | Phase 4 | Presence indicators for teams |
| RadexOne | VERY LOW | MIGRATE | Phase 4 | Radiation detection (safety/education) |
| Velleman K8055 | LOW | MIGRATE | Phase 4 | Hobbyist experiment board |
| Zoom H4n | VERY LOW | MIGRATE | Phase 4 | Legacy audio equipment |
| Fencing equipment | VERY LOW | MIGRATE | Phase 4 | Sport equipment automation |
| LANC | VERY LOW | MIGRATE | Phase 4 | Camera protocol (video production) |
| Rigol | N/A | DELETE | - | Stub project only, no implementation |
| EByte modules | LOW | MIGRATE | Phase 4 | IoT/LoRa communication |

**Migration Approach:**
- ALL specialized hardware will be migrated to maintain full functionality
- Projects will be packaged separately for specialized domains
- Follow provider/factory pattern for device abstractions
- Maintain in Extensions layer: `OoBDev.Extensions.Hardware.*`
- Document specific use cases for each device type

---

## Part 8: CLI Tools

### 8.1 Command-Line Tools (4 Projects)

**Status:** EVALUATE
**Priority:** LOW

#### Tools

1. **BinaryDataDecoders.IO.Controller.Cli** - Device controller
2. **BinaryDataDecoders.Net.ServiceHost.Cli** - Network service host
3. **BinaryDataDecoders.PackMan.Cli** - Package manager
4. **BinaryDataDecoders.Xslt.Cli** - XSLT transformation

**OoBDev Equivalent:**
- OoBDev has its own CLI tools:
  - OoBDev.DacPacCompiler.Cli
  - OoBDev.DocumentConverter.Cli
  - OoBDev.FileRagEngine.Cli
  - OoBDev.TemplateEngine.Cli

**Migration Status:**

| Tool | Action | Phase | Notes |
|------|--------|-------|-------|
| IO.Controller.Cli | MIGRATE | Phase 4 | Device control utility |
| ServiceHost.Cli | MIGRATE | Phase 4 | Network service hosting |
| PackMan.Cli | MIGRATE | Phase 4/5 | Package management utility |
| Xslt.Cli | **MERGE** | Integrate into OoBDev.TemplateEngine.Cli |

---

## Part 9: UI Components

### 9.1 Windows Forms

**Status:** NEW
**Source:** `BinaryDataDecoders.Windows.Forms`
**Target:** `OoBDev.Extensions.Windows.Forms`
**Priority:** LOW

**Migration Plan:**
- **MIGRATE** Windows Forms components (Phase 4/5)
- Windows Forms is actively supported in .NET 9.0
- Create `OoBDev.Extensions.Windows.Forms` for validation controls
- Useful for desktop applications built on OoBDev
- Package separately for desktop/UI scenarios
- OoBDev supports both desktop and server scenarios - Windows Forms extends the framework for desktop UI applications
- Maintain full compatibility with .NET 9.0 Windows Forms

---

## Part 10: Critical Bug Fixes Required

### Summary of Bugs

#### OoBDev Bugs (MUST FIX BEFORE MIGRATION)

**BUG-001: PathEx Lambda Bug**
- **File:** `/current/src/src/Framework/OoBDev.System/IO/PathEx.cs`
- **Line:** ~115
- **Impact:** HIGH - Wildcard path matching broken
- **Fix:** `wildCards.Any(ps.Contains)` → `wildCards.Any(c => ps.Contains(c))`

**BUG-002: StreamDevice Nullable Annotations**
- **File:** `/current/src/src/Framework/OoBDev.System.IO.Pipelines/StreamDevice.cs`
- **Lines:** 19-21
- **Impact:** MEDIUM - Nullable reference warnings
- **Fix:** Add `?` to nullable properties

**BUG-003: StreamDevice Event Typo**
- **File:** `/current/src/src/Framework/OoBDev.System.IO.Pipelines/StreamDevice.cs`
- **Impact:** MEDIUM - API naming consistency
- **Fix:** `MessageTrasmitterError` → `MessageTransmitterError`

**BUG-004: SerialPortFactory Verbose Ternary**
- **File:** `/current/src/src/Framework/OoBDev.System.IO.Ports/SerialPortFactory.cs`
- **Impact:** LOW - Code style
- **Fix:** Simplify ternary expression

**BUG-005: ShiftCommutativeVariablesRight Stub**
- **File:** `/current/src/src/Framework/OoBDev.ExpressionCalculator/Optimizers/ShiftCommutativeVariablesRight.cs`
- **Impact:** CRITICAL - Expression normalization broken
- **Fix:** Replace with working implementation from BinaryDataDecoders

#### BinaryDataDecoders Bugs (if maintaining)

**BUG-006: PathEx Missing Null Check**
- **File:** `BinaryDataDecoders.ToolKit/IO/PathEx.cs`
- **Impact:** LOW - Potential null reference
- **Fix:** Add null check in CreateParentIfNotExists

**BUG-007: YamlNavigator Missing Null-Safe Operator**
- **File:** `OoBDev.System.Text.Yaml/YamlNavigator.cs`
- **Line:** 36
- **Impact:** LOW - Null-safe operator missing
- **Fix:** Add null-safe operator

---

## Migration Strategy Matrix

| Category | Projects | Status | Priority | Target Layer | Effort |
|----------|----------|--------|----------|--------------|--------|
| **ToolKit Foundation** | 2 | UPDATE | CRITICAL | Framework | MEDIUM |
| **CodeAnalysis** | 2 | NEW | HIGH | Framework | MEDIUM |
| **ExpressionCalculator** | 1 | UPDATE | CRITICAL | Framework | LOW |
| **Archives** | 1 | UPDATE | MEDIUM | Framework | LOW |
| **Drawing** | 1 | NEW | MEDIUM | Extensions | MEDIUM |
| **NMEA Protocol** | 1 | NEW | MEDIUM | Framework | LOW |
| **FileSystems** | 1 | NEW | LOW | Extensions | LOW |
| **Cryptography** | 1 | NEW | LOW | Framework | LOW |
| **Network Utils** | 1 | EXISTS | LOW | - | Phase 5 |
| **TestUtilities** | 1 | EXISTS | LOW | - | Phase 5 |
| **Apple II** | 1 | NEW | VERY LOW | Extensions | Phase 4 |
| **Hardware (9 projects)** | 9 | NEW | VERY LOW | Extensions | Phase 4 |
| **CLI Tools** | 4 | MIXED | LOW | Tools | Phase 4/5 |
| **Windows Forms** | 1 | NEW | LOW | Extensions | Phase 4/5 |

---

## Architectural Compliance Plan

Following `/current/src/docs/architecture` guidelines:

### Layer Placement

**Framework Layer:**
- OoBDev.CodeAnalysis.Abstractions
- OoBDev.CodeAnalysis
- OoBDev.ExpressionCalculator (enhance existing)
- OoBDev.Archives
- OoBDev.Protocols.Nmea.Abstractions
- OoBDev.Protocols.Nmea
- OoBDev.Security.Cryptography.Classic

**Extensions Layer:**
- OoBDev.Extensions.Drawing.Barcodes
- OoBDev.Extensions.Drawing.Images
- OoBDev.Extensions.FileSystems.ISO9660

**ExternalServices Layer:**
- OoBDev.Microsoft.CodeAnalysis.CSharp
- OoBDev.Microsoft.CodeAnalysis.VisualBasic
- OoBDev.Microsoft.Build.StructuredLog

### Provider/Factory Pattern Application

**CodeAnalysis:**
```
ICodeNavigator → ICodeNavigatorProvider → ICodeNavigatorProviderFactory
```

**Drawing:**
```
IBarcodeGenerator → IBarcodeGeneratorProvider → IBarcodeGeneratorProviderFactory
```

**Archives:**
```
IArchiveReader<T> → IArchiveReaderProvider → IArchiveReaderProviderFactory
```

### Dependency Injection Pattern

All new components MUST follow:
- TryAdd* extension methods
- Builder pattern for configuration
- IOptions<T> for settings
- Keyed services for multiple implementations

### Testing Standards

All migrated code MUST meet:
- Minimum 80% coverage for Framework projects
- MSTest framework
- Coverlet code coverage
- Test categories: [TestCategory("Unit")], [TestCategory("Simulate")]

### Documentation Standards

Each migrated project MUST have:
- README.md (enforced by build)
- XML documentation on public APIs
- Usage examples
- Configuration documentation
- PlantUML diagrams where appropriate

---

## Migration Phases (No Timeline)

### Phase 0: Critical Bug Fixes

**IMMEDIATE - Before any migration:**
1. Fix BUG-001 (PathEx lambda)
2. Fix BUG-002 (StreamDevice nullable)
3. Fix BUG-003 (StreamDevice typo)
4. Fix BUG-004 (SerialPortFactory style)
5. Fix BUG-005 (ShiftCommutativeVariablesRight)

**Validation:**
- All OoBDev tests pass
- No new warnings
- Expression calculator tests pass
- Path wildcard tests added and pass

### Phase 1: Foundation Enhancement

**Projects:**
- BinaryDataDecoders.ToolKit (selective merge)
  - Endianness types (BigEndianInt32, etc.)
  - FormattableNumber<T>
  - Collections (DoubleLinkedList, ObservableDictionary)
  - Threading utilities

**Actions:**
1. Create feature-by-feature migration plan
2. Merge into OoBDev.System
3. Maintain architectural patterns
4. Add comprehensive tests
5. Update documentation

**Validation:**
- All tests pass
- No breaking changes to existing OoBDev.System APIs
- Documentation updated
- Examples added

### Phase 2: High-Value Features

**Projects:**
- BinaryDataDecoders.CodeAnalysis
- BinaryDataDecoders.ExpressionCalculator (complete)
- BinaryDataDecoders.Archives

**Actions:**
1. Create new Framework projects following OoBDev patterns
2. Implement provider/factory abstractions
3. Create ExternalServices wrappers for Roslyn
4. Add comprehensive tests
5. Create documentation and examples

**Validation:**
- Follows architectural guidelines
- Provider/factory pattern implemented
- DI registration via TryAdd* methods
- 80%+ test coverage
- README and XML docs complete

### Phase 3: Protocols & Extensions

**Projects:**
- BinaryDataDecoders.Nmea
- BinaryDataDecoders.Drawing (selective)

**Actions:**
1. Create protocol abstractions
2. Implement drawing with modern libraries (SkiaSharp/ImageSharp)
3. Follow extension layer patterns
4. Add tests and documentation

**Validation:**
- Modern dependencies (no System.Drawing)
- Extension layer placement correct
- Tests comprehensive
- Documentation complete

### Phase 4: Specialized Features (Selective)

**Projects (evaluate individually):**
- BinaryDataDecoders.FileSystems
- BinaryDataDecoders.Cryptography
- BinaryDataDecoders.Apple2

**Actions:**
- Only migrate if specific business need
- Follow architectural patterns
- Mark as specialized/educational where appropriate
- Document limitations and use cases

### Phase 5: Cleanup & Documentation

**Actions:**
1. Update all cross-references
2. Create migration guide
3. Archive BinaryDataDecoders (mark as legacy)
4. Update CHANGELOG
5. Tag release

---

## Namespace Migration Map

| BinaryDataDecoders | OoBDev Target |
|-------------------|---------------|
| BinaryDataDecoders.ToolKit | OoBDev.System (merge) |
| BinaryDataDecoders.ToolKit.Abstractions | OoBDev.System.Abstractions (merge) |
| BinaryDataDecoders.CodeAnalysis | OoBDev.CodeAnalysis |
| BinaryDataDecoders.CodeAnalysis.Abstractions | OoBDev.CodeAnalysis.Abstractions |
| BinaryDataDecoders.ExpressionCalculator | OoBDev.ExpressionCalculator (enhance) |
| BinaryDataDecoders.Archives | OoBDev.Archives |
| BinaryDataDecoders.Drawing | OoBDev.Extensions.Drawing |
| BinaryDataDecoders.FileSystems | OoBDev.Extensions.FileSystems |
| BinaryDataDecoders.Nmea | OoBDev.Protocols.Nmea |
| BinaryDataDecoders.Cryptography | OoBDev.Security.Cryptography.Classic |
| BinaryDataDecoders.Net | (merge into OoBDev.Communications) |

---

## Final Recommendations

### Must Migrate (CRITICAL)
1. Bug fixes (Phase 0)
2. ExpressionCalculator working implementation
3. Endianness types
4. CodeAnalysis (Roslyn XPath)

### Should Migrate (HIGH VALUE)
1. Archives (TAR/ZIP)
2. NMEA protocol decoder
3. Selected ToolKit utilities (collections, threading)

### Consider Migrating (MEDIUM VALUE)
1. Drawing/Barcodes (if use case exists)
2. FileSystems (if ISO support needed)

### Skip (LOW VALUE / OUT OF SCOPE)
1. All specialized hardware projects
2. Apple II retro computing
3. Windows Forms UI
4. Most CLI tools
5. Classic cryptography (unless educational need)

### Delete (OBSOLETE / INFERIOR)
1. Windows Forms components
2. CLI tools duplicating OoBDev functionality
3. Rigol stub project
4. Any features inferior to OoBDev equivalents

---

## Success Criteria

Migration is complete and successful when:

1. **All critical bugs fixed** in OoBDev
2. **All HIGH priority features** migrated and tested
3. **Architectural compliance** verified
4. **80%+ test coverage** maintained
5. **Documentation complete** for all migrated features
6. **No breaking changes** to existing OoBDev APIs
7. **Build succeeds** without warnings
8. **All tests pass** (Unit + Simulate)
9. **NuGet packages** generated successfully
10. **Migration guide** published

---

## Related Documentation

- [architectural-guidelines.md](../architecture/architectural-guidelines.md)
- [architectural-standards.md](../architecture/architectural-standards.md)
- [architectural-patterns.md](../architecture/architectural-patterns.md)
- [layering-architecture.md](../architecture/layering-architecture.md)

---

## Change Log

- 2026-01-12 v1.0: Initial BinaryDataDecoders feature mapping created
