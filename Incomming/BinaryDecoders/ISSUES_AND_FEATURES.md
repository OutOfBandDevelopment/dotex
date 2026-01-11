# BinaryDataDecoders - Issues and Features Tracking

**Source Repository:** https://github.com/mwwhited/BinaryDataDecoders
**Total Issues:** 53 (18 open, 35 closed)
**Generated:** 2026-01-11
**Purpose:** Track all issues from BinaryDataDecoders for migration to dotex

---

## Table of Contents

1. [Open Issues Summary](#open-issues-summary)
2. [Open Issues by Category](#open-issues-by-category)
3. [Detailed Open Issues](#detailed-open-issues)
4. [Closed Issues Reference](#closed-issues-reference)
5. [Migration Recommendations](#migration-recommendations)

---

## Open Issues Summary

| # | Title | Labels | Created | Priority for dotex |
|---|-------|--------|---------|-------------------|
| 43 | create markdown with plantuml support | - | 2023-05-20 | ⭐⭐⭐ HIGH |
| 42 | create handlebars template | - | 2023-05-20 | ⭐⭐⭐ HIGH |
| 41 | create html binding template | - | 2023-05-20 | ⭐⭐⭐ HIGH |
| 30 | clean up warnings and messages | - | 2021-08-08 | ⭐⭐ MEDIUM |
| 28 | EcoWitt support | - | 2021-08-08 | ⭐ LOW |
| 27 | move templates to use embedded resources | - | 2021-08-08 | ⭐⭐ MEDIUM |
| 25 | Create tokenizer/detokenizer for basicstamp | - | 2020-12-06 | ⭐ LOW |
| 22 | add midi library support | enhancement | 2020-12-04 | ⭐⭐ MEDIUM |
| 21 | create GitBook documentation | documentation, enhancement | 2020-12-04 | ⭐ LOW |
| 20 | add shield.io + nuget badges | documentation | 2020-12-03 | ⭐ LOW |
| 19 | work on publish target for rptproj | documentation, enhancement | 2020-12-03 | ⭐ LOW |
| 18 | Add FAT12,16,32 filesystem support | enhancement | 2020-12-03 | ⭐⭐⭐ HIGH |
| 17 | replace file path parser with Microsoft.Extensions | enhancement | 2020-12-03 | ⭐⭐ MEDIUM |
| 16 | Invert segmenter from base class to interface | - | 2020-12-03 | ⭐⭐⭐ HIGH |
| 15 | Build XPathNavigator wrapping CodeAnalysis | - | 2020-12-03 | ⭐⭐⭐ HIGH |
| 14 | Convert Archive Tar to ReadOnlySequence<> | - | 2020-12-03 | ⭐⭐ MEDIUM |
| 13 | Add ZIP file format to Archive | - | 2020-12-03 | ⭐⭐⭐ HIGH |
| 12 | Update Apple2Encoding bidirectionality | - | 2020-12-03 | ⭐ LOW |
| 11 | Add language parser for ApplesoftBASIC | - | 2020-12-03 | ⭐ LOW |
| 10 | Add ability to store data back to Dos33 format | - | 2020-12-03 | ⭐ LOW |
| 9 | Refactor ApplesoftBasic Detokenizer | - | 2020-12-03 | ⭐⭐ MEDIUM |
| 7 | Create TestUtility with CSV data | - | 2020-12-03 | ⭐⭐ MEDIUM |

---

## Open Issues by Category

### 📝 Templating & Documentation (5 issues)

#### ⭐⭐⭐ HIGH PRIORITY

**#43 - Create markdown with plantuml support**
- **Created:** 2023-05-20
- **Description:** Feature request for markdown with PlantUML diagram support
- **dotex Status:** ❌ PlantUML not supported in dotex's Markdig integration
- **Recommendation:** **IMPLEMENT** - Add PlantUML extension to OoBDev.Markdig
- **Effort:** Low (1-2 days)
- **Value:** High - Excellent for documentation

**#42 - Create handlebars template**
- **Created:** 2023-05-20
- **Description:** Implement handlebars template functionality
- **dotex Status:** ✅ ALREADY EXISTS - OoBDev.Handlebars has full Handlebars support
- **Recommendation:** **CLOSE** - Already implemented in dotex
- **Action:** Mark as resolved/duplicate

**#41 - Create html binding template**
- **Created:** 2023-05-20
- **Description:** Support binding paths using JSON path, XML paths, and CSS selectors with HTML templates
- **dotex Status:** ✅ ALREADY EXISTS - OoBDev.TextTemplating has GenerateText with JSONPath binding
- **Recommendation:** **CLOSE** - Already implemented in dotex
- **Action:** Verify dotex implementation covers all requirements, then close

#### ⭐ LOW PRIORITY

**#21 - Create GitBook documentation**
- **Created:** 2020-12-04
- **Labels:** documentation, enhancement
- **Description:** Investigate GitBook integration for documentation
- **URL:** https://www.gitbook.com/
- **Recommendation:** **EVALUATE** - Consider for dotex documentation strategy
- **Action:** Defer until Phase 4

**#20 - Add shield.io + nuget badges**
- **Created:** 2020-12-03
- **Labels:** documentation
- **Description:** Add badges to assembly doc generation with per-assembly summaries
- **Recommendation:** **EVALUATE** - Consider for dotex documentation
- **Action:** Low priority, defer

---

### 🏗️ Architecture & Code Quality (5 issues)

#### ⭐⭐⭐ HIGH PRIORITY

**#16 - Invert segmenter from base class to interface**
- **Created:** 2020-12-03
- **Description:**
  - Should IO Segmenters be in abstraction library or IO library?
  - Convert segmenter from base class to interface passed into pipeline
- **dotex Status:** Segmenters currently use base classes
- **Recommendation:** **IMPLEMENT** - Improves testability and flexibility
- **Effort:** Medium (3-5 days)
- **Impact:** Affects OoBDev.System.IO.Pipelines architecture
- **Action:** Refactor after Phase 1 migration

**#15 - Build XPathNavigator wrapping CodeAnalysis**
- **Created:** 2020-12-03
- **Description:** Update XPathNavigators to work with CodeAnalysis and add semantic model support
- **dotex Status:** ❌ Not implemented
- **Recommendation:** **IMPLEMENT** - Part of CodeAnalysis migration
- **Dependencies:** Requires BinaryDataDecoders.CodeAnalysis migration (Phase 2)
- **Effort:** Medium (3-5 days)
- **Value:** High - Enables code queries via XPath
- **Action:** Include in Phase 2 (CodeAnalysis integration)

#### ⭐⭐ MEDIUM PRIORITY

**#30 - Clean up warnings and messages**
- **Created:** 2021-08-08
- **Description:** Address and resolve warnings and messages in codebase
- **Recommendation:** **IMPLEMENT** - Apply during migration
- **Action:** Fix all warnings when integrating into dotex
- **Note:** Use this as quality gate during migration

**#17 - Replace file path parser with Microsoft.Extensions**
- **Created:** 2020-12-03
- **Labels:** enhancement
- **Description:** Replace custom file globbing with Microsoft.Extensions.FileSystemGlobbing
- **Current:** Custom implementation in XSLT process and toolkit
- **Recommendation:** **IMPLEMENT** - Standardize on Microsoft libraries
- **Effort:** Medium (2-3 days)
- **Action:** Include in ToolKit migration (Phase 1)

**#9 - Refactor ApplesoftBasic Detokenizer**
- **Created:** 2020-12-03
- **Description:** Modernize to use ReadOnlySpan<> or ReadOnlySequence<> instead of IEnumerable<>
- **Recommendation:** **IMPLEMENT** - Performance improvement
- **Effort:** Low (1-2 days)
- **Action:** Apply when migrating Apple2 projects (Phase 4)

---

### 📦 File Formats & Archives (4 issues)

#### ⭐⭐⭐ HIGH PRIORITY

**#18 - Add FAT12,16,32 filesystem support**
- **Created:** 2020-12-03
- **Labels:** enhancement
- **Description:** Add filesystem support for FAT12, FAT16, and FAT32
- **dotex Status:** ❌ Not implemented (only has ISO 9660)
- **Recommendation:** **IMPLEMENT** - Common filesystem formats
- **Effort:** High (1-2 weeks)
- **Value:** High - Very useful for embedded systems and legacy devices
- **Action:** Include in Phase 3 or 4

**#13 - Add ZIP file format to Archive**
- **Created:** 2020-12-03
- **Description:** Validate ZIP format port from older code store
- **dotex Status:** ❌ BDD has partial ZIP support, dotex has none
- **Recommendation:** **IMPLEMENT** - Complete ZIP support
- **Effort:** Medium (3-5 days)
- **Dependencies:** Part of Archives migration
- **Action:** Include in Phase 2 (Archives integration)
- **Note:** System.IO.Compression already provides ZIP, but BDD has custom implementation

#### ⭐⭐ MEDIUM PRIORITY

**#14 - Convert Archive Tar to ReadOnlySequence<>**
- **Created:** 2020-12-03
- **Description:** Modernize TAR to use ReadOnlySequence<> instead of byte[]
- **Recommendation:** **IMPLEMENT** - Performance and memory improvement
- **Effort:** Medium (2-3 days)
- **Action:** Apply during Archives migration (Phase 2)

**#7 - Create TestUtility with CSV data**
- **Created:** 2020-12-03
- **Description:** Create reusable test utility for CSV-based test data with configurable parameters
- **Code snippet provided:** Dynamic data from CSV resources
- **Recommendation:** **IMPLEMENT** - Useful for data-driven tests
- **Effort:** Low (1-2 days)
- **Action:** Include in Phase 2 (TestUtilities migration)

---

### 🎵 Hardware & Protocol Support (4 issues)

#### ⭐⭐ MEDIUM PRIORITY

**#22 - Add MIDI library support**
- **Created:** 2020-12-04
- **Labels:** enhancement
- **Description:** Implement MIDI support for file format and serial protocol
- **References:**
  - CMU Standard MIDI File Format: http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html
  - SparkFun MIDI: https://www.sparkfun.com/categories/218
  - Wikipedia MIDI: https://en.wikipedia.org/wiki/MIDI
- **dotex Status:** ❌ Not implemented
- **Recommendation:** **EVALUATE** - Niche but valuable for audio applications
- **Effort:** High (1-2 weeks)
- **Action:** Phase 4 - Selective integration based on need

#### ⭐ LOW PRIORITY

**#28 - EcoWitt support**
- **Created:** 2021-08-08
- **Description:** Implement decoder for EcoWitt weather station data
- **URL:** http://www.ecowitt.com
- **dotex Status:** ❌ Not implemented
- **Recommendation:** **LOW PRIORITY** - Very specialized weather hardware
- **Action:** Phase 4 or skip - Only if weather station support needed

**#25 - Create tokenizer/detokenizer for BasicStamp**
- **Created:** 2020-12-06
- **Description:** Tools to tokenize/detokenize code for Basic Stamp microcontroller
- **References:**
  - http://www.robotics.mcmanis.com/robots/stamps/decoding.html
  - https://www.parallax.com/product/basic-stamp-2-microcontroller-module/
- **dotex Status:** ❌ Not implemented
- **Recommendation:** **LOW PRIORITY** - Very specialized embedded hardware
- **Action:** Skip unless embedded microcontroller support is strategic

**#27 - Move templates to use embedded resources**
- **Created:** 2021-08-08
- **Description:**
  - Use embedded resources as default fallback for templates
  - "If templates not provided from command line use embedded resources"
  - Support naming conventions or .NET attributes tied to file types
  - Support multiple embedded resources per file type with selection
- **dotex Status:** Partial - OoBDev.TextTemplating has database-backed templates
- **Recommendation:** **CONSIDER** - Embedded resources as fallback is good pattern
- **Effort:** Low (1-2 days)
- **Action:** Include in templating enhancements (Phase 2)

---

### 🍎 Apple II / Retro Computing (4 issues)

#### ⭐ LOW PRIORITY (All specialized)

**#12 - Update Apple2Encoding bidirectionality**
- **Created:** 2020-12-03
- **Description:**
  - Current implementation strips upper bit on decode but doesn't restore on encode
  - Need bidirectional conversion: String ↔ Bytes with upper bit preserved
- **Recommendation:** **FIX** - If migrating Apple2 support
- **Effort:** Low (few hours)
- **Action:** Phase 4 - Fix during Apple2 migration

**#11 - Add language parser for ApplesoftBASIC**
- **Created:** 2020-12-03
- **Description:**
  - Create ANTLR4 parser for AppleSoft Basic
  - Build abstract syntax tree
  - Convert AST to tokens
- **Recommendation:** **EVALUATE** - Interesting but very specialized
- **Effort:** High (1-2 weeks for grammar + parser)
- **Action:** Phase 4 or skip - Retro computing niche

**#10 - Add ability to store data back to Dos33 format**
- **Created:** 2020-12-03
- **Description:** Extend DOS33 library to support write operations (currently read-only)
- **Recommendation:** **IMPLEMENT** - If supporting Apple II
- **Effort:** Medium (3-5 days)
- **Action:** Phase 4 - Complete DOS33 bidirectionality

**#19 - Work on publish target for rptproj**
- **Created:** 2020-12-03
- **Labels:** documentation, enhancement
- **Description:** Develop custom SDK project type or publish profile for reports
- **Recommendation:** **SKIP** - Very specialized reporting scenario
- **Action:** Not applicable to dotex

---

## Detailed Open Issues

### 🔴 CRITICAL - Implement Immediately

These issues represent features that are either:
- Already resolved in dotex (close as duplicate)
- High-value additions that align with dotex goals

---

#### Issue #43: Create markdown with plantuml support

**Status:** Open
**Created:** 2023-05-20
**Author:** mwwhited (Owner)

**Description:**
Feature request to implement markdown functionality with PlantUML diagram support for enhanced documentation capabilities.

**Current State:**
- No body content provided (placeholder issue)
- No comments or activity since creation

**dotex Analysis:**
- ✅ dotex has Markdig integration (OoBDev.Markdig)
- ❌ PlantUML support NOT implemented
- 📊 PlantUML is valuable for:
  - Architecture diagrams
  - Sequence diagrams
  - Class diagrams
  - Component diagrams

**Implementation Path:**
1. Add PlantUML extension to Markdig
2. Options:
   - Use PlantUML.Net package
   - Shell out to PlantUML JAR
   - Use PlantUML web service
3. Integrate into OoBDev.Markdig project

**Recommendation:** ⭐⭐⭐ **IMPLEMENT HIGH PRIORITY**

**Effort:** Low (1-2 days)

**Files Affected:**
- Add to: `/current/src/dotex/src/ExternalServices/OoBDev.Markdig/`
- New class: `PlantUmlExtension.cs`

**Migration Action:**
- [ ] Add PlantUML support to OoBDev.Markdig
- [ ] Add unit tests with sample diagrams
- [ ] Update documentation
- [ ] Close issue #43 as implemented in dotex

---

#### Issue #42: Create handlebars template

**Status:** Open
**Created:** 2023-05-20
**Author:** mwwhited (Owner)

**Description:**
Feature request to implement Handlebars template functionality.

**Current State:**
- No detailed description provided
- Appears to be placeholder for Handlebars integration

**dotex Analysis:**
- ✅ **ALREADY IMPLEMENTED** in OoBDev.Handlebars
- ✅ Full Handlebars.Net integration
- ✅ Helper system with:
  - DateNow, GuidNew, Get, Set, Hash, StringReplace
- ✅ JSON support via HandlebarsDotNet.Extension.Json
- ✅ Block helpers and inline helpers
- ✅ NoEscape configuration
- ✅ Content type: `text/x-handlebars-template`

**Recommendation:** ⭐⭐⭐ **CLOSE AS DUPLICATE**

**Migration Action:**
- [ ] Verify dotex Handlebars implementation meets requirements
- [ ] Document capabilities in FEATURE_INVENTORY.md
- [ ] Close issue #42 as already implemented in dotex
- [ ] Reference: `/current/src/dotex/src/ExternalServices/OoBDev.Handlebars/`

---

#### Issue #41: Create html binding template

**Status:** Open
**Created:** 2023-05-20
**Author:** mwwhited (Owner)

**Description:**
Support binding paths using JSON path, XML paths, and CSS selectors in conjunction with HTML templates.

**Current State:**
- No detailed implementation notes
- Placeholder for data binding feature

**dotex Analysis:**
- ✅ **ALREADY IMPLEMENTED** in OoBDev.TextTemplating
- ✅ GenerateText has comprehensive binding:
  - `<value-of binding="$.path" />` - JSONPath binding
  - `<repeater item="x" binding="$.items">` - Collection binding
  - `<value-attr item="attrName" binding="$.value" />` - Attribute binding
  - `<condition rule="jsonpath-filter">` - Conditional rendering
  - `data-binding` attribute support
- ✅ JSONPath-based data binding via Newtonsoft.Json.Linq
- ✅ Scoped variable support
- ✅ Format support for dates and numbers

**Missing from dotex:**
- ❌ XML path binding (XPath)
- ❌ CSS selector binding

**Recommendation:** ⭐⭐⭐ **PARTIALLY COMPLETE**

**Migration Action:**
- [ ] Verify JSONPath binding covers requirements
- [ ] Evaluate need for XPath and CSS selector binding
- [ ] If needed, add XPath and CSS support to GenerateText
- [ ] Document existing capabilities
- [ ] Update issue #41 or close as substantially complete

**Files:**
- `/current/src/dotex/Incomming/SharedFramework/OoBDev.TextTemplating/GenerateText.cs`

---

#### Issue #16: Invert segmenter from base class to interface

**Status:** Open
**Created:** 2020-12-03
**Author:** mwwhited (Owner)

**Description:**
Two architectural concerns:
1. Should IO Segmenters be in abstraction library or IO library?
2. Convert segmenter from base class to interface passed into pipeline

**Current State:**
- Segmenters use base class pattern
- Located in IO.Abstractions

**dotex Analysis:**
- ⚠️ dotex has same pattern as BDD
- Current implementation: Base classes in IO.Abstractions
  - `BetweenSegmenter`
  - `StartAndFixLengthSegmenter`
  - `PassThroughSegmenter`
- These work but violate dependency inversion principle

**Recommendation:** ⭐⭐⭐ **IMPLEMENT - Architecture Improvement**

**Benefits:**
- Better testability (mock interfaces)
- Cleaner dependency injection
- Follows SOLID principles
- Allows custom segmenters without inheritance

**Implementation Plan:**
1. Create `ISegmenter` interface
2. Convert existing segmenters to implement interface
3. Update `PipelineBuilder` to accept `ISegmenter`
4. Move concrete implementations to IO library
5. Keep interface in Abstractions

**Effort:** Medium (3-5 days)

**Impact:** Breaking change for pipeline consumers

**Migration Action:**
- [ ] Refactor segmenter architecture in dotex
- [ ] Update all references to use interface
- [ ] Add migration guide for consumers
- [ ] Apply same pattern when integrating BDD code
- [ ] Close issue #16

**Files Affected:**
- `/current/src/dotex/src/Framework/OoBDev.System.IO.Abstractions/`
- `/current/src/dotex/src/Framework/OoBDev.System.IO.Pipelines/`

---

#### Issue #15: Build XPathNavigator wrapping CodeAnalysis

**Status:** Open
**Created:** 2020-12-03
**Author:** mwwhited (Owner)

**Description:**
Update XPathNavigators to work with Microsoft.CodeAnalysis and add semantic model support.

**Current State:**
- BDD has CodeAnalysis XPath navigators (CSharpNavigator, VisualBasicNavigator)
- Not yet in dotex

**dotex Analysis:**
- ❌ Not implemented in dotex
- ✅ BDD has complete implementation:
  - `CSharpNavigator` - C# syntax tree navigation
  - `CSharpSemanticNavigator` - Semantic model navigation
  - `VisualBasicNavigator` - VB syntax tree navigation
  - `SemanticModelNode` - Semantic information access

**Use Cases:**
- Query code structure with XPath
- Find classes, methods, properties by XPath expression
- Analyze code patterns
- Code transformation tools
- Documentation generation
- Code metrics extraction

**Example:**
```csharp
// Find all public classes
var navigator = new CSharpNavigator(syntaxTree);
var publicClasses = navigator.Select("//class[@accessibility='public']");
```

**Recommendation:** ⭐⭐⭐ **IMPLEMENT HIGH PRIORITY**

**Dependencies:**
- Requires BinaryDataDecoders.CodeAnalysis migration (Phase 2)
- Microsoft.CodeAnalysis.CSharp
- Microsoft.CodeAnalysis.VisualBasic

**Effort:** Medium (3-5 days)

**Value:** Very high - Enables powerful code analysis scenarios

**Migration Action:**
- [ ] Migrate BinaryDataDecoders.CodeAnalysis to dotex (Phase 2)
- [ ] Integrate XPath navigators for C# and VB
- [ ] Add semantic model navigation
- [ ] Create examples and documentation
- [ ] Add to OoBDev.CodeAnalysis project
- [ ] Close issue #15

**Destination:**
- Create: `/current/src/dotex/src/Framework/OoBDev.CodeAnalysis/`

---

#### Issue #18: Add FAT12,16,32 filesystem support

**Status:** Open
**Created:** 2020-12-03
**Labels:** enhancement
**Author:** mwwhited (Owner)

**Description:**
Add filesystem support for FAT12, FAT16, and FAT32 formats.

**Current State:**
- BDD has ISO 9660 support (CD-ROM filesystem)
- dotex has nothing (ISO 9660 will be migrated)

**Use Cases:**
- Reading floppy disk images
- Embedded system storage
- SD card filesystems
- Legacy system data recovery
- Forensic analysis
- Retro computing

**Recommendation:** ⭐⭐⭐ **IMPLEMENT HIGH PRIORITY**

**Value:** High - Very common filesystem formats

**Effort:** High (1-2 weeks)

**Implementation Scope:**
1. **FAT12** - Floppy disks (1.44MB, etc.)
2. **FAT16** - Small hard drives, older USB drives
3. **FAT32** - USB drives, SD cards, modern removable media

**Migration Action:**
- [ ] Research existing .NET FAT libraries
- [ ] Implement or integrate FAT12/16/32 support
- [ ] Add to OoBDev.FileSystems project
- [ ] Support read operations at minimum
- [ ] Consider write support (Phase 2)
- [ ] Add comprehensive tests with real disk images
- [ ] Close issue #18

**Destination:**
- Add to: `/current/src/dotex/Incomming/BinaryDecoders/src/BinaryDataDecoders.FileSystems/`

---

#### Issue #13: Add ZIP file format to Archive

**Status:** Open
**Created:** 2020-12-03
**Author:** mwwhited (Owner)

**Description:**
Validate ZIP format port from older code store.

**Current State:**
- BDD has partial ZIP support (LocalFileHeader, CompressionMethodType)
- System.IO.Compression provides full ZIP support in .NET

**dotex Analysis:**
- ❌ No custom ZIP implementation
- ✅ Can use System.IO.Compression.ZipArchive
- ⚠️ BDD has low-level ZIP format parsing

**Recommendation:** ⭐⭐ **EVALUATE BEFORE IMPLEMENTING**

**Questions:**
1. Does BDD ZIP provide capabilities beyond System.IO.Compression?
2. Is low-level format parsing needed?
3. Should dotex use built-in or custom implementation?

**Migration Action:**
- [ ] Review BDD ZIP implementation
- [ ] Compare with System.IO.Compression capabilities
- [ ] Decide: Use built-in vs migrate custom code
- [ ] If custom needed, integrate BDD ZIP support
- [ ] Document decision rationale
- [ ] Close issue #13

**Recommendation:** Use System.IO.Compression unless BDD has unique capabilities

---

### 🟡 MEDIUM PRIORITY

These issues should be addressed during migration phases but are not blocking.

---

#### Issue #30: Clean up warnings and messages

**Status:** Open
**Created:** 2021-08-08
**Author:** mwwhited (Owner)

**Description:**
Address and resolve warnings and messages in codebase.

**Recommendation:** ⭐⭐ **IMPLEMENT AS QUALITY GATE**

**Migration Action:**
- [ ] Fix all compiler warnings during migration
- [ ] Enable "treat warnings as errors"
- [ ] Use nullable reference types throughout
- [ ] Fix all XML documentation warnings
- [ ] Apply .editorconfig rules
- [ ] Close issue #30 when dotex integration is warning-free

**Quality Standards for dotex:**
- Zero compiler warnings
- Nullable reference types enabled
- Full XML documentation
- Code analysis enabled
- Consistent formatting

---

#### Issue #17: Replace file path parser with Microsoft.Extensions

**Status:** Open
**Created:** 2020-12-03
**Labels:** enhancement
**Author:** mwwhited (Owner)

**Description:**
Replace custom file globbing implementation with Microsoft.Extensions.FileSystemGlobbing.

**Current Implementation:**
- Custom globbing in XSLT process
- Custom globbing in ToolKit

**Recommendation:** ⭐⭐ **IMPLEMENT - Use Standard Library**

**Benefits:**
- Reduce maintenance burden
- Use well-tested Microsoft library
- Better performance (likely)
- Standard patterns

**Migration Action:**
- [ ] Evaluate Microsoft.Extensions.FileSystemGlobbing
- [ ] Compare performance with custom implementation
- [ ] Replace custom globbing in ToolKit
- [ ] Replace custom globbing in XSLT CLI
- [ ] Add unit tests
- [ ] Close issue #17

**Package:** `Microsoft.Extensions.FileSystemGlobbing`

**Effort:** Low-Medium (2-3 days)

---

#### Issue #27: Move templates to use embedded resources

**Status:** Open
**Created:** 2021-08-08
**Author:** mwwhited (Owner)

**Description:**
- Use embedded resources as default fallback for templates
- Support file type-specific embedded resources
- Use naming conventions or .NET attributes
- Support multiple embedded resources per file type

**Current dotex State:**
- File-based templates (OoBDev.System)
- Database-backed templates (OoBDev.TextTemplating - Incoming)
- No embedded resource fallback

**Recommendation:** ⭐⭐ **IMPLEMENT - Good Pattern**

**Benefits:**
- Templates ship with library
- No external file dependencies
- Version-controlled templates
- Easier distribution

**Implementation:**
1. Add embedded resources to projects
2. Create `EmbeddedResourceTemplateSource : ITemplateSource`
3. Fallback order: Command-line → Database → Embedded
4. Use attributes for file type mapping

**Migration Action:**
- [ ] Implement EmbeddedResourceTemplateSource
- [ ] Add default templates as embedded resources
- [ ] Update template resolution logic
- [ ] Add examples
- [ ] Close issue #27

**Effort:** Low (1-2 days)

---

#### Issue #14: Convert Archive Tar to ReadOnlySequence<>

**Status:** Open
**Created:** 2020-12-03
**Author:** mwwhited (Owner)

**Description:**
Modernize TAR archive to use `ReadOnlySequence<>` and `ReadOnlySpan<>` instead of `byte[]`.

**Recommendation:** ⭐⭐ **IMPLEMENT - Performance Improvement**

**Benefits:**
- Reduced memory allocations
- Better performance for large files
- Modern C# span-based APIs
- Follows .NET best practices

**Migration Action:**
- [ ] Refactor TAR implementation during Archives migration (Phase 2)
- [ ] Use `ReadOnlySequence<byte>` for streaming
- [ ] Use `ReadOnlySpan<byte>` for parsing
- [ ] Benchmark performance improvements
- [ ] Close issue #14

**Effort:** Medium (2-3 days)

---

#### Issue #22: Add MIDI library support

**Status:** Open
**Created:** 2020-12-04
**Labels:** enhancement
**Author:** mwwhited (Owner)

**Description:**
Implement MIDI support for file format and serial protocol.

**References:**
- Standard MIDI File Format: http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html
- SparkFun MIDI: https://www.sparkfun.com/categories/218
- Wikipedia: https://en.wikipedia.org/wiki/MIDI

**Use Cases:**
- Music software development
- Audio processing applications
- MIDI device communication
- Music notation tools
- DAW integration

**Recommendation:** ⭐⭐ **EVALUATE - Niche but Valuable**

**Existing .NET Libraries:**
- NAudio (mature, widely used)
- Melanchall DryWetMIDI (modern, comprehensive)

**Migration Action:**
- [ ] Evaluate existing .NET MIDI libraries
- [ ] Decide: Implement custom vs integrate existing
- [ ] If implementing, add to dotex
- [ ] Phase 4 - Selective based on need
- [ ] Close issue #22

**Effort:** High (1-2 weeks for custom implementation)

---

#### Issue #9: Refactor ApplesoftBasic Detokenizer

**Status:** Open
**Created:** 2020-12-03
**Author:** mwwhited (Owner)

**Description:**
Modernize Applesode Basic detokenizer to use `ReadOnlySequence<>` or `ReadOnlyMemory<>` instead of `IEnumerable<byte>`.

**Recommendation:** ⭐⭐ **IMPLEMENT - Performance Improvement**

**Benefits:**
- Reduced allocations
- Better performance
- Modern C# patterns

**Migration Action:**
- [ ] Apply during Apple2 migration (Phase 4)
- [ ] Refactor to use ReadOnlySpan/ReadOnlyMemory
- [ ] Benchmark performance
- [ ] Close issue #9

**Effort:** Low (1-2 days)

---

#### Issue #7: Create TestUtility with CSV data

**Status:** Open
**Created:** 2020-12-03
**Author:** mwwhited (Owner)

**Description:**
Create reusable test utility for CSV-based test data with configurable object array support.

**Code Example Provided:**
```csharp
[DataTestMethod]
[DynamicData(nameof(GetJsonTests), DynamicDataSourceType.Method)]
public void MethodTest(string input) { /* ... */ }

private static IEnumerable<object[]> GetJsonTests()
{
    // TODO: make this an object array or configurable
    using var reader = new StreamReader(/* CSV */);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    foreach (var record in csv.GetRecords<dynamic>())
        yield return new object[] { /* data */ };
}
```

**Recommendation:** ⭐⭐ **IMPLEMENT - Useful Test Utility**

**Benefits:**
- Data-driven test support
- CSV-based test data management
- Reusable across projects

**Migration Action:**
- [ ] Create `CsvDataTestAttribute` or helper class
- [ ] Add to BinaryDataDecoders.TestUtilities
- [ ] Integrate into dotex test infrastructure
- [ ] Add examples
- [ ] Close issue #7

**Effort:** Low (1-2 days)

**Destination:**
- `/current/src/dotex/Incomming/BinaryDecoders/src/BinaryDataDecoders.TestUtilities/`

---

### 🟢 LOW PRIORITY / SPECIALIZED

These issues are either very specialized or can be deferred to Phase 4.

---

#### Issue #28: EcoWitt support

**Status:** Open
**Created:** 2021-08-08
**Author:** mwwhited (Owner)

**Description:**
Implement decoder for EcoWitt weather station data.

**URL:** http://www.ecowitt.com

**Recommendation:** ⭐ **LOW PRIORITY - Very Specialized**

**Use Case:** Weather station data decoding

**Migration Action:**
- [ ] Evaluate demand for weather station support
- [ ] Phase 4 or skip
- [ ] Only implement if strategic to dotex
- [ ] Close issue #28 or defer

---

#### Issue #25: Create tokenizer/detokenizer for BasicStamp

**Status:** Open
**Created:** 2020-12-06
**Author:** mwwhited (Owner)

**Description:**
Create tools to tokenize/detokenize code for Basic Stamp microcontroller platform.

**References:**
- http://www.robotics.mcmanis.com/robots/stamps/decoding.html
- https://www.parallax.com/product/basic-stamp-2-microcontroller-module/

**Recommendation:** ⭐ **LOW PRIORITY - Embedded Niche**

**Migration Action:**
- [ ] Skip unless embedded microcontroller support is strategic
- [ ] Very specialized hardware
- [ ] Close issue #25 or defer indefinitely

---

#### Issue #21: Create GitBook documentation

**Status:** Open
**Created:** 2020-12-04
**Labels:** documentation, enhancement
**Author:** mwwhited (Owner)

**Description:**
Investigate GitBook integration for documentation.

**URL:** https://www.gitbook.com/

**Recommendation:** ⭐ **EVALUATE for dotex Documentation**

**dotex Current State:**
- Markdown documentation in `/docs`
- Auto-generated API docs
- README files

**GitBook Benefits:**
- Professional documentation site
- Search functionality
- Version control integration
- Nice UI/UX

**Migration Action:**
- [ ] Evaluate GitBook for dotex documentation
- [ ] Compare with alternatives (Docusaurus, MkDocs, etc.)
- [ ] Phase 4 - Documentation infrastructure
- [ ] Close issue #21 with decision

---

#### Issue #20: Add shield.io + nuget badges

**Status:** Open
**Created:** 2020-12-03
**Labels:** documentation
**Author:** mwwhited (Owner)

**Description:**
Add badges to assembly doc generation with per-assembly summaries linking to related files.

**Recommendation:** ⭐ **LOW PRIORITY - Documentation Enhancement**

**Migration Action:**
- [ ] Consider for dotex README files
- [ ] Add shield.io badges for:
  - Build status
  - NuGet versions
  - Code coverage
  - License
- [ ] Phase 4
- [ ] Close issue #20

---

#### Issue #19: Work on publish target for rptproj

**Status:** Open
**Created:** 2020-12-03
**Labels:** documentation, enhancement
**Author:** mwwhited (Owner)

**Description:**
Develop custom SDK project type for reports or publish profile configuration.

**Recommendation:** ⭐ **SKIP - Not Applicable to dotex**

**Migration Action:**
- [ ] Skip - Very specialized reporting scenario
- [ ] Not relevant to dotex
- [ ] Close issue #19 as not applicable

---

#### Issue #12: Update Apple2Encoding bidirectionality

**Status:** Open
**Created:** 2020-12-03
**Author:** mwwhited (Owner)

**Description:**
Fix Apple 2 encoding to properly round-trip String ↔ Bytes with upper bit preservation.

**Current Problem:**
- Decode: Strips upper bit to convert Apple bytes to ASCII ✓
- Encode: Does NOT restore upper bit ✗

**Recommendation:** ⭐ **FIX During Apple2 Migration**

**Migration Action:**
- [ ] Fix bidirectional encoding during Phase 4
- [ ] Add round-trip tests
- [ ] Close issue #12

**Effort:** Low (few hours)

---

#### Issue #11: Add language parser for ApplesoftBASIC

**Status:** Open
**Created:** 2020-12-03
**Author:** mwwhited (Owner)

**Description:**
Create ANTLR4 parser and AST for AppleSoft Basic with tokenization capabilities.

**Recommendation:** ⭐ **EVALUATE - Interesting but Niche**

**Value:** Educational, retro computing, emulation

**Migration Action:**
- [ ] Phase 4 - Retro computing support
- [ ] Only if there's strategic value
- [ ] Consider as example of ANTLR usage
- [ ] Close issue #11 or defer

**Effort:** High (1-2 weeks for grammar + parser + tests)

---

#### Issue #10: Add ability to store data back to Dos33 format

**Status:** Open
**Created:** 2020-12-03
**Author:** mwwhited (Owner)

**Description:**
Extend DOS33 library to support write operations (currently read-only).

**Recommendation:** ⭐ **IMPLEMENT if Supporting Apple II**

**Migration Action:**
- [ ] Phase 4 - Complete DOS33 bidirectionality
- [ ] Add write support for catalog and files
- [ ] Add tests with round-trip verification
- [ ] Close issue #10

**Effort:** Medium (3-5 days)

---

## Closed Issues Reference

### Recent Closed Issues (2023-2024)

| # | Title | Closed Date | Notes |
|---|-------|-------------|-------|
| 53 | Dev/upgrade to net90v2 | 2025-02-18 | .NET 9.0 upgrade |
| 52 | Bump System.Text.Json 8.0.3→8.0.5 | 2024-12-30 | Dependency update |
| 51 | Update dotnet-core.yml | 2024-12-30 | CI/CD update |
| 50 | Bump System.Text.Json 8.0.3→8.0.4 | 2024-12-30 | Dependency update |
| 49 | Dev/updates | 2024-05-22 | General updates |
| 48 | Create dependabot.yml | 2024-02-01 | Automated dependency management |
| 47 | Dev/update projects | 2024-01-07 | Project updates |
| 46 | Dev/port in code | 2024-01-07 | Code porting |

### Historical Closed Issues (2020-2022)

**Total:** 35 closed issues/PRs

**Categories:**
- Framework upgrades (.NET 5.0 → 6.0 → 7.0 → 9.0)
- Build pipeline improvements
- Dependency updates
- Code quality improvements
- Bug fixes

**Note:** See GitHub for full history: https://github.com/mwwhited/BinaryDataDecoders/issues?q=is%3Aissue+is%3Aclosed

---

## Migration Recommendations

### Immediate Actions (Week 1-2)

1. **Close as Duplicate (Already in dotex):**
   - [ ] #42 - Handlebars template (OoBDev.Handlebars exists)
   - [ ] #41 - HTML binding template (OoBDev.TextTemplating exists)

2. **Implement High-Value Features:**
   - [ ] #43 - PlantUML support (Add to OoBDev.Markdig)
   - [ ] #30 - Fix all warnings during migration

3. **Plan Architecture Improvements:**
   - [ ] #16 - Segmenter interface refactoring
   - [ ] #17 - Replace custom globbing with Microsoft.Extensions

### Phase 1: Foundation (Week 1-2)

**Focus:** ToolKit migration + bug fixes

- [ ] Apply issue #30 - Zero warnings
- [ ] Apply issue #17 - Microsoft.Extensions.FileSystemGlobbing
- [ ] Document ToolKit features from issues

### Phase 2: Core Extensions (Week 3-4)

**Focus:** CodeAnalysis, ExpressionCalculator, Archives, Drawing

- [ ] Implement issue #15 - XPathNavigator for CodeAnalysis
- [ ] Implement issue #13 - ZIP format (evaluate vs System.IO.Compression)
- [ ] Implement issue #14 - TAR with ReadOnlySequence
- [ ] Implement issue #7 - CSV test data utility

### Phase 3: Network & Protocols (Week 5-6)

**Focus:** Net utilities, protocols

- [ ] Evaluate issue #22 - MIDI support (selective)
- [ ] Merge Net utilities with OoBDev.Communications

### Phase 4: Specialized (Week 7+)

**Focus:** Selective integration

- [ ] Implement issue #18 - FAT filesystem support (HIGH VALUE)
- [ ] Evaluate issue #27 - Embedded resource templates
- [ ] Apple II issues (#9, #10, #11, #12) - If strategic
- [ ] Other specialized issues (#25, #28) - Only if needed

### Phase 5: Documentation & Polish

**Focus:** Complete documentation

- [ ] Evaluate issue #21 - GitBook documentation
- [ ] Implement issue #20 - Badges and assembly docs
- [ ] Close all resolved issues

---

## Priority Summary

### ⭐⭐⭐ CRITICAL (Must Do)

1. #43 - PlantUML markdown support
2. #16 - Segmenter interface refactoring
3. #15 - CodeAnalysis XPathNavigator
4. #18 - FAT filesystem support
5. #13 - ZIP format evaluation

**Total:** 5 issues

### ⭐⭐ HIGH (Should Do)

1. #30 - Clean up warnings
2. #17 - Microsoft.Extensions.FileSystemGlobbing
3. #27 - Embedded resource templates
4. #14 - TAR ReadOnlySequence modernization
5. #22 - MIDI support (evaluate)
6. #9 - ApplesoftBasic modernization
7. #7 - CSV test utility

**Total:** 7 issues

### ⭐ MEDIUM-LOW (Consider)

1. #28 - EcoWitt support
2. #25 - BasicStamp tokenizer
3. #21 - GitBook documentation
4. #20 - Badges
5. #19 - Report publishing (skip)
6. #12 - Apple2Encoding fix
7. #11 - ApplesoftBASIC parser
8. #10 - DOS33 write support

**Total:** 8 issues

### ✅ CLOSE (Already Implemented in dotex)

1. #42 - Handlebars template
2. #41 - HTML binding template

**Total:** 2 issues

---

## Tracking Status

| Status | Count | Percentage |
|--------|-------|------------|
| ⭐⭐⭐ Critical | 5 | 27.8% |
| ⭐⭐ High | 7 | 38.9% |
| ⭐ Medium-Low | 8 | 44.4% |
| ✅ Close as Duplicate | 2 | 11.1% |
| **Total Open Issues** | **18** | **100%** |

---

## Next Steps

1. **Review this document** with stakeholders
2. **Close duplicates** (#42, #41)
3. **Prioritize critical issues** for immediate implementation
4. **Track progress** during migration phases
5. **Update this document** as issues are resolved
6. **Cross-reference** with COMPARISON_REPORT.md for migration planning

---

**Document Version:** 1.0
**Last Updated:** 2026-01-11
**Source:** https://github.com/mwwhited/BinaryDataDecoders/issues
**Maintainer:** dotex Migration Team

---

*End of Document*
