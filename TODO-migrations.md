# TODO - Migrations Epic

**Last Updated:** 2026-01-20

This document tracks all migration work for Incomming projects and BinaryDataDecoders.

> **Parent Document:** [TODO.md](./TODO.md)

---

## Individual Migration TODOs

**📋 See:** [TODO-migrations-index.md](./TODO-migrations-index.md) for complete index

Each migration area has a dedicated TODO file:

### SharedFramework - 🎨 REPLACED WITH DESIGN DOCUMENTATION
1. [TODO-migrations-communications.md](./TODO-migrations-communications.md) - 🎨 Design docs (Epic 2)
2. [TODO-migrations-caching.md](./Features/Caching/TODO-migrations-caching.md) ✅ COMPLETE (2026-01-20)
3. **Message Queues** ✅ COMPLETE (2026-01-20) - [Archived](./docs/changes/TODO-migrations-message-queues.md)
4. [TODO-migrations-spatial.md](./TODO-migrations-spatial.md) - 🎨 Design docs (Epic 6)
5. [TODO-migrations-identity.md](./TODO-migrations-identity.md) - 🎨 Design docs (Epic 7)
6. [TODO-migrations-documents.md](./TODO-migrations-documents.md) - 🎨 Design docs (Epic 6)
7. [TODO-migrations-text-templating.md](./TODO-migrations-text-templating.md) - 🎨 Design docs (Epic 10)
8. [TODO-migrations-data-loader.md](./TODO-migrations-data-loader.md) - 🎨 Design docs (Epic 5)
9. [TODO-migrations-complex-events.md](./TODO-migrations-complex-events.md) - 🎨 Design docs (Epic 6)
10. [TODO-migrations-generations.md](./TODO-migrations-generations.md) - 🎨 Design docs (Epic 5)

### Other Incoming
11. 🗑️ **Framework** - CANCELLED: Directory removed from Incoming/
12. [TODO-migrations-binarydatadecoders.md](./TODO-migrations-binarydatadecoders.md) - Blocked by decisions

---

## Overview

Migration work includes:
- 🗑️ **Incomming/Framework** - CANCELLED: Directory removed from Incoming/ (Vector library already integrated)
- 🎨 **Incomming/SharedFramework** - REPLACED: Code migration replaced with design-first documentation (120 docs, 90.9% complete)
- **BinaryDataDecoders** - 5-phase migration of binary processing, protocols, and specialized features

---

## Incomming/Framework Migration Work

🗑️ **CANCELLED** - Directory removed from Incoming/

**What was preserved:**
- Vector library already integrated in main codebase at `src/Framework/OoBDev.System.Abstractions/Math/`
- Namespace updated from `OoBDev.Common.Math` → `OoBDev.System.Math` (5 files)

**Documentation (archived):**
- [Feature Mapping](docs/migration/framework-feature-mapping.md) - Analysis of what was in Framework
- [Vector Comparison](docs/migration/vector-comparison.md) - Detailed vector implementation analysis

**All other Framework migration work cancelled.**

---

## Incomming/SharedFramework Migration Work - 🎨 REPLACED WITH DESIGN DOCUMENTATION

