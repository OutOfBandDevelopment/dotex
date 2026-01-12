# BinaryDataDecoders Migration Plan

**Version:** 1.0
**Last Updated:** 2026-01-12
**Source Repository:** BinaryDataDecoders (Incomming/BinaryDecoders)
**Target Repository:** OoBDev (dotex) Framework

---

## Overview

This document provides a detailed, actionable migration plan for integrating BinaryDataDecoders features into the OoBDev framework. The plan is organized by migration phases, with each feature area broken down into specific tasks that follow OoBDev architectural standards.

**Reference:** See [binarydatadecoders-feature-mapping.md](./binarydatadecoders-feature-mapping.md) for comprehensive feature comparison.

---

## Migration Principles

All migration work MUST follow these principles from `/current/src/docs/architecture`:

1. **Layered Architecture** - Place projects in correct layer (Common, Framework, Extensions, ExternalServices)
2. **Provider/Factory Pattern** - Use for all integrations with multiple implementations
3. **Dependency Injection** - TryAdd* extensions, builder pattern, IOptions<T>
4. **Type Safety** - Generic constraints, nullable enabled, strongly-typed
5. **Testing** - 80% coverage minimum, MSTest, categorized tests
6. **Documentation** - README required, XML docs on public APIs, usage examples
7. **No Breaking Changes** - Maintain backward compatibility with existing OoBDev APIs

---

## Migration Scope

**ALL features from BinaryDataDecoders will be migrated.**

- Phases indicate **priority order**, not feature selection
- Even highly specialized and niche features will be maintained
- Incomplete features will be migrated and tracked in TODO.md for future completion
- No features will be skipped or deleted unless:
  - They are UWP/Framework-specific with no .NET 9.0 equivalent
  - They are completely obsolete (e.g., Silverlight-specific code)

**Incomplete Features:** If a feature in BinaryDataDecoders is partially implemented or has TODOs:
1. Migrate the existing implementation
2. Add comprehensive TODO items in `/current/src/TODO.md`
3. Document what's missing in the project README
4. Mark incomplete areas with `// TODO:` comments in code

---

## Phase 0: Critical Bug Fixes (IMMEDIATE)

**Priority:** CRITICAL
**Dependencies:** None
**Impact:** Fixes broken functionality in current OoBDev

### Task 0.1: Fix PathEx Lambda Bug

**Status:** BUG - CRITICAL
**File:** `/current/src/src/Framework/OoBDev.System/IO/PathEx.cs`
**Issue:** Wildcard path matching completely broken

**Current Code:**
```csharp
wildCards.Any(ps.Contains)
```

**Fixed Code:**
```csharp
wildCards.Any(c => ps.Contains(c))
```

**Steps:**
1. Read `/current/src/src/Framework/OoBDev.System/IO/PathEx.cs`
2. Locate line containing `wildCards.Any(ps.Contains)`
3. Replace with `wildCards.Any(c => ps.Contains(c))`
4. Add unit test for wildcard matching
5. Verify all existing tests pass

**Validation:**
- [ ] Code compiles without warnings
- [ ] Wildcard tests pass
- [ ] Existing PathEx tests still pass
- [ ] Added test for wildcard functionality

**Reference:** BinaryDataDecoders.ToolKit/IO/PathEx.cs:115

---

### Task 0.2: Fix StreamDevice Nullable Annotations

**Status:** BUG - MEDIUM
**File:** `/current/src/src/Framework/OoBDev.System.IO.Pipelines/StreamDevice.cs`
**Issue:** Missing nullable annotations on Device property

**Current Code (lines 19-21):**
```csharp
public IDeviceAdapter Device => _device;
```

**Fixed Code:**
```csharp
public IDeviceAdapter? Device => _device;
```

**Steps:**
1. Read `/current/src/src/Framework/OoBDev.System.IO.Pipelines/StreamDevice.cs`
2. Review all properties and fields for nullable correctness
3. Add `?` to nullable types
4. Verify no null reference warnings

**Validation:**
- [ ] No CS8600-8629 warnings (nullable)
- [ ] Build succeeds with nullable warnings as errors
- [ ] Tests pass

---

### Task 0.3: Fix StreamDevice Event Name Typo

**Status:** BUG - MEDIUM
**File:** `/current/src/src/Framework/OoBDev.System.IO.Pipelines/StreamDevice.cs`
**Issue:** Event named "MessageTrasmitterError" instead of "MessageTransmitterError"

**Current Code:**
```csharp
public event EventHandler<ErrorEventArgs>? MessageTrasmitterError;
```

**Fixed Code:**
```csharp
public event EventHandler<ErrorEventArgs>? MessageTransmitterError;
```

**Steps:**
1. Search for all occurrences of "MessageTrasmitterError"
2. Replace with "MessageTransmitterError"
3. Update any event subscribers
4. Check for breaking change impact

**Validation:**
- [ ] All references updated
- [ ] Tests updated if needed
- [ ] No breaking changes to public API (consider adding [Obsolete] redirect)

**Breaking Change Mitigation:**
```csharp
[Obsolete("Use MessageTransmitterError instead. This will be removed in v3.0.")]
public event EventHandler<ErrorEventArgs>? MessageTrasmitterError
{
    add => MessageTransmitterError += value;
    remove => MessageTransmitterError -= value;
}

public event EventHandler<ErrorEventArgs>? MessageTransmitterError;
```

---

### Task 0.4: Fix SerialPortFactory Verbose Ternary

**Status:** BUG - LOW (code style)
**File:** `/current/src/src/Framework/OoBDev.System.IO.Ports/SerialPortFactory.cs`
**Issue:** Verbose ternary expression can be simplified

