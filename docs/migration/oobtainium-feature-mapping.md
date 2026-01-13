# OoBDev.Oobtainium Feature Mapping

**Version:** 1.0
**Last Updated:** 2026-01-12
**Source:** OoBDev.Oobtainium (`Incomming/OoBDev.Oobtainium`)
**Target:** OoBDev (dotex) Framework
**Status:** 🔍 INVESTIGATION - Does not exist in main codebase

---

## Overview

**OoBDev.Oobtainium** is a sophisticated .NET mocking and proxy framework that **does not currently exist** in the main OoBDev framework. Unlike dotnet-lib (which was 95% synchronized), this is a **completely new library** that must be evaluated for migration.

### What is Oobtainium?

A comprehensive mock/proxy framework providing:
- Runtime interface proxy creation using DispatchProxy
- Method call interception and recording
- Fluent API for binding methods to behaviors
- Full async/await support
- Thread-safe call recording
- Microsoft.Extensions.DependencyInjection integration

**Repository:** https://github.com/OutOfBandDevelopment/oobtainium/

---

## Statistics

**Project Metrics:**
- **Total Files:** 48 C# files
- **Total LOC:** ~1,578 lines
- **Projects:** 3 (Abstractions, Implementation, Tests)
- **Target Framework:** .NET Standard 2.1 / .NET Core 3.1
- **Dependencies:** Microsoft.Extensions.* (3.1.9), System.ServiceModel.Primitives (4.7.0)
- **Test Framework:** MSTest
- **Author:** Matthew Whited
- **Company:** Out-of-Band Development, LLC
- **Copyright:** 2021

**Migration Status:**
- **EXISTS in main:** ❌ NO - Completely new library
- **Status:** NEW - Requires migration decision
- **Priority:** TBD - Depends on use case and framework goals

---

## Architecture

### Project Structure

```
OoBDev.Oobtainium/
├── OoBDev.Oobtainium.Abstractions/    [Interfaces layer]
│   └── 14 files, ~300 LOC
├── OoBDev.Oobtainium/                 [Implementation]
│   └── 21 files, ~900 LOC
└── OoBDev.Oobtainium.Tests/           [MSTest]
    └── 13 files, ~370 LOC
```

### Namespace Organization

- **OoBDev.Oobtainium** (Root)
  - ComponentModel - Type system utilities
  - Recording - Call recording features
  - Reflection - Runtime type generation

### Layered Design

**Follows separation of concerns:**
1. **Abstractions Layer** - All interfaces (ICallBinder, ICallRecorder, ICaptureProxyFactory)
2. **Implementation Layer** - Concrete classes (CaptureProxy, CallHandler, BindingBuilder)
3. **Test Layer** - MSTest unit tests with proof-of-concept experiments

---

## Feature Breakdown

### 1. Core Proxy/Mock Creation

**Status:** NEW - No equivalent in main OoBDev

**Features:**
- `CaptureProxy<T>` - DispatchProxy-based runtime proxy for any interface
- `CaptureProxyFactory` - Factory pattern for proxy creation
- `CallRecorderProxy<T>` - Extended proxy with recording capabilities
- Method invocation logging via ILogger<T>

**Use Cases:**
- Unit testing with dynamic mocks
- Service interface interception
- Call monitoring and debugging

**Files:**
- `CaptureProxy.cs` (153 lines) - Core DispatchProxy implementation
- `CaptureProxyFactory.cs` - Factory with DI integration
- `Recording/CallRecorderProxyFactory.cs` - Extended factory with recording

---

### 2. Method Binding & Execution

**Status:** NEW - No equivalent in main OoBDev

**Features:**
- Fluent API for binding interface methods to delegates
- Type-safe method references using lambda expressions
- Support for void, Task, Task<T>, and value-returning methods
- Generic method support with type parameters
- Property and indexer support with backing stores

**Use Cases:**
- Configure mock behavior without inheritance
- Runtime behavior injection
- Test fixture setup

**Files:**
- `ICallBinder.cs` / `CallBinder.cs` - Entry point for binding configuration
- `IBindingBuilder.cs` / `BindingBuilder.cs` (84 lines) - Fluent API implementation
- `CallBindingStore.cs` - ConcurrentDictionary-based storage
- `CallHandler.cs` - Delegate execution engine
- `ExpressionExtensions.cs` - Lambda to MethodInfo conversion

**Example Usage:**
```csharp
binder.Build<IMyService>()
    .Bind(s => s.GetData(), () => "mock data")
    .Bind(s => s.SaveAsync(default), async (data) => await SaveMock(data));
```

---

### 3. Call Recording & History

**Status:** NEW - No equivalent in main OoBDev

**Features:**
- Thread-safe recording of all method invocations
- Captures: instance, interface type, method info, arguments, return values
- Enumerable collection for easy iteration
- ExcludeFromRecordingAttribute to skip specific methods
- CaptureHandler delegate for custom recording callbacks

**Use Cases:**
- Verify method calls in unit tests
- Debug interaction patterns
- Audit service calls
- Behavior verification

