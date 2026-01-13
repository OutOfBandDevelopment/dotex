# TODO - OoBDev (dotex) Framework

**Last Updated:** 2026-01-12

> **New to this project?** Read `/CLAUDE.md` first for a complete development guide including architecture, patterns, and migration scope.

---

## Overview

This document tracks architectural work, migration planning, and bug fixes for the OoBDev framework.

### Migration Scope

**ALL features from BinaryDataDecoders will be migrated** - phases indicate priority order, not feature selection.

- Even highly specialized and niche features will be maintained
- Incomplete features will be migrated and tracked here for future completion
- No features will be skipped unless:
  - Stub/placeholder projects with zero implementation
  - Platform-specific with no .NET 9.0 equivalent (e.g., Silverlight-only)
  - Completely obsolete without modern use cases

---

## Completed Work ✓

### Architecture Documentation
- [x] Created `docs/architecture/README.md` - Overview and quick reference
- [x] Created `docs/architecture/architectural-guidelines.md` - 14 core principles
- [x] Created `docs/architecture/architectural-standards.md` - Enforceable standards
- [x] Created `docs/architecture/architectural-patterns.md` - 11 pattern catalog
- [x] Created `docs/architecture/layering-architecture.md` - Layer-by-layer breakdown

### Migration Analysis - BinaryDataDecoders
- [x] Deep comparison of `Incomming/BinaryDecoders` vs `src/`
- [x] Created `docs/migration/binarydatadecoders-feature-mapping.md` - Feature comparison
- [x] Created `docs/migration/binarydatadecoders-migration-plan.md` - 5-phase plan
- [x] Created `docs/migration/binarydatadecoders-critical-questions.md` - Decision points
- [x] Created `docs/migration/README.md` - Migration tracking

### Migration Analysis - dotnet-lib (COMPLETE & DELETED)
- [x] Deep comparison of `Incomming/dotnet-lib` vs `src/` - All 40 files
- [x] Created `docs/migration/dotnet-lib-feature-mapping.md` - Feature analysis
- [x] Created `docs/migration/dotnet-lib-migration-plan.md` - Investigation plan
- [x] Created `docs/migration/dotnet-lib-comparison-matrix.md` - File-by-file comparison
- [x] **Deleted `Incomming/dotnet-lib/`** - All valuable information extracted

### dotnet-lib Investigation Results (COMPLETE)
- [x] **Phase 0: Investigation**
  - [x] Compared all 40 C# files against main codebase
  - [x] Result: 38 files (95%) IDENTICAL, 2 files differ
  - [x] All files exist in main codebase - No migration needed
- [x] **Phase 1: Critical Bug Fix**
  - [x] **AdditionalSwaggerGenEndpointsOptions.cs** - Fixed missing namespace prefix for generic types
    - Location: `src/Framework/OoBDev.AspNetCore.Mvc/SwaggerGen/AdditionalSwaggerGenEndpointsOptions.cs:111-116`
    - Issue: Main codebase missing `{type.Namespace}.` in `ResolveSchemaType` method
    - Impact: Swagger schema IDs for generic types now properly qualified
    - Severity: HIGH - Prevented potential schema naming collisions
  - [x] **HealthCheckSwaggerGenEndpointOptions.cs** - Stylistic difference only, no action needed
- [x] **Phase 2: Documentation**
  - [x] Documented synchronization status (95% identical)
  - [x] Updated all migration documentation with completion status
  - [x] Deleted source directory after extracting all valuable information

**Outcome:** dotnet-lib was fully synchronized with main codebase. Critical bug fix applied. **Directory deleted 2026-01-12.**

### Migration Analysis - OoBDev.Oobtainium (INVESTIGATION COMPLETE)
- [x] Deep analysis of `Incomming/OoBDev.Oobtainium` structure and features
- [x] Created `docs/migration/oobtainium-feature-mapping.md` - Comprehensive feature analysis
- [x] Created `docs/migration/oobtainium-migration-plan.md` - Migration options and plan
- [x] Compared against existing mocking frameworks (Moq, NSubstitute, FakeItEasy)