**Steps:**
1. Read file and identify verbose ternary
2. Simplify expression
3. Verify tests pass

**Validation:**
- [ ] Code simplified
- [ ] Behavior unchanged
- [ ] Tests pass

---

### Task 0.5: Replace ShiftCommutativeVariablesRight Stub

**Status:** BUG - CRITICAL
**File:** `/current/src/src/Framework/OoBDev.ExpressionCalculator/Optimizers/ShiftCommutativeVariablesRight.cs`
**Issue:** Non-functional stub that breaks expression normalization

**Current Code (BROKEN):**
```csharp
public class ShiftCommutativeVariablesRight<T> : IExpressionOptimizer<T>
    where T : struct, INumber<T>
{
    public IExpression<T> Optimize(IExpression<T> expression)
    {
        return expression; // DOES NOTHING!!!
    }
}
```

**Steps:**
1. Read working implementation from BinaryDataDecoders:
   - `/current/src/Incomming/BinaryDecoders/src/BinaryDataDecoders.ExpressionCalculator/Optimizers/ShiftCommutativeVariablesRight.cs`
2. Copy working implementation to OoBDev
3. Adjust namespaces to OoBDev conventions
4. Add comprehensive unit tests
5. Verify expression normalization works

**Test Cases:**
```csharp
[TestMethod]
public void Optimize_VariablePlusConstant_ShiftsToConstantPlusVariable()
{
    // a + 2 should become 2 + a
    var expression = new AddExpression<int>(
        new VariableExpression<int>("a"),
        new NumberExpression<int>(2)
    );
    var optimized = optimizer.Optimize(expression);
    // Assert: left is NumberExpression, right is VariableExpression
}

[TestMethod]
public void Optimize_ConstantPlusVariable_NoChange()
{
    // 2 + a should stay 2 + a
    var expression = new AddExpression<int>(
        new NumberExpression<int>(2),
        new VariableExpression<int>("a")
    );
    var optimized = optimizer.Optimize(expression);
    // Assert: no change
}
```

**Validation:**
- [ ] Implementation matches BinaryDataDecoders logic
- [ ] Expression tree recursion works correctly
- [ ] All test cases pass
- [ ] Integration tests with full optimizer pipeline pass

**Impact:** CRITICAL - Without this, expressions like `a + 2` and `2 + a` are not recognized as equivalent.

---

### Phase 0 Completion Criteria

- [ ] All 5 bugs fixed
- [ ] All OoBDev tests pass (existing + new)
- [ ] Build succeeds with no warnings
- [ ] Expression calculator tests comprehensive
- [ ] Documentation updated for any API changes

**Estimated Effort:** Small (bug fixes only)
**Blocking:** All subsequent phases (Phase 1-5)

---

## Phase 1: Foundation Enhancement

**Priority:** HIGH
**Dependencies:** Phase 0 complete
**Goal:** Enhance OoBDev.System with missing utilities from BinaryDataDecoders.ToolKit

### Task 1.1: Migrate Endianness Types

**Status:** NEW
**Target:** `OoBDev.System`
**Source:** `BinaryDataDecoders.ToolKit`

**Current State:**
- OoBDev has: `BigEndianUShort`
- Missing: `BigEndianInt16`, `BigEndianInt32`, `BigEndianUInt32`, `BigEndianInt64`, `BigEndianUInt64`, `LittleEndian*` variants

**Steps:**
1. Create file: `src/Framework/OoBDev.System/BigEndianInt32.cs`
2. Implement struct with:
   - Implicit conversions to/from int
   - Explicit byte[] conversion
   - ToString, Equals, GetHashCode
3. Repeat for: Int16, UInt32, Int64, UInt64
4. Add LittleEndian variants if needed
5. Add comprehensive tests
6. Update documentation

**Pattern to Follow:**
```csharp
public readonly struct BigEndianInt32 : IEquatable<BigEndianInt32>, IComparable<BigEndianInt32>
{
    private readonly int _value;

    public BigEndianInt32(int value) => _value = value;

    public static implicit operator int(BigEndianInt32 value) => value._value;
    public static implicit operator BigEndianInt32(int value) => new(value);

    public byte[] ToBytes() => BitConverter.GetBytes(_value).Reverse().ToArray();
    public static BigEndianInt32 FromBytes(byte[] bytes) => BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);

    // IEquatable, IComparable, object overrides
}
```

**Validation:**
- [ ] All numeric types covered (int16, int32, uint32, int64, uint64)
- [ ] Implicit conversions work
- [ ] Byte conversion correct (test with known values)
- [ ] Equals/GetHashCode/CompareTo implemented
- [ ] 90%+ test coverage
- [ ] XML documentation complete

---

### Task 1.2: Migrate FormattableNumber<T>

**Status:** NEW
**Target:** `OoBDev.System`
**Source:** `BinaryDataDecoders.ToolKit/FormattableNumber.cs`

**Feature:**
- Engineering notation (1.5k, 2.3M, 4.5G)
- Scientific notation (1.5e3, 2.3e6)
- Byte notation (1.5 KB, 2.3 MB, 4.5 GB)
- Customizable formatting

**Steps:**
1. Create `src/Framework/OoBDev.System/Formatting/FormattableNumber.cs`
2. Copy implementation from BinaryDataDecoders
3. Adjust to OoBDev patterns
4. Add IFormattable implementation
5. Add comprehensive tests
6. Add XML documentation

**Test Cases:**
```csharp
[TestMethod]
public void Format_EngineeringNotation_Thousands()
{
    var num = new FormattableNumber<double>(1500);
    Assert.AreEqual("1.5k", num.ToString("E"));
}

[TestMethod]
public void Format_ByteNotation_Megabytes()
{
    var num = new FormattableNumber<long>(1572864);
    Assert.AreEqual("1.5 MB", num.ToString("B"));
}
```