🗑️ **Directory removed from Incoming/** - Code migration replaced with comprehensive design-first documentation

**Strategic Change (2026-01-22):**
Instead of migrating SharedFramework code, we created 120 comprehensive design documents across 8 epics following the Epic 11 pattern. This ensures clean architecture from first principles without inheriting technical debt.

**Status:** 90.9% complete (120 of 132 documents created)

**✅ Completed Migrations (Before Pivot):**
- Caching Framework (4 impl + 3 test projects) - [Details](docs/changes/migration-caching-framework-2026-01-20.md)
- Message Queues (AWS SQS + Azure Service Bus) - [Details](docs/changes/migration-message-queues-2026-01-20.md)

**🎨 Design Documentation Created:**
- **Epic 11:** Data Enhancement (16 docs) ✅ COMPLETE
- **Epic 10:** Text Templating (12 docs) ✅ COMPLETE
- **Epic 12:** Message Composition (4 docs) ✅ COMPLETE
- **Epic 7:** Identity & Session (16 docs) ✅ COMPLETE
- **Epic 6:** Document Services (44 docs) ✅ COMPLETE
- **Epic 4:** Distributed Caching (12 docs) ✅ COMPLETE
- **Epic 2:** Communications (6 docs) 🔄 PARTIAL (6/16)
- **Epic 5:** Master Data (10 docs) 🔄 PARTIAL (10/12)

**Archived Documentation (from before pivot):**
- [Feature Mapping](docs/migration/sharedframework-feature-mapping.md) - 52-project analysis
- [Migration Plan](docs/migration/sharedframework-migration-plan.md) - Original 12-phase plan
- [Coverage Analysis](docs/migration/sharedframework-phase0-coverage-analysis.md) - Overlap analysis

**Next Steps:**
Complete remaining 12 design documents:
- Epic 2: Communications (10 docs remaining)
- Epic 5: Master Data (2 docs remaining)

---

## BinaryDataDecoders Migration Work

**Status:** ⏸️ BLOCKED - Awaiting critical decisions

**See Also:** [TODO-decisions.md](./TODO-decisions.md) - Decision points requiring answers before Phase 1

### Phase 1: Foundation Enhancement

#### 1.1 Endianness Support (UPDATE)
- [ ] Review `Incomming/BinaryDecoders/Utilities/Endian.cs`
- [ ] Enhance `OoBDev.System/BinaryPrimitives/EndianType.cs` with:
  - [ ] Runtime detection methods
  - [ ] Conversion methods for all primitive types
  - [ ] Extension methods for `BinaryReader`/`BinaryWriter`
- [ ] Add comprehensive unit tests
- [ ] Update documentation

**Files to modify:**
- `src/Framework/OoBDev.System/BinaryPrimitives/EndianType.cs`

#### 1.2 Utility Enhancements (UPDATE)
- [ ] Review utilities in `Incomming/BinaryDecoders/Utilities/`
- [ ] Enhance `OoBDev.System/IO/PathEx.cs` with:
  - [ ] Add `GetRelativePath` if missing
  - [ ] Add `NormalizePath` if missing
  - [ ] Add file enumeration improvements
- [ ] Migrate UI/MVVM collections:
  - [ ] ObservableDictionary (INotifyPropertyChanged + INotifyCollectionChanged)
  - [ ] Other observable collections for WPF/Windows Forms/Blazor
  - [ ] Create `OoBDev.Extensions.UI.Collections` if needed
- [ ] Add unit tests for new functionality

**Files to review:**
- `src/Framework/OoBDev.System/IO/PathEx.cs`
- `Incomming/BinaryDecoders/Utilities/PathHelper.cs`
- `Incomming/BinaryDecoders/ToolKit/Collections/`

#### 1.3 BinaryPrimitives Expansion (NEW)
- [ ] Create `src/Framework/OoBDev.System/BinaryPrimitives/BinaryReaderExtensions.cs`
  - [ ] Add `ReadInt16BigEndian()`, `ReadInt16LittleEndian()`
  - [ ] Add `ReadInt32BigEndian()`, `ReadInt32LittleEndian()`
  - [ ] Add `ReadInt64BigEndian()`, `ReadInt64LittleEndian()`
  - [ ] Add UInt variants
  - [ ] Add float/double variants
- [ ] Create `src/Framework/OoBDev.System/BinaryPrimitives/BinaryWriterExtensions.cs`
  - [ ] Add corresponding Write methods
- [ ] Add comprehensive unit tests
- [ ] Add XML documentation

**New files to create:**
- `src/Framework/OoBDev.System/BinaryPrimitives/BinaryReaderExtensions.cs`
- `src/Framework/OoBDev.System/BinaryPrimitives/BinaryWriterExtensions.cs`
- `src/Tests/OoBDev.System.Tests/BinaryPrimitives/BinaryReaderExtensionsTests.cs`
- `src/Tests/OoBDev.System.Tests/BinaryPrimitives/BinaryWriterExtensionsTests.cs`

### Phase 2: High-Value Features

#### 2.1 CodeAnalysis Migration (NEW)
- [ ] Review `Incomming/BinaryDecoders/CodeAnalysis/` structure
- [ ] Design namespace: `OoBDev.CodeAnalysis.*` or `OoBDev.Roslyn.*`
- [ ] Create project structure:
  - [ ] `src/Framework/OoBDev.CodeAnalysis/`
  - [ ] `src/Framework/OoBDev.CodeAnalysis.CSharp/`
  - [ ] `src/Tests/OoBDev.CodeAnalysis.Tests/`

**Key components to migrate:**
- [ ] `Incomming/BinaryDecoders/CodeAnalysis/Extensions/CompilationUnitSyntaxExtensions.cs`
- [ ] `Incomming/BinaryDecoders/CodeAnalysis/Extensions/SyntaxNodeExtensions.cs`
- [ ] `Incomming/BinaryDecoders/CodeAnalysis/Visitors/` → adapter pattern
- [ ] `Incomming/BinaryDecoders/CodeAnalysis/Analyzers/` → Roslyn analyzers

**Sub-tasks:**
- [ ] Add Roslyn package references
- [ ] Implement using OoBDev patterns (Handler, Visitor)
- [ ] Add comprehensive unit tests
- [ ] Create analyzer test harness
- [ ] Document usage patterns

#### 2.2 ExpressionCalculator Migration (NEW)
- [ ] Review `Incomming/BinaryDecoders/ExpressionCalculator/` structure
- [ ] Create `src/Framework/OoBDev.ExpressionCalculator/` (or move from System)
- [ ] Verify current implementation vs incoming:
  - [ ] Compare ANTLR grammar files
  - [ ] Compare optimizer implementations
  - [ ] Compare evaluator implementations

**Key components to migrate/update:**
- [ ] `Expressions/` - Compare implementations
- [ ] `Evaluators/` - Compare implementations
- [ ] `Optimizers/` - Already fixed ShiftCommutativeVariablesRight, check others
- [ ] `Parser/` - Compare ANTLR grammars

**Sub-tasks:**
- [ ] Audit existing `src/Framework/OoBDev.System/ExpressionCalculator/`
- [ ] Identify gaps vs `Incomming/BinaryDecoders/ExpressionCalculator/`
- [ ] Create migration checklist for each sub-component
- [ ] Add missing optimizers
- [ ] Add comprehensive expression tests
- [ ] Performance benchmark comparisons

#### 2.3 Archive Support (NEW)
- [ ] Review `Incomming/BinaryDecoders/Archives/` structure
- [ ] Create `src/Framework/OoBDev.IO.Archives/`
- [ ] Implement formats:
  - [ ] TAR support
  - [ ] CPIO support
  - [ ] ZIP support (if not using System.IO.Compression)

**Architecture:**
- [ ] Design `IArchiveReader` interface
- [ ] Design `IArchiveWriter` interface
- [ ] Implement provider/factory pattern
- [ ] Add streaming support for large archives
- [ ] Add compression support

**Sub-tasks:**
- [ ] Create core abstractions
- [ ] Implement TAR reader/writer
- [ ] Implement CPIO reader/writer
- [ ] Add format detection
- [ ] Add comprehensive tests (including corrupted archives)
- [ ] Add performance tests

#### 2.4 BinaryData Enhancements (UPDATE)
- [ ] Review `Incomming/BinaryDecoders/BinaryData/`
- [ ] Enhance `src/Framework/OoBDev.System/BinaryData/`
- [ ] Add missing features:
  - [ ] Bit-level operations
  - [ ] Checksum/CRC utilities
  - [ ] Binary pattern matching

### Phase 3: Protocol Support

#### 3.1 NMEA Protocol Support (NEW)
- [ ] Review `Incomming/BinaryDecoders/Nmea/` structure
- [ ] Create `src/Framework/OoBDev.Protocols.Nmea/`
- [ ] Implement NMEA 0183 support:
  - [ ] Sentence parser
  - [ ] Sentence formatter
  - [ ] Checksum validation
  - [ ] Common sentence types (GGA, RMC, GSA, etc.)

**Architecture:**
- [ ] Design `INmeaSentence` interface
- [ ] Implement sentence-specific handlers
- [ ] Add validation and error handling
- [ ] Follow existing pipeline patterns

**Sub-tasks:**
- [ ] Create core parser
- [ ] Implement common sentence types
- [ ] Add extensibility for custom sentences
- [ ] Add comprehensive tests with real NMEA data
- [ ] Add documentation and examples

#### 3.2 Drawing/Geometry (NEW)
- [ ] Review `Incomming/BinaryDecoders/Drawing/`
- [ ] Decide: Migrate or use System.Drawing/SkiaSharp?
- [ ] If migrating:
  - [ ] Create `src/Framework/OoBDev.Drawing/`
  - [ ] Implement core primitives (Point, Size, Rectangle, etc.)
  - [ ] Implement geometry operations
- [ ] Add comprehensive tests

### Phase 4: Specialized Domain Features

#### 4.1 FileSystems (ISO 9660)
- [ ] Review `Incomming/BinaryDecoders/FileSystems/`
- [ ] Create `src/Extensions/OoBDev.Extensions.FileSystems.ISO9660/`
- [ ] Migrate ISO 9660 filesystem implementation
- [ ] Add comprehensive tests for ISO image reading
- [ ] Document use cases (CD/DVD access, image mounting)
- [ ] Add examples for common scenarios

**New files to create:**
- `src/Extensions/OoBDev.Extensions.FileSystems.ISO9660/`
- Full ISO 9660 reader implementation
- Tests and documentation

#### 4.2 Classic Cryptography (Educational)
- [ ] Review `Incomming/BinaryDecoders/Cryptography/`
- [ ] Create `src/Extensions/OoBDev.Security.Cryptography.Classic/`
- [ ] Migrate historical cipher implementations:
  - [ ] Enigma machine simulation
  - [ ] Lorenz cipher
  - [ ] Caesar cipher
  - [ ] Vigenère cipher
  - [ ] PlayFair cipher
- [ ] Add `[Obsolete("For educational use only. NOT SECURE.")]` to all classes
- [ ] Add strong security warnings in XML documentation
- [ ] Add comprehensive tests
- [ ] Document educational and CTF use cases

**Important:** Package separately, do NOT include in main distribution

**New files to create:**
- `src/Extensions/OoBDev.Security.Cryptography.Classic/`
- All cipher implementations with security warnings
- Educational documentation

#### 4.3 Retro Computing (Apple II)
- [ ] Review `Incomming/BinaryDecoders/Apple2/`
- [ ] Create `src/Extensions/OoBDev.Retro.Apple2/`
- [ ] Migrate Apple II disk format support
- [ ] Migrate DOS 3.3 filesystem support
- [ ] Add disk image readers/writers
- [ ] Add comprehensive tests
- [ ] Document use cases (legacy data recovery, historical preservation)

**New files to create:**
- `src/Extensions/OoBDev.Retro.Apple2/`
- Disk format implementations
- Tests and documentation

#### 4.4 Hardware Device Support (8 Devices)
- [ ] Review `Incomming/BinaryDecoders/` hardware projects
- [ ] Create `src/Extensions/OoBDev.Extensions.Hardware/` structure
- [ ] Migrate specialized hardware support:
  - [ ] **Kuando Busylight** - Presence indicators
    - Create `OoBDev.Extensions.Hardware.KuandoBusylight`
    - Device factory and provider pattern
    - Tests and documentation
  - [ ] **RadexOne** - Radiation detection
    - Create `OoBDev.Extensions.Hardware.RadexOne`
    - Safety and educational use cases
  - [ ] **Velleman K8055** - Experiment board
    - Create `OoBDev.Extensions.Hardware.VellemanK8055`
    - Hobbyist and educational applications
  - [ ] **Zoom H4n** - Audio equipment
    - Create `OoBDev.Extensions.Hardware.ZoomH4n`
    - Legacy audio device control
  - [ ] **Fencing equipment** - Sport automation
    - Create `OoBDev.Extensions.Hardware.Fencing`
    - Specialized sport equipment control
  - [ ] **LANC** - Camera protocol
    - Create `OoBDev.Extensions.Hardware.LANC`
    - Video production equipment
  - [ ] **EByte modules** - IoT/LoRa
    - Create `OoBDev.Extensions.Hardware.EByte`
    - LoRa and RS485 communication
  - [ ] **ZWave** - Home automation (if exists)
    - Create `OoBDev.Extensions.Hardware.ZWave`
    - Smart home device control
- [ ] Apply provider/factory pattern to all hardware
- [ ] Add comprehensive tests for each device type
- [ ] Package separately for specialized users
- [ ] Document use cases and setup for each device

**Note:** Rigol project is stub only - DELETE

#### 4.5 CLI Tools Migration
- [ ] Review `Incomming/BinaryDecoders/` CLI tools
- [ ] Migrate command-line utilities:
  - [ ] **IO.Controller.Cli** - Device controller
    - Migrate to `src/Tools/OoBDev.IO.Controller.Cli`
    - Device control and automation
  - [ ] **ServiceHost.Cli** - Network service host
    - Migrate to `src/Tools/OoBDev.Net.ServiceHost.Cli`
    - Service hosting utilities
  - [ ] **PackMan.Cli** - Package manager
    - Migrate to `src/Tools/OoBDev.PackMan.Cli`
    - Package management utilities
  - [ ] **Xslt.Cli** - XSLT transformation
    - Merge into existing `OoBDev.TemplateEngine.Cli`
    - XSLT transformation capabilities
- [ ] Add comprehensive help documentation for each tool
- [ ] Add unit and integration tests
- [ ] Package as dotnet tools

#### 4.6 Windows Forms Components
- [ ] Review `Incomming/BinaryDecoders/Windows.Forms/`
- [ ] Create `src/Extensions/OoBDev.Extensions.Windows.Forms/`
- [ ] Migrate Windows Forms validation controls:
  - [ ] Custom validators
  - [ ] Data binding helpers
  - [ ] UI validation components
- [ ] Ensure .NET 10.0 Windows Forms compatibility
- [ ] Add comprehensive tests
- [ ] Document desktop application scenarios
- [ ] Package separately for desktop/UI use cases

**Note:** OoBDev supports both desktop and server scenarios - Windows Forms extends the framework for desktop UI applications

#### 4.7 Platform-Specific Code Review
- [ ] Review UWP-specific code
  - [ ] Determine if any UWP features should be migrated
  - [ ] Update to modern Windows App SDK if needed
  - [ ] Otherwise DELETE
- [ ] Review .NET Framework-specific code
  - [ ] Port to .NET 10.0 if valuable
  - [ ] Otherwise DELETE
- [ ] Review other platform-specific features
  - [ ] Migrate valuable cross-platform features
  - [ ] DELETE platform-locked code without modern equivalent

### Phase 5: Cleanup & Documentation

#### 5.1 Future Development: DeepZoom Viewer Controls (NEW)

**Note:** These are NEW controls, not migrations. To be implemented after migration complete.

##### 5.1.1 WPF DeepZoom Viewer Control
- [ ] Create `src/Extensions/OoBDev.Extensions.WPF.DeepZoom/`
- [ ] Design WPF control architecture:
  - [ ] DeepZoomViewer control (pan, zoom, multi-touch)
  - [ ] Progressive tile loading with caching
  - [ ] Smooth zoom transitions (animation)
  - [ ] Touch/gesture support (pinch-to-zoom)
  - [ ] Mouse wheel and drag support
  - [ ] Keyboard navigation
- [ ] Implement rendering pipeline:
  - [ ] Tile fetching (local and HTTP)
  - [ ] Tile caching strategy
  - [ ] View frustum culling
  - [ ] Level-of-detail (LOD) management
- [ ] Add features:
  - [ ] Min/max zoom constraints
  - [ ] Initial viewport positioning
  - [ ] Viewport changed events
  - [ ] Custom tile source providers
  - [ ] Overlay support (annotations, markers)
- [ ] Performance optimization:
  - [ ] Background tile loading
  - [ ] Render throttling
  - [ ] Memory management
- [ ] Add comprehensive examples:
  - [ ] Basic viewer usage
  - [ ] Custom tile sources
  - [ ] Annotation overlays
  - [ ] Multi-viewer synchronization
- [ ] Create documentation and demos

**New files to create:**
- `src/Extensions/OoBDev.Extensions.WPF.DeepZoom/Controls/DeepZoomViewer.xaml`
- `src/Extensions/OoBDev.Extensions.WPF.DeepZoom/TileLoaders/`
- `src/Extensions/OoBDev.Extensions.WPF.DeepZoom/Caching/`
- `src/Extensions/OoBDev.Extensions.WPF.DeepZoom.Demo/` (sample app)

##### 5.1.2 JavaScript/TypeScript DeepZoom Viewer Library
- [ ] Create `src/Web/OoBDev.Web.DeepZoom/` (TypeScript library)
- [ ] Design JavaScript/TypeScript API:
  - [ ] DeepZoomViewer class
  - [ ] Configuration options
  - [ ] Event system
  - [ ] Plugin architecture
- [ ] Implement core viewer:
  - [ ] Canvas-based rendering
  - [ ] Touch/mouse/wheel event handling
  - [ ] Smooth zoom animations (CSS transitions or requestAnimationFrame)
  - [ ] Progressive tile loading with preloading
- [ ] Add features:
  - [ ] Responsive design (resize handling)
  - [ ] Mobile-optimized touch gestures
  - [ ] Accessibility (keyboard navigation, ARIA)
  - [ ] SVG overlay support
  - [ ] Custom controls (zoom in/out, home, fullscreen)
- [ ] Framework integrations:
  - [ ] Vanilla JavaScript/TypeScript
  - [ ] React component wrapper
  - [ ] Angular component wrapper (optional)
  - [ ] Vue component wrapper (optional)
- [ ] Build tooling:
  - [ ] TypeScript compilation
  - [ ] Module bundling (ESM, UMD, CommonJS)
  - [ ] Minification
  - [ ] Source maps
- [ ] Add comprehensive examples:
  - [ ] Basic HTML usage
  - [ ] React integration
  - [ ] Custom tile sources
  - [ ] Annotation layers
- [ ] Create documentation site:
  - [ ] API reference
  - [ ] Interactive demos
  - [ ] Getting started guide

**New files to create:**
- `src/Web/OoBDev.Web.DeepZoom/src/DeepZoomViewer.ts`
- `src/Web/OoBDev.Web.DeepZoom/src/TileLoader.ts`
- `src/Web/OoBDev.Web.DeepZoom/src/Cache.ts`
- `src/Web/OoBDev.Web.DeepZoom/react/` (React wrapper)
- `src/Web/OoBDev.Web.DeepZoom/examples/`
- `src/Web/OoBDev.Web.DeepZoom/docs/`

**Package Distribution:**
- NPM package: `@oodev/deepzoom-viewer`
- NuGet package: `OoBDev.Extensions.WPF.DeepZoom`

#### 5.2 Code Cleanup
- [ ] Remove obsolete/unused code
- [ ] Consolidate duplicate utilities
- [ ] Verify all projects have README.md files
- [ ] Ensure consistent coding standards

#### 5.3 Testing
- [ ] Achieve 80%+ code coverage for Framework layer
- [ ] Add integration tests for migrated components
- [ ] Add performance benchmarks where applicable

#### 5.4 Documentation
- [ ] Update all component README.md files
- [ ] Update architecture documentation with new components
- [ ] Create migration notes document
- [ ] Add API documentation examples

#### 5.5 Final Validation
- [ ] Run full test suite
- [ ] Run security audit (use security-audit.md protocol)
- [ ] Run architectural compliance check
- [ ] Update GitVersion.yml if needed

---

## Migration Priorities

**NOTE:** ALL features from BinaryDataDecoders will be migrated. Phases indicate priority order, not feature selection.

### Priority 1: Foundation (Phase 1)
**Why:** Required by other phases, fixes existing gaps
- Endianness support (needed by protocols and binary readers)
- Utility enhancements (quality of life improvements)
- BinaryPrimitives expansion (foundation for all binary operations)

### Priority 2: Core Features (Phase 2)
**Why:** Substantial new capabilities with broad applicability
- CodeAnalysis (valuable for code generation and analysis tools)
- ExpressionCalculator (already partially exists, needs completion)
- Archive support (useful for many scenarios)
- BinaryData enhancements (bit-level operations, checksums)

### Priority 3: Protocols (Phase 3)
**Why:** Self-contained protocol implementations for specific domains
- NMEA (maritime/GPS applications)
- Drawing/Geometry (graphics primitives and operations)

### Priority 4: Specialized Domains (Phase 4)
**Why:** Domain-specific features for specialized use cases, all will be migrated
- FileSystems (ISO 9660 for CD/DVD image access)
- Classic Cryptography (educational/CTF purposes with security warnings)
- Retro Computing (Apple II legacy data recovery, DOS 3.3 support)
- Hardware Devices (8 specialized devices: Busylight, RadexOne, K8055, H4n, Fencing, LANC, EByte, ZWave)
- CLI Tools (4 command-line utilities)
- Platform-specific code (UWP/Framework - migrate or delete based on modern equivalents)

### Priority 5: Finalization (Phase 5)
**Why:** Polish, documentation, and project completion