**Files:**
- `Recording/ICallRecorder.cs` / `CallRecorder.cs` - Recording infrastructure
- `Recording/RecordedCall.cs` - Call data structure
- `Recording/ICallRecorderFactory.cs` - Factory pattern
- `Recording/ExcludeFromRecordingAttribute.cs` - Opt-out attribute

**Example Usage:**
```csharp
var mock = factory.CreateWithRecorder<IMyService>();
mock.GetData();
mock.SaveAsync(data);

if (mock.TryGetRecorder(out var recorder))
{
    Assert.AreEqual(2, recorder.Count());
    Assert.IsTrue(recorder.Any(c => c.Method.Name == "GetData"));
}
```

---

### 4. Type System Extensions

**Status:** NEW - No equivalent in main OoBDev

**Features:**
- `TypeExtensions.GetDefaultValue()` - Get default value for any type
- `TypeExtensions.ConvertOrDefault()` - Safe type conversion
- Dynamic type generation using Reflection.Emit
- Interface merging and runtime type creation
- GeneratedInterfaceAttribute for marking dynamic types

**Use Cases:**
- Default value handling for generic types
- Type conversion in proxy methods
- Runtime interface composition

**Files:**
- `ComponentModel/TypeExtensions.cs` - Type utilities
- `Reflection/TypeInstanceExtender.cs` (81 lines) - Reflection.Emit type generation
- `Reflection/GeneratedInterfaceAttribute.cs` - Custom attribute

---

### 5. Async/Task Support

**Status:** NEW - No equivalent in main OoBDev

**Features:**
- Automatic Task awaiting in proxy methods
- Task<T> result unwrapping and rewrapping
- Proper generic type preservation
- Support for Task, Task<void>, Task<T>
- Async delegate binding

**Use Cases:**
- Mock async service methods
- Test async workflows
- Async call recording

**Files:**
- `TaskExtensions.cs` - Task unwrapping utilities
- `CaptureProxy.cs` - Async method handling in Invoke()

---

### 6. Dependency Injection Integration

**Status:** NEW - Compatible with main OoBDev DI patterns

**Features:**
- `ServiceCollectionExtensions.AddOobtainium()` - IServiceCollection extension
- Registers all core services with proper lifetimes
- Scoped lifetime for recorders (per-request state)
- Transient for factories (stateless)
- Singleton for binders (shared configuration)

**Use Cases:**
- Integration with ASP.NET Core
- MSTest with DI container
- Service provider patterns

**Files:**
- `ServiceCollectionExtensions.cs` - DI registration

**Example Usage:**
```csharp
services.AddOobtainium();
var factory = serviceProvider.GetRequiredService<ICaptureProxyFactory>();
```

---

### 7. Reflection & Instance Wrapping

**Status:** NEW - Advanced feature not in main OoBDev

**Features:**
- WrappedProxy - Base class for proxies that wrap existing instances
- TypeInstanceExtender - Runtime interface merging using Reflection.Emit
- INeedInstance/IHaveInstance - Instance access patterns

**Use Cases:**
- Wrap existing objects with additional interfaces
- Extend instances at runtime
- Decorator pattern implementation

**Files:**
- `Reflection/WrappedProxy.cs` - Wrapper base class
- `Reflection/TypeInstanceExtender.cs` - Type generation
- `Reflection/INeedInstance.cs` / `IHaveInstance.cs` - Interfaces

---

## Comparison with Existing Mocking Frameworks

### vs. Moq (Most Popular)

| Feature | Oobtainium | Moq |
|---------|-----------|-----|
| Syntax | Fluent binding API | Setup/Returns fluent API |
| Recording | Built-in call recorder | Verify() after the fact |
| DI Integration | Native ServiceCollection | Manual creation |
| Async Support | Full native support | Full support |
| Matchers | Basic | Advanced (It.IsAny, regex) |
| Callback Support | CaptureHandler delegates | Callback() method |
| Strictness | Lenient (no strict mode) | Strict/Loose modes |
| Learning Curve | Simple, focused | More features, steeper |