**Validation:**
- [ ] Engineering notation works (k, M, G, T)
- [ ] Scientific notation works
- [ ] Byte notation works (KB, MB, GB, TB)
- [ ] Custom format strings supported
- [ ] Generic for all numeric types
- [ ] Tests cover all notation types

---

### Task 1.3: Migrate BCD (Binary-Coded Decimal)

**Status:** NEW
**Target:** `OoBDev.System`
**Source:** `BinaryDataDecoders.ToolKit/BcdEx.cs`

**Feature:**
- BCD encoding/decoding
- Support for packed BCD
- Validation

**Steps:**
1. Create `src/Framework/OoBDev.System/Binary/BcdConverter.cs`
2. Implement encoding/decoding methods
3. Add validation
4. Add tests
5. Add XML documentation

**API Design:**
```csharp
public static class BcdConverter
{
    public static byte[] Encode(int value);
    public static int Decode(byte[] bcd);
    public static bool TryDecode(byte[] bcd, out int value);
    public static bool IsValidBcd(byte[] bcd);
}
```

**Validation:**
- [ ] Encoding correct (test with known BCD values)
- [ ] Decoding correct
- [ ] TryDecode returns false for invalid BCD
- [ ] Validation detects invalid digits
- [ ] Tests comprehensive

---

### Task 1.4: Migrate DoubleLinkedList<T>

**Status:** NEW
**Target:** `OoBDev.System`
**Source:** `BinaryDataDecoders.ToolKit/Collections/DoubleLinkedList.cs`

**Feature:**
- Bidirectional linked list
- Forward and backward navigation
- IEnumerable support

**Steps:**
1. Create `src/Framework/OoBDev.System/Collections/DoubleLinkedList.cs`
2. Implement ICollection<T>, IEnumerable<T>
3. Add bidirectional node navigation
4. Add comprehensive tests
5. Add XML documentation

**Validation:**
- [ ] Add/Remove/Clear operations work
- [ ] Forward enumeration works
- [ ] Backward enumeration works
- [ ] Count property accurate
- [ ] Thread-safety considered (document if not thread-safe)
- [ ] 90%+ test coverage

---

### Task 1.5: Migrate ObservableDictionary<K, V>

**Status:** NEW
**Target:** `OoBDev.System`
**Source:** `BinaryDataDecoders.ToolKit/Collections/ObservableDictionary.cs`

**Feature:**
- Dictionary with INotifyPropertyChanged
- INotifyCollectionChanged support
- MVVM-friendly

**Migration Plan:**
- OoBDev supports desktop and server scenarios
- MVVM components useful for WPF, Windows Forms, and Blazor UI
- **MIGRATE** to `OoBDev.Extensions.UI.Collections` (Phase 4/5)
- Package separately for UI scenarios

**Validation:**
- [ ] INotifyPropertyChanged works
- [ ] INotifyCollectionChanged events fire
- [ ] All dictionary operations work
- [ ] Tests cover event notifications
- [ ] Works with WPF, Windows Forms, and Blazor data binding

**Files to Create:**
- `src/Extensions/OoBDev.Extensions.UI.Collections/ObservableDictionary.cs`
- `src/Extensions/OoBDev.Extensions.UI.Collections.Tests/`

---

### Task 1.6: Migrate Threading Utilities

**Status:** NEW
**Target:** `OoBDev.System`
**Source:** `BinaryDataDecoders.ToolKit/Threading/`

**Features:**
- TaskEx.WhenAllOrException - Aggregate exceptions
- ParallelQueryEx - PLINQ helpers

**Steps:**
1. Create `src/Framework/OoBDev.System/Threading/TaskExtensions.cs`
2. Implement WhenAllOrException
3. Add ParallelQuery extensions
4. Add tests (including exception scenarios)
5. Add XML documentation

**API Design:**
```csharp
public static class TaskExtensions
{
    public static async Task WhenAllOrException(params Task[] tasks);
    public static async Task<T[]> WhenAllOrException<T>(params Task<T>[] tasks);
}
```

**Validation:**
- [ ] WhenAllOrException aggregates all exceptions
- [ ] Successful tasks complete
- [ ] Exception handling correct
- [ ] Tests cover success and failure cases

---

### Phase 1 Completion Criteria

- [ ] All endianness types implemented and tested
- [ ] FormattableNumber working with all notations
- [ ] BCD conversion validated
- [ ] Collections (DoubleLinkedList) tested
- [ ] Threading utilities comprehensive
- [ ] All tests pass (90%+ coverage)
- [ ] XML documentation complete
- [ ] README updated

**Estimated Effort:** Medium
**Blocking:** None (Phase 0 complete)

---

## Phase 2: High-Value Features

**Priority:** HIGH
**Dependencies:** Phase 0 complete
**Goal:** Add CodeAnalysis, complete ExpressionCalculator, add Archives support

### Task 2.1: Migrate CodeAnalysis Foundation

**Status:** NEW
**Target:** `OoBDev.CodeAnalysis.Abstractions`
**Source:** `BinaryDataDecoders.CodeAnalysis`

**Architecture:**
```
Framework/
  OoBDev.CodeAnalysis.Abstractions/
    ICodeNavigator.cs
    ISemanticNavigator.cs
    ISyntaxNode.cs
  OoBDev.CodeAnalysis/
    (shared utilities)

ExternalServices/
  Microsoft/OoBDev.Microsoft.CodeAnalysis.CSharp/
    CSharpNavigator.cs
    CSharpSemanticNavigator.cs
  Microsoft/OoBDev.Microsoft.CodeAnalysis.VisualBasic/
    VisualBasicNavigator.cs
  Microsoft/OoBDev.Microsoft.Build.StructuredLog/
    StructuredLogNavigator.cs
```