### OoBDev.Oobtainium Investigation Results (COMPLETE)
- [x] **Discovery:**
  - [x] Oobtainium does NOT exist in main codebase (new library)
  - [x] Complete mocking/proxy framework (48 files, ~1,578 LOC)
  - [x] Uses .NET Standard 2.1 with 3.1.9 dependencies (outdated)
  - [x] GitHub repository: https://github.com/OutOfBandDevelopment/oobtainium/
- [x] **Analysis:**
  - [x] Runtime proxy creation using DispatchProxy
  - [x] Method binding with fluent API
  - [x] Call recording infrastructure
  - [x] Full async/await support
  - [x] DI integration (ServiceCollection)
  - [x] Good architecture (Abstractions + Implementation + Tests)
- [x] **Comparison:**
  - [x] Simpler than Moq but less feature-rich
  - [x] Lightweight (1,578 LOC vs Moq's 20,000+)
  - [x] Limited matchers and verification compared to established frameworks
- [x] **Decision Options:**
  - Option 1: MIGRATE to main framework (requires .NET 9.0 upgrade, moderate effort)
  - Option 2: REFERENCE as external NuGet package (low effort)
  - Option 3: ARCHIVE in Incomming/ (no effort)
  - Option 4: DELETE (minimal effort)

**RECOMMENDATION: Option 4 (Delete)** - Mocking is well-solved by Moq/NSubstitute. OoBDev's value is in unique features (binary processing, protocols, hardware) not mocking tools. Focus resources on differentiating capabilities.

**Outcome:** Investigation complete. **Awaiting decision on which option to execute.**

### Migration Analysis - Incomming/Framework (INVESTIGATION COMPLETE)
- [x] Deep analysis of `Incomming/Framework` structure and features (55 files, ~2,054 LOC)
- [x] Created `docs/migration/framework-feature-mapping.md` - Comprehensive feature analysis
- [x] File-by-file comparison with main OoBDev codebase

### Incomming/Framework Investigation Results (COMPLETE)
- [x] **Discovery:**
  - [x] Contains 3 projects: OoBDev.Common.Abstractions, OoBDev.Common, OoBDev.AspNetCore.Extensions
  - [x] 55 implementation files vs main's 1 aggregator file
  - [x] Main OoBDev.Common is meta-package (references other projects), Incomming has actual implementations
  - [x] Multi-targets .NET 8.0 and 9.0
- [x] **Comparison Results:**
  - [x] **0 files (0%)** - IDENTICAL to main codebase
  - [x] **25 files (45%)** - DIFFER (exist in both but have changes) - **Requires systematic comparison**
  - [x] **30 files (55%)** - NEW (only in Incomming)
- [x] **Key NEW Features:**
  - [x] **Vector Math Library** (5 files) - AI/ML vector operations for SemanticKernel
  - [x] **SQL Database Mapper** (1 file) - ORM-like stored procedure mapping with reflection
  - [x] **Audit Logging System** (5 files) - Request/response capture middleware
  - [x] **User & Application Access** (5 files) - Current user resolution, multi-tenancy
  - [x] **Environment Settings** (4 files) - Centralized configuration
  - [x] **Swagger/OpenAPI Customizations** (4 files) - Internal endpoint hiding
  - [x] **HTTP Extensions** (2 files) - Request preparation utilities
  - [x] **JSON Serializer Wrapper** (1 file) - Centralized JSON configuration
  - [x] **Validation Attributes** (1 file) - QuoteIdAttribute
- [x] **Branding Issue Found:**
  - [x] "EriskInternalDocumentFilter.cs" needs renaming to "OoBDevInternalDocumentFilter.cs"
- [x] **Critical Work Required:**
  - [ ] **Phase 0:** Systematic diff of all 25 DIFFERS files
  - [ ] **Namespace Decision:** Map "OoBDev.Common" to main Framework structure
  - [ ] **Project Placement:** Where to place new features (OoBDev.System? OoBDev.Data.Common?)
  - [ ] **Branding Audit:** Search and replace "Erisk" references

**COMPLEXITY:** HIGH - Not a simple sync like dotnet-lib. Requires:
1. File-by-file comparison of 25 DIFFERS files
2. Strategic decisions on namespace mapping
3. Integration of 30 NEW feature files
4. Breaking change analysis

**RECOMMENDATION:** Requires systematic Phase 0 comparison before migration decisions.

**Outcome:** Investigation complete. **Phase 0 (file comparison) required before migration.**

### Protocols
- [x] Created `.claude/protocols/software/incoming-codebase-comparison.md`
- [x] Updated `.claude/protocols/software/README.md` with new protocol

### CI/CD
- [x] Updated `.github/workflows/dotnet.yml` - Added path filters for selective compilation

### Phase 0: Critical Bug Fixes (COMPLETED)
- [x] **PathEx.cs** - Fixed lambda expression syntax error (lines 42, 66, 92)
- [x] **StreamDevice.cs** - Fixed nullable annotations and transmission delay typo
- [x] **SerialPortFactory.cs** - Simplified verbose ternary expression
- [x] **ShiftCommutativeVariablesRight.cs** - Replaced non-functional stub with working implementation
- [x] **ExpressionParserTests.cs** - Fixed floating-point precision test failures by adding epsilon tolerance
- [x] **NumericAsserts.cs** - Created reusable numeric comparison utility in OoBDev.TestUtilities

---

## Pending Work

### Build Verification (COMPLETED)
- [x] Run `dotnet build src/` to verify all bug fixes compile
- [x] Run `dotnet test src/` to verify all tests pass - ALL PASSED
- [x] Address any compilation or test failures - None found

---

## BinaryDataDecoders Migration - Decision Points

**Status:** ⏸️ BLOCKED - Awaiting decisions on migration approach

Before proceeding with BinaryDataDecoders migration (Phases 1-5), critical questions need answers. See:
- **[Critical Questions Document](docs/migration/binarydatadecoders-critical-questions.md)** - Complete decision matrix

### Summary of Required Decisions

**Immediate (Phase 1 - Foundation):**
- [ ] **Endianness API design** - Extension methods, static methods, or both?
- [ ] **BinaryPrimitives naming** - `ReadInt32BigEndian()` style preferred?
- [ ] **UI Collections location** - Create `OoBDev.Extensions.UI.Collections`?

**High Priority (Phase 2 - High-Value Features):**
- [ ] **CodeAnalysis use case** - What are you building with Roslyn extensions?
- [ ] **Archive formats** - Which formats needed (TAR, CPIO, ZIP)?
- [ ] **ExpressionCalculator audit** - Is current implementation sufficient?

**Medium Priority (Phase 3 - Protocols):**
- [ ] **NMEA Protocol** - GPS hardware integration or data file parsing?
- [ ] **Drawing/Geometry** - Migrate or use existing libraries (SkiaSharp, ImageSharp)?
- [ ] **Barcode** - Which formats? Use ZXing.Net or custom?

**Lower Priority (Phase 4 - Specialized):**
- [ ] **Hardware devices** - Which of the 8 devices are actively used?
- [ ] **CLI tools** - Which of the 4 tools should be migrated?
- [ ] **ISO 9660, Apple II, Classic Crypto** - Active use cases?
- [ ] **Windows Forms, UWP** - Modernization approach?

**Recommendation:** Can start Phase 1 with recommended defaults while Phase 2-4 decisions are made.

---

## OoBDev.Oobtainium - Migration Decision

**Status:** ⏸️ PENDING DECISION - Choose migration approach

**What is Oobtainium?**
- Complete mocking/proxy framework (48 files, ~1,578 LOC)
- Runtime interface proxies using DispatchProxy
- Method call recording and binding
- Does NOT exist in main OoBDev codebase (new library)
- GitHub: https://github.com/OutOfBandDevelopment/oobtainium/

**Current State:**
- .NET Standard 2.1 (needs upgrade to .NET 9.0)
- Microsoft.Extensions.* 3.1.9 dependencies (2020 - outdated)
- Good architecture: Abstractions + Implementation + Tests
- Simpler than Moq but less feature-rich

**Decision Required - Choose ONE of four options:**

### Option 1: MIGRATE to Main Framework
- **Action:** Upgrade to .NET 9.0, integrate into `src/Framework/OoBDev.Mocking/` or `src/Extensions/OoBDev.Extensions.Mocking/`
- **Effort:** MEDIUM (upgrade dependencies, rename namespaces, add documentation)
- **Maintenance:** HIGH (ongoing .NET updates, feature additions)
- **Pros:** First-party mocking solution, DI-integrated, simpler than Moq for basic scenarios
- **Cons:** Maintenance burden, duplicates existing tools (Moq, NSubstitute)
- **See:** [Migration Plan](docs/migration/oobtainium-migration-plan.md) - Phase 1-5 detailed steps

### Option 2: REFERENCE as External NuGet Package
- **Action:** Verify if published to NuGet, or publish it, then reference where needed
- **Effort:** LOW (add PackageReference)
- **Maintenance:** MINIMAL (external updates)
- **Pros:** No code maintenance, separate evolution
- **Cons:** Dependency on external package, may not be published

### Option 3: ARCHIVE in Incomming/
- **Action:** Create README.md documenting decision, keep for reference
- **Effort:** MINIMAL (documentation only)
- **Maintenance:** NONE
- **Pros:** Available for future reconsideration, no commitment
- **Cons:** Not integrated, users won't discover it

### Option 4: DELETE ⭐ RECOMMENDED
- **Action:** Remove `/current/src/Incomming/OoBDev.Oobtainium` directory
- **Effort:** MINIMAL (rm -rf, update docs)
- **Maintenance:** NONE
- **Pros:** No burden, focus on unique OoBDev features, Moq/NSubstitute already solve this
- **Cons:** Lose lightweight alternative
- **Rationale:**
  - Mocking is well-solved (Moq: 460M+ downloads, NSubstitute: 130M+)
  - OoBDev's value is in unique features (binary processing, protocols, hardware)
  - Limited differentiation vs established frameworks
  - Better resource allocation on BinaryDataDecoders migration

**Documentation:**
- [Feature Mapping](docs/migration/oobtainium-feature-mapping.md) - Complete feature analysis and comparison
- [Migration Plan](docs/migration/oobtainium-migration-plan.md) - All 4 options with detailed steps

**Next Steps:**
- [ ] **DECISION:** Choose Option 1, 2, 3, or 4
- [ ] Execute chosen option per migration plan

---

## BinaryDataDecoders Migration Work

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
- [ ] Ensure .NET 9.0 Windows Forms compatibility
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
  - [ ] Port to .NET 9.0 if valuable
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

---

## Known Issues

### Critical (Fixed, Pending Verification)
- [x] PathEx lambda syntax error - FIXED
- [x] StreamDevice nullable annotations - FIXED
- [x] StreamDevice transmission delay typo - FIXED
- [x] SerialPortFactory verbose ternary - FIXED
- [x] ShiftCommutativeVariablesRight stub - FIXED
- [x] ExpressionParserTests floating-point precision - FIXED (added epsilon tolerance)
- [x] NumericAsserts utility created - Migrated to OoBDev.TestUtilities for reuse

### To Investigate
- [ ] Verify ExpressionCalculator optimizers match incoming implementation
- [ ] Verify ANTLR grammar versions match
- [ ] Check for other stub implementations in codebase

---

## Reference Documentation

### Architecture
- `docs/architecture/README.md` - Overview
- `docs/architecture/architectural-guidelines.md` - Principles
- `docs/architecture/architectural-standards.md` - Standards
- `docs/architecture/architectural-patterns.md` - Patterns
- `docs/architecture/layering-architecture.md` - Layer details

### Migration
- `docs/migration/README.md` - Migration overview
- `docs/migration/binarydatadecoders-feature-mapping.md` - Feature comparison
- `docs/migration/binarydatadecoders-migration-plan.md` - Detailed plan

### Protocols
- `.claude/protocols/software/incoming-codebase-comparison.md` - Comparison protocol
- `.claude/protocols/software/architectural-analysis.md` - Architecture protocol
- `.claude/protocols/software/security-audit.md` - Security protocol

---

## Notes

- All new code must follow `docs/architecture/architectural-standards.md`
- All new projects must have README.md (build-enforced)
- Framework layer requires 80%+ test coverage
- Use provider/factory pattern for extensible components
- Follow existing dependency injection patterns
- Maintain .NET 9.0 target framework
- Keep nullable reference types enabled
- Keep implicit usings disabled