**Oobtainium Advantages:**
- Simpler API for basic scenarios
- Built-in call recording
- Native DI integration
- Lightweight (1,578 LOC vs Moq's 20,000+)

**Moq Advantages:**
- More mature and widely adopted
- Advanced matchers and verification
- Strict mode for detecting unexpected calls
- Extensive documentation and community

---

### vs. NSubstitute

| Feature | Oobtainium | NSubstitute |
|---------|-----------|--------------|
| Syntax | Bind() methods | Natural syntax (mock[arg]) |
| Recording | Explicit recorder | Received() calls |
| Setup | Fluent builder | Direct assignment |
| Verification | Enumerable recorder | Received() assertions |

---

### vs. FakeItEasy

| Feature | Oobtainium | FakeItEasy |
|---------|-----------|------------|
| Syntax | Explicit binding | A.CallTo() |
| Configuration | Fluent builder | Fluent assertions |
| Recording | Built-in | Recorded calls API |

---

## Migration Considerations

### Pros (Reasons to Migrate)

1. **Consistent Namespace** - Already uses OoBDev.* namespace
2. **Lightweight Alternative** - Simple mock framework for basic scenarios
3. **DI Integration** - Follows Microsoft.Extensions patterns already in OoBDev
4. **Good Architecture** - Proper separation (Abstractions + Implementation)
5. **Active Development** - GitHub repository available
6. **Test Coverage** - Includes comprehensive MSTest suite

### Cons (Reasons NOT to Migrate)

1. **Duplicate Functionality** - Moq, NSubstitute already exist in ecosystem
2. **Maintenance Burden** - Need to keep up with .NET versions
3. **Limited Features** - Less mature than established frameworks
4. **Old Dependencies** - Uses .NET Standard 2.1 and 3.1.9 packages
5. **Specialized Use Case** - Mocking is well-solved by existing tools
6. **GitHub Dependency** - External repository may not align with OoBDev goals

### Framework Compatibility Issues

**Current State:**
- Target: .NET Standard 2.1
- Dependencies: Microsoft.Extensions.* 3.1.9 (released 2020)

**OoBDev Framework Standard:**
- Target: .NET 9.0
- Dependencies: Latest Microsoft.Extensions.* 9.0+

**Migration Required:**
- Upgrade to .NET 9.0
- Update all package references to 9.0.x
- Verify System.ServiceModel.Primitives compatibility
- May need to replace DispatchProxy if breaking changes

---

## Decision Matrix

### Option A: MIGRATE to Main Framework

**Location:** `src/Framework/OoBDev.Mocking/` or `src/Extensions/OoBDev.Extensions.Mocking/`

**Pros:**
- Provides first-party mocking solution
- Consistent with OoBDev patterns
- Simpler than Moq for basic scenarios
- Good for internal OoBDev testing

**Cons:**
- Maintenance overhead
- Duplicate of existing tools
- Need to modernize dependencies

**Effort:** MEDIUM
- Update to .NET 9.0
- Migrate dependencies
- Add to main solution
- Update README and docs

---

### Option B: KEEP as Separate Package

**Location:** Keep in `Incomming/OoBDev.Oobtainium/`

**Pros:**
- No migration effort
- Maintains separate GitHub repo
- Can evolve independently

**Cons:**
- Not integrated with main framework
- Inconsistent .NET versions
- Discovery issues (users won't know it exists)

**Effort:** NONE

---

### Option C: REFERENCE as External Dependency

**Location:** Reference as NuGet package (if published)

**Pros:**
- No code maintenance
- External updates handled by author
- Clean separation

**Cons:**
- May not be published to NuGet
- Versioning dependencies
- Less control

**Effort:** LOW
- Publish to NuGet (if needed)
- Reference in consuming projects

---

### Option D: DO NOT MIGRATE (Delete)

**Rationale:**
- Moq, NSubstitute, FakeItEasy already solve this problem
- Limited value in maintaining another mocking framework
- Effort better spent on unique OoBDev features

**Pros:**
- No maintenance burden
- Focus on core OoBDev value
- Users can choose preferred mocking tool

**Cons:**
- Lose lightweight alternative
- Lose DI-integrated option

**Effort:** MINIMAL
- Document decision
- Archive or delete

---

## Recommended Action

### **RECOMMENDED: Option D - Do Not Migrate**

**Rationale:**

1. **Well-Solved Problem** - Mocking frameworks are mature and abundant
2. **Limited Differentiation** - Oobtainium doesn't offer unique features that justify maintenance
3. **Resource Allocation** - Better to focus on unique OoBDev features (BinaryDataDecoders, protocols, hardware, etc.)
4. **Ecosystem Integration** - Users already have mocking tool preferences (Moq, NSubstitute)
5. **Old Dependencies** - Would require significant modernization effort

**Alternative Recommendation:**
If mocking capability is desired for OoBDev's own tests:
- Use Moq or NSubstitute as dependencies
- Focus on framework features, not tooling

---

## Questions Requiring Answers

Before making final decision:

1. **Is Oobtainium actively used in any OoBDev projects?**
   - Check if any existing code depends on it
   - Review commit history

2. **Is it published to NuGet?**
   - If yes, what's the download count?
   - Active community?

3. **What's the strategic value?**
   - Does OoBDev need its own mocking framework?
   - Is there a unique use case not covered by Moq?

4. **Who is the intended audience?**
   - Internal OoBDev testing?
   - External users?
   - Both?

5. **What's the maintenance commitment?**
   - Who will maintain it?
   - Is there bandwidth for .NET 9.0 updates?

---

## Related Documents

- [Oobtainium Migration Plan](./oobtainium-migration-plan.md) - Detailed migration tasks (if Option A chosen)
- [Architectural Guidelines](../architecture/architectural-guidelines.md)
- [GitHub Repository](https://github.com/OutOfBandDevelopment/oobtainium/)

---

## Change Log

- 2026-01-12 v1.0: Initial feature mapping created