**Steps:**

1. **Create Abstractions Project**
   ```bash
   mkdir -p src/Framework/OoBDev.CodeAnalysis.Abstractions
   ```

2. **Define Core Interfaces**
   ```csharp
   // ICodeNavigator.cs
   public interface ICodeNavigator : IXPathNavigable
   {
       XPathNavigator CreateNavigator();
       IEnumerable<ISyntaxNode> Query(string xpath);
   }

   // ISyntaxNode.cs
   public interface ISyntaxNode
   {
       string Kind { get; }
       string Name { get; }
       TextSpan Span { get; }
       IEnumerable<ISyntaxNode> Children { get; }
   }
   ```

3. **Create Provider Interfaces**
   ```csharp
   public interface ICodeNavigatorProvider
   {
       ICodeNavigator GetNavigator(string language);
   }

   public interface ICodeNavigatorProviderFactory
   {
       ICodeNavigatorProvider Create(string providerKey);
   }
   ```

4. **Create C# Navigator Implementation**
   - Copy from BinaryDataDecoders.CodeAnalysis
   - Adjust to OoBDev patterns
   - Implement ICodeNavigator

5. **Add Tests**
   - Test XPath queries
   - Test semantic navigation
   - Test symbol lookup

6. **Create Extension Methods**
   ```csharp
   public static class ServiceCollectionExtensions
   {
       public static IServiceCollection TryAddCodeAnalysis(
           this IServiceCollection services,
           IConfiguration configuration,
           Action<CodeAnalysisExtensionBuilder>? configure = null)
       {
           services.TryAddSingleton<ICodeNavigatorFactory, CodeNavigatorFactory>();

           var builder = new CodeAnalysisExtensionBuilder(services, configuration);
           configure?.Invoke(builder);

           return services;
       }
   }
   ```

**Validation:**
- [ ] Abstractions project created
- [ ] Provider/factory pattern implemented
- [ ] C# navigator works
- [ ] XPath queries return correct results
- [ ] DI registration follows TryAdd* pattern
- [ ] Tests 80%+ coverage
- [ ] README created
- [ ] XML documentation complete

---

### Task 2.2: Complete ExpressionCalculator Migration

**Status:** UPDATE
**Target:** `OoBDev.ExpressionCalculator`
**Source:** `BinaryDataDecoders.ExpressionCalculator`

**Already Done in Phase 0:**
- ShiftCommutativeVariablesRight fixed

**Additional Tasks:**

1. **Verify All Optimizers Work**
   - InnerExpressionReducer
   - UnaryNumericExpressionReducer
   - IdentityExpressionOptimizer
   - DeterminedExpressionReducer

2. **Add Missing Tests from BinaryDataDecoders**
   - Copy comprehensive test suite
   - Verify all test cases pass
   - Add tests for edge cases

3. **Verify ANTLR Grammar Match**
   - Compare ExpressionTree.g4 files
   - Ensure operators match
   - Test precedence rules

4. **Add Optimizer Pipeline Tests**
   ```csharp
   [TestMethod]
   public void OptimizerPipeline_ComplexExpression_FullyOptimized()
   {
       // Input: ((a + 0) * 1) + (2 + b)
       // Expected: a + (2 + b)
       // After shift: a + (b + 2)  (if shift is last)
   }
   ```

**Validation:**
- [ ] All optimizers verified working
- [ ] Test suite comprehensive (from BDD)
- [ ] All tests pass
- [ ] Expression normalization works
- [ ] Coverage 90%+

---

### Task 2.3: Migrate Archives Support

**Status:** UPDATE
**Target:** `OoBDev.Archives`
**Source:** `BinaryDataDecoders.Archives`

**Current OoBDev State:**
- Basic ZIP structures in OoBDev.System/Archives/
- TAR header only

**Steps:**

1. **Create Archives Framework Project**
   ```bash
   mkdir -p src/Framework/OoBDev.Archives.Abstractions
   mkdir -p src/Framework/OoBDev.Archives
   ```

2. **Define Archive Abstractions**
   ```csharp
   public interface IArchiveReader<T> where T : class
   {
       IEnumerable<IArchiveEntry> Entries { get; }
       Stream OpenEntry(string entryName);
       bool TryOpenEntry(string entryName, out Stream? stream);
   }

   public interface IArchiveWriter<T> where T : class
   {
       void AddEntry(string entryName, Stream content);
       void Save(Stream output);
   }

   public interface IArchiveEntry
   {
       string Name { get; }
       long Size { get; }
       DateTimeOffset LastModified { get; }
   }
   ```

3. **Implement TAR Support**
   - Copy TAR implementation from BinaryDataDecoders
   - Adapt to OoBDev abstractions
   - Add tests for read/write

4. **Enhance ZIP Support**
   - Merge BinaryDataDecoders improvements
   - Keep compatibility with System.IO.Compression
   - Add tests

5. **Create Provider Pattern**
   ```csharp
   public interface IArchiveProvider
   {
       IArchiveReader<T> GetReader<T>(Stream stream) where T : class;
       IArchiveWriter<T> GetWriter<T>(Stream stream) where T : class;
   }
   ```

6. **Add Extension Methods**
   ```csharp
   public static IServiceCollection TryAddArchives(
       this IServiceCollection services,
       IConfiguration configuration)
   {
       services.TryAddSingleton<IArchiveProvider, TarArchiveProvider>();
       services.TryAddSingleton<IArchiveProvider, ZipArchiveProvider>();
       services.TryAddSingleton<IArchiveProviderFactory, ArchiveProviderFactory>();
       return services;
   }
   ```

**Validation:**
- [ ] TAR read/write works
- [ ] ZIP read/write works
- [ ] Provider pattern implemented
- [ ] Tests comprehensive (various archive types)
- [ ] Compatible with existing OoBDev.System structures
- [ ] Documentation complete

---

### Phase 2 Completion Criteria

- [ ] CodeAnalysis framework operational
- [ ] C# and VB navigators work
- [ ] XPath queries functional
- [ ] ExpressionCalculator complete (all optimizers working)
- [ ] Archives support TAR and ZIP
- [ ] All tests pass (80%+ coverage)
- [ ] Provider/factory pattern implemented
- [ ] DI registration via TryAdd* methods
- [ ] README files created for all new projects
- [ ] XML documentation complete

**Estimated Effort:** Large
**Blocking:** None (Phase 0 complete)

---

## Phase 3: Protocols & Graphics

**Priority:** MEDIUM
**Dependencies:** Phase 0 complete
**Goal:** Add NMEA protocol and Drawing capabilities

### Task 3.1: Migrate NMEA Protocol Decoder

**Status:** NEW
**Target:** `OoBDev.Protocols.Nmea`
**Source:** `BinaryDataDecoders.Nmea`

**Architecture:**
```
Framework/
  OoBDev.Protocols.Nmea.Abstractions/
    INmeaDecoder.cs
    INmeaSentence.cs
    Sentences/
      GgaSentence.cs (model)
      GsaSentence.cs (model)
  OoBDev.Protocols.Nmea/
    NmeaDecoder.cs
    NmeaChecksum.cs
    Sentences/
      GgaDecoder.cs
      GsaDecoder.cs
```

**Steps:**

1. **Create Abstractions Project**
   ```csharp
   public interface INmeaDecoder
   {
       INmeaSentence? Decode(string sentence);
       bool ValidateChecksum(string sentence);
   }

   public interface INmeaSentence
   {
       string SentenceId { get; }
       DateTime? Timestamp { get; }
   }

   public interface IGgaSentence : INmeaSentence
   {
       decimal Latitude { get; }
       decimal Longitude { get; }
       int FixQuality { get; }
       int NumberOfSatellites { get; }
       decimal Altitude { get; }
   }
   ```

2. **Implement Decoder**
   - Copy from BinaryDataDecoders.Nmea
   - Adapt to OoBDev patterns
   - Add checksum validation
   - Add sentence parsing

3. **Add Sentence Models**
   - GgaSentence (GPS Fix Data)
   - GsaSentence (DOP and Active Satellites)
   - Extensible for additional sentence types

4. **Create Message Decoder for OoBDev.System.IO**
   ```csharp
   public class NmeaMessageDecoder : IMessageDecoder<INmeaSentence>
   {
       public bool TryDecode(ReadOnlySpan<byte> buffer, out INmeaSentence? message)
       {
           // Decode NMEA sentence from byte buffer
       }
   }
   ```

5. **Add Tests**
   - Test checksum validation
   - Test sentence parsing
   - Test invalid data handling
   - Test integration with StreamDevice

6. **Add Extension Methods**
   ```csharp
   public static IServiceCollection TryAddNmeaProtocol(
       this IServiceCollection services,
       IConfiguration configuration)
   {
       services.TryAddSingleton<INmeaDecoder, NmeaDecoder>();
       services.TryAddSingleton<IMessageDecoder<INmeaSentence>, NmeaMessageDecoder>();
       return services;
   }
   ```

**Integration Example:**
```csharp
// Use NMEA with SerialPort via StreamDevice
var serialPort = new SerialPort("COM1", 4800);
var adapter = new SerialPortDeviceAdapter(serialPort);
var segmenter = new LineSegmenter(); // NMEA sentences are line-delimited
var decoder = new NmeaMessageDecoder();

var device = new StreamDevice<INmeaSentence>(adapter, segmenter, decoder);
device.MessageReceived += (sender, e) =>
{
    if (e.Message is IGgaSentence gga)
    {
        Console.WriteLine($"GPS: {gga.Latitude}, {gga.Longitude}");
    }
};
```

**Validation:**
- [ ] NMEA decoder works
- [ ] Checksum validation correct
- [ ] GGA sentences parsed correctly
- [ ] GSA sentences parsed correctly
- [ ] Integrates with OoBDev.System.IO
- [ ] Tests comprehensive
- [ ] Documentation complete

---

### Task 3.2: Migrate Drawing Features

**Status:** NEW
**Target:** `OoBDev.Extensions.Drawing`
**Source:** `BinaryDataDecoders.Drawing`

**Features to Migrate:**
- Barcode generation (Code39)
- DeepZoom tiling
- JPEG manipulation utilities
- Graphics primitives (if not duplicating System.Drawing)

**Steps:**

1. **Create Drawing Extensions Projects**
   ```bash
   mkdir -p src/Extensions/OoBDev.Extensions.Drawing.Barcodes.Abstractions
   mkdir -p src/Extensions/OoBDev.Extensions.Drawing.Barcodes
   mkdir -p src/Extensions/OoBDev.Extensions.Drawing.DeepZoom
   mkdir -p src/Extensions/OoBDev.Extensions.Drawing.Imaging
   ```

2. **Modernize Imaging Dependencies**
   - Replace System.Drawing with SkiaSharp or SixLabors.ImageSharp
   - System.Drawing.Common is deprecated for cross-platform
   - Choose modern, actively-maintained imaging library

3. **Migrate Barcode Generation**
   ```csharp
   public interface IBarcodeGenerator
   {
       byte[] Generate(string data, BarcodeOptions options);
       Stream GenerateStream(string data, BarcodeOptions options);
   }

   public class Code39BarcodeGenerator : IBarcodeGenerator
   {
       // Implementation using modern imaging library
   }
   ```

4. **Migrate DeepZoom Tile Generation (Backend)**
   - Z-order curve generation
   - Tile image generation with configurable size
   - Multi-resolution pyramid support
   - DZI metadata file generation
   - Note: This is the tile GENERATION component

5. **Create DeepZoom Viewer Controls (NEW - Future Phase)**
   - WPF viewer control (pan, zoom, smooth transitions)
   - JavaScript/TypeScript viewer library
   - Touch/gesture support
   - Progressive loading
   - See Phase 5 planning for viewer implementation

6. **Migrate JPEG Utilities**
   - EXIF data reading/writing
   - JPEG marker parsing
   - Quality adjustment utilities

7. **Add Comprehensive Tests**
   - Barcode generation and validation
   - DeepZoom tile generation
   - JPEG manipulation
   - Visual verification (save to file in tests)

8. **Document All Features**
   - Usage examples for each drawing feature
   - Performance considerations
   - Imaging library selection rationale

**Validation:**
- [ ] Modern imaging library integrated (SkiaSharp/ImageSharp)
- [ ] Code39 barcode generation working
- [ ] DeepZoom tiling operational
- [ ] JPEG utilities functional
- [ ] Cross-platform compatible (.NET 9.0)
- [ ] All tests pass
- [ ] Documentation complete with examples

---

### Phase 3 Completion Criteria

- [ ] NMEA protocol decoder operational
- [ ] Integrates with OoBDev.System.IO
- [ ] GPS sentences parsed correctly
- [ ] All Drawing features migrated
  - [ ] Barcode generation (Code39)
  - [ ] DeepZoom tiling
  - [ ] JPEG manipulation
- [ ] Modern imaging library integrated (SkiaSharp/ImageSharp)
- [ ] All tests pass
- [ ] Documentation complete

**Estimated Effort:** Medium-Large (complete migration of protocols and drawing)
**Blocking:** None (Phase 0 complete)

---

## Phase 4: Specialized Domain Features

**Priority:** LOW
**Dependencies:** Phase 0 complete
**Goal:** Migrate all specialized domain features for completeness

**Note:** ALL features will be migrated to maintain full BinaryDataDecoders functionality. Phases indicate priority order, not feature selection.

### Task 4.1: Migrate FileSystems (ISO 9660)

**Status:** NEW
**Target:** `OoBDev.Extensions.FileSystems.ISO9660`
**Source:** `BinaryDataDecoders.FileSystems`

**Use Cases:**
- CD/DVD ISO image reading
- Disk image mounting and analysis
- Legacy data recovery from optical media
- Software distribution analysis

**Steps:**

1. Create Extensions project structure
2. Migrate ISO 9660 core implementation
3. Add Rock Ridge extensions support if present
4. Add Joliet extensions support if present
5. Implement file/directory reading
6. Add comprehensive tests
7. Document API and usage examples

**Validation:**
- [ ] Can read standard ISO 9660 images
- [ ] Can read files and directories
- [ ] Tests cover various ISO formats
- [ ] Documentation includes examples
- [ ] Performance acceptable for large images

---

### Task 4.2: Migrate Classic Cryptography (Educational)

**Status:** NEW
**Target:** `OoBDev.Security.Cryptography.Classic`
**Source:** `BinaryDataDecoders.Cryptography`

**Use Cases:**
- Educational demonstrations of cipher weaknesses
- CTF (Capture The Flag) challenges
- Historical cryptography simulation
- Security training materials

**CRITICAL WARNINGS:**
- These are BROKEN ciphers - NOT for production security
- Educational and historical purposes ONLY
- Must include strong security warnings

**Steps:**

1. Create Extensions project with security warnings in README
2. Migrate cipher implementations:
   - Enigma machine simulation
   - Lorenz cipher
   - Caesar cipher
   - Vigenère cipher
   - PlayFair cipher
3. Mark ALL classes with `[Obsolete("For educational use only. NOT SECURE. Do not use for actual security.")]`
4. Add comprehensive XML documentation explaining why each cipher is broken
5. Add tests demonstrating cipher operation AND breaking
6. Document educational use cases
7. Create separate NuGet package (NOT included in main bundle)

**Validation:**
- [ ] All classes have Obsolete attribute with security warning
- [ ] XML docs explain cipher weaknesses
- [ ] Tests include both encryption and breaking examples
- [ ] Documentation explicitly warns against production use
- [ ] Packaged separately from core framework
- [ ] README includes educational use statement

---

### Task 4.3: Migrate Apple II Support

**Status:** NEW
**Target:** `OoBDev.Retro.Apple2`
**Source:** `BinaryDataDecoders.Apple2`

**Use Cases:**
- Retro computing enthusiasts and emulator developers
- Legacy data recovery from Apple II disk images
- Historical digital preservation projects
- Educational demonstrations of early computing

**Steps:**

1. Create Extensions project for retro computing support
2. Migrate Apple II disk format implementations:
   - DOS 3.3 filesystem
   - ProDOS filesystem (if present)
   - Nibble disk formats
   - DSK image format
3. Migrate disk image readers/writers
4. Add sector-level and file-level access
5. Add comprehensive tests with real Apple II disk images
6. Document disk formats and usage
7. Include examples for common scenarios

**Validation:**
- [ ] Can read DOS 3.3 disk images
- [ ] Can read ProDOS disk images (if supported)
- [ ] Can extract files from disk images
- [ ] Can write disk images
- [ ] Tests use real Apple II disk images
- [ ] Documentation explains disk formats
- [ ] Examples show common use cases

---

### Phase 4 Completion Criteria

- [ ] FileSystems (ISO 9660) migrated and tested
- [ ] Classic Cryptography migrated with security warnings
- [ ] Apple II support migrated and tested
- [ ] Hardware devices (8 devices) migrated and tested
- [ ] Windows Forms components migrated and tested
- [ ] CLI tools migrated/merged
- [ ] Platform-specific code reviewed (UWP/Framework)
- [ ] All migrated features have comprehensive tests
- [ ] All documentation complete with use cases
- [ ] Security warnings in place for cryptography
- [ ] Separate packaging for educational/niche/specialized features

**Estimated Effort:** Medium-Large (complete migration of all specialized features)
**Blocking:** None

---

### Task 4.4: Migrate Windows Forms Components

**Status:** NEW
**Target:** `OoBDev.Extensions.Windows.Forms`
**Source:** `BinaryDataDecoders.Windows.Forms`

**Use Cases:**
- Desktop applications built on OoBDev
- Windows Forms validation controls
- Data binding helpers for desktop scenarios
- Custom UI validators

**Note:** OoBDev supports both desktop and server scenarios - Windows Forms is actively supported in .NET 9.0 and extends the framework for desktop UI applications

**Steps:**

1. Create Extensions project for Windows Forms
2. Migrate validation controls
3. Migrate data binding helpers
4. Migrate custom UI components
5. Ensure .NET 9.0 Windows Forms compatibility
6. Add comprehensive tests
7. Document desktop application scenarios
8. Package separately for desktop/UI use cases

**Validation:**
- [ ] Windows Forms components working in .NET 9.0
- [ ] Validation controls functional
- [ ] Data binding helpers tested
- [ ] Desktop scenario documentation complete
- [ ] Packaged separately from core framework
- [ ] All tests pass

---

## Phase 5: Cleanup & Documentation

**Priority:** LOW
**Dependencies:** Phases 0-4 complete
**Goal:** Finalize migration, update documentation, archive legacy

### Task 5.1: Update Cross-References

**Steps:**
1. Update all internal documentation links
2. Update README files to reference new projects
3. Update architecture documentation if new layers added
4. Create migration guide from BinaryDataDecoders to OoBDev

**Validation:**
- [ ] All links work
- [ ] No broken references
- [ ] Architecture docs reflect new projects

---

### Task 5.2: Create Migration Guide

**Create:** `/current/src/docs/migration/binarydatadecoders-migration-guide.md`

**Contents:**
- Namespace mapping
- API changes
- Breaking changes (if any)
- Migration examples
- Before/after code samples

**Example:**
```markdown
## Migrating from BinaryDataDecoders to OoBDev

### Namespace Changes

| BinaryDataDecoders | OoBDev |
|-------------------|---------|
| BinaryDataDecoders.ToolKit | OoBDev.System |
| BinaryDataDecoders.CodeAnalysis | OoBDev.CodeAnalysis |

### Code Changes

**Before (BinaryDataDecoders):**
```csharp
using BinaryDataDecoders.ExpressionCalculator;
var evaluator = ExpressionEvaluatorFactory.Create<double>();
```

**After (OoBDev):**
```csharp
using OoBDev.ExpressionCalculator;
var evaluator = ExpressionEvaluatorFactory.Create<double>();
// (No change - API compatible)
```
```

**Validation:**
- [ ] Migration guide created
- [ ] Examples tested
- [ ] All common scenarios covered

---

### Task 5.3: Archive BinaryDataDecoders

**Steps:**
1. Mark BinaryDataDecoders as [Obsolete] in README
2. Add deprecation notice
3. Point to OoBDev for active development
4. Keep repository for historical reference

**README Notice:**
```markdown
# BinaryDataDecoders [ARCHIVED]

**This repository has been archived.**

All active development has moved to [OoBDev (dotex)](https://github.com/OutOfBandDevelopment/dotex).

Features from BinaryDataDecoders have been migrated to OoBDev with improvements:
- CodeAnalysis → OoBDev.CodeAnalysis
- ExpressionCalculator → OoBDev.ExpressionCalculator
- Archives → OoBDev.Archives
- NMEA → OoBDev.Protocols.Nmea

See [Migration Guide](docs/migration-to-oobd.md) for details.
```

**Validation:**
- [ ] Archive notice added
- [ ] Links to OoBDev correct
- [ ] Migration guide linked

---

### Task 5.4: Update CHANGELOG

**Add to OoBDev CHANGELOG.md:**

```markdown
## [X.Y.Z] - YYYY-MM-DD

### Added - BinaryDataDecoders Migration
- **CodeAnalysis:** Roslyn-based C#/VB syntax tree navigation via XPath
- **ExpressionCalculator:** ANTLR-based expression parser with optimization
- **Archives:** Complete TAR and enhanced ZIP support
- **Protocols.Nmea:** GPS NMEA 0183 protocol decoder
- **System:** Additional endianness types (BigEndianInt32, etc.)
- **System:** BCD conversion utilities
- **System:** FormattableNumber<T> with engineering/scientific notation
- **System:** Enhanced collections (DoubleLinkedList)

### Fixed - BinaryDataDecoders Migration
- **PathEx:** Fixed lambda bug in wildcard matching
- **StreamDevice:** Added missing nullable annotations
- **StreamDevice:** Fixed event name typo (MessageTransmitterError)
- **ExpressionCalculator:** Replaced non-functional ShiftCommutativeVariablesRight stub
- **SerialPortFactory:** Simplified verbose ternary expression

### Changed
- **Archives:** Moved from OoBDev.System to dedicated OoBDev.Archives project

### Deprecated
- None (no breaking changes)

### Migration Notes
See [BinaryDataDecoders Migration Guide](docs/migration/binarydatadecoders-migration-guide.md)
```

**Validation:**
- [ ] CHANGELOG updated
- [ ] All changes documented
- [ ] Migration guide linked

---

### Task 5.5: Tag Release

**Steps:**
1. Ensure all tests pass
2. Build all NuGet packages
3. Tag commit with version
4. Push tag to repository
5. Create GitHub release with notes

**Release Notes:**
```
# OoBDev v[X.Y.Z] - BinaryDataDecoders Integration

This release integrates features from BinaryDataDecoders, adding powerful
code analysis, expression parsing, archive handling, and protocol decoding
capabilities to the OoBDev framework.

## Highlights

- **Code Analysis:** Query C# and VB code using XPath expressions
- **Expression Calculator:** Parse and optimize mathematical expressions
- **Archive Support:** Full TAR and ZIP read/write capabilities
- **NMEA Protocol:** GPS data decoding with checksum validation
- **Critical Bug Fixes:** Resolved 5 bugs including broken expression optimizer

## Breaking Changes

None - All changes are backward compatible.

## Migration from BinaryDataDecoders

See [Migration Guide](docs/migration/binarydatadecoders-migration-guide.md)

## New Packages

- OoBDev.CodeAnalysis.Abstractions
- OoBDev.CodeAnalysis
- OoBDev.Microsoft.CodeAnalysis.CSharp
- OoBDev.Microsoft.CodeAnalysis.VisualBasic
- OoBDev.Archives
- OoBDev.Protocols.Nmea

## Enhanced Packages

- OoBDev.System (endianness types, BCD, formatting)
- OoBDev.ExpressionCalculator (fixed optimizers)

Full changelog: [CHANGELOG.md](CHANGELOG.md)
```

**Validation:**
- [ ] Version tagged
- [ ] Release created
- [ ] Release notes comprehensive
- [ ] All packages published

---

### Phase 5 Completion Criteria

- [ ] All documentation updated
- [ ] Migration guide created
- [ ] CHANGELOG updated
- [ ] BinaryDataDecoders archived
- [ ] Release tagged and published
- [ ] All cross-references working

**Estimated Effort:** Small
**Blocking:** Phases 0-4

---

## Success Metrics

Migration is successful when:

### Code Quality
- [ ] All critical bugs fixed (Phase 0)
- [ ] Build succeeds with zero warnings
- [ ] All tests pass (Unit + Simulate)
- [ ] Code coverage ≥ 80% for Framework projects
- [ ] Code coverage ≥ 90% for LINQ/Expression projects

### Architecture Compliance
- [ ] All projects in correct layer (Framework, Extensions, ExternalServices)
- [ ] Provider/factory pattern implemented where appropriate
- [ ] Dependency injection via TryAdd* extensions
- [ ] IOptions<T> for configuration
- [ ] No breaking changes to existing OoBDev APIs

### Documentation
- [ ] README.md in every project (enforced by build)
- [ ] XML documentation on all public APIs
- [ ] Usage examples for all major features
- [ ] Migration guide complete
- [ ] CHANGELOG updated

### Testing
- [ ] All test categories assigned correctly
- [ ] Integration tests for new features
- [ ] Performance tests for critical paths (if applicable)
- [ ] Edge cases covered

### Packaging
- [ ] NuGet packages generated successfully
- [ ] Package metadata complete
- [ ] Dependencies correct
- [ ] README included in packages

### Release
- [ ] Version tagged
- [ ] GitHub release created
- [ ] Release notes comprehensive
- [ ] BinaryDataDecoders archived with notice

---

## Risk Mitigation

### Risk: Breaking Changes

**Mitigation:**
- All changes additive (new projects/features)
- No modifications to existing OoBDev public APIs
- Deprecation warnings before removal
- Migration guide for any API changes

### Risk: Performance Regression

**Mitigation:**
- Performance tests for critical paths
- Benchmark comparison before/after
- Code review for hot paths
- Profiling of new features

### Risk: Test Coverage Drop

**Mitigation:**
- Enforce 80% minimum coverage
- Fail build if coverage drops
- Review coverage reports
- Add tests before merge

### Risk: Incomplete Migration

**Mitigation:**
- Phased approach (can stop after any phase)
- Each phase standalone
- Priority-based (critical first)
- Clear completion criteria

### Risk: Namespace Conflicts

**Mitigation:**
- Clear namespace mapping defined
- Gradual migration (BinaryDataDecoders can coexist)
- Deprecation warnings in BinaryDataDecoders
- Aliasing support if needed

---

## Rollback Plan

If migration causes issues:

1. **Immediate Rollback:**
   - Revert commit
   - Restore previous version
   - Fix issue

2. **Partial Rollback:**
   - Remove problematic feature
   - Keep successful migrations
   - Document issue

3. **Full Rollback:**
   - Revert to pre-migration state
   - Create fix branch
   - Retry after fixes

**Rollback Criteria:**
- Build failures that can't be fixed quickly
- Test failures indicating fundamental issue
- Performance regression > 20%
- Breaking changes discovered

---

## Related Documentation

- [BinaryDataDecoders Feature Mapping](./binarydatadecoders-feature-mapping.md)
- [Architectural Guidelines](../architecture/architectural-guidelines.md)
- [Architectural Standards](../architecture/architectural-standards.md)
- [Layering Architecture](../architecture/layering-architecture.md)
- [Provider/Factory Pattern](../architecture/provider-factory-pattern.md)

---

## Change Log

- 2026-01-12 v1.0: Initial BinaryDataDecoders migration plan created
