# OoBDev.Oobtainium Migration Plan

**Version:** 1.0
**Last Updated:** 2026-01-12
**Source Repository:** OoBDev.Oobtainium (`Incomming/OoBDev.Oobtainium`)
**Target Repository:** OoBDev (dotex) Framework
**Status:** ⏸️ AWAITING DECISION - Migration approach to be determined

---

## Executive Summary

**OoBDev.Oobtainium** is a complete .NET mocking/proxy framework (48 files, ~1,578 LOC) that **does not exist in the main OoBDev framework**. This is fundamentally different from dotnet-lib (which was 95% synchronized) - Oobtainium is a **standalone library** requiring a strategic decision.

### Key Facts

- **Repository:** https://github.com/OutOfBandDevelopment/oobtainium/
- **Framework:** .NET Standard 2.1 (needs upgrade to .NET 9.0)
- **Dependencies:** Microsoft.Extensions.* 3.1.9 (2020 - needs upgrade)
- **Architecture:** Proper layering (Abstractions + Implementation + Tests)
- **Purpose:** Runtime proxy creation, method interception, call recording
- **Comparison:** Simpler than Moq, but less feature-rich

### Decision Required

**Choose one of four options:**

1. **MIGRATE** to `src/Framework/OoBDev.Mocking/` - Become part of core framework
2. **REFERENCE** as external NuGet package - Keep separate, reference when needed
3. **ARCHIVE** in `Incomming/` - Keep for reference, don't integrate
4. **DELETE** - Remove entirely (mocking already solved by Moq/NSubstitute)

**RECOMMENDATION: Option 4 (Delete)** - See rationale in feature mapping document

---

## Migration Principles

If migration is chosen (Option 1), all work MUST follow these principles from `/docs/architecture`:

1. **Layered Architecture** - Place in appropriate layer (Framework or Extensions)
2. **Provider/Factory Pattern** - Already follows this pattern ✓
3. **Dependency Injection** - Already uses ServiceCollection ✓
4. **Type Safety** - Generic constraints, nullable enabled ✓
5. **Testing** - 80% coverage minimum (currently has tests ✓)
6. **Documentation** - README required (needs creation)
7. **No Breaking Changes** - N/A (new library)
8. **.NET 9.0 Target** - Currently .NET Standard 2.1 (needs upgrade)
9. **Modern Dependencies** - Currently 3.1.9 packages (needs upgrade)

---

## Option 1: MIGRATE to Main Framework

### Phase 0: Investigation & Preparation

**Priority:** HIGH (if migration chosen)
**Status:** NOT STARTED

#### Task 0.1: Verify No Conflicts

- [ ] Search main codebase for any existing mocking/proxy infrastructure
- [ ] Verify namespace `OoBDev.Mocking` is available
- [ ] Check for existing dependencies on Moq/NSubstitute/FakeItEasy
- [ ] Review test projects for existing mock usage patterns

#### Task 0.2: Dependency Analysis

- [ ] Check System.ServiceModel.Primitives .NET 9.0 compatibility
- [ ] Verify DispatchProxy availability in .NET 9.0
- [ ] Identify any breaking changes in Reflection.Emit APIs
- [ ] Test DI integration with Microsoft.Extensions 9.0+

#### Task 0.3: Usage Validation

- [ ] Determine if any existing OoBDev code depends on Oobtainium
- [ ] Review GitHub repository for active development
- [ ] Check NuGet if published (download statistics)
- [ ] Contact original author (Matthew Whited) for status

---

### Phase 1: Framework Modernization

**Priority:** HIGH (if migration chosen)
**Dependencies:** Phase 0 complete

#### Task 1.1: Upgrade to .NET 9.0

**Target Projects:**
- OoBDev.Oobtainium.Abstractions.csproj
- OoBDev.Oobtainium.csproj
- OoBDev.Oobtainium.Tests.csproj

**Changes Required:**

```xml
<!-- OLD -->
<TargetFramework>netstandard2.1</TargetFramework>
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="3.1.9" />

<!-- NEW -->
<TargetFramework>net9.0</TargetFramework>
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
```

**Steps:**
- [ ] Update all `.csproj` files to `<TargetFramework>net9.0</TargetFramework>`
- [ ] Update package references:
  - [ ] Microsoft.Extensions.DependencyInjection.Abstractions: 3.1.9 → 9.0.0
  - [ ] Microsoft.Extensions.Logging.Abstractions: 3.1.9 → 9.0.0
  - [ ] Microsoft.Extensions.Logging: 3.1.9 → 9.0.0
  - [ ] Microsoft.Extensions.Logging.Console: 3.1.9 → 9.0.0
  - [ ] Microsoft.Extensions.Logging.Debug: 3.1.9 → 9.0.0
  - [ ] Microsoft.Extensions.Configuration.Abstractions: 3.1.9 → 9.0.0
- [ ] Verify System.ServiceModel.Primitives compatibility (check latest version)
- [ ] Update test framework:
  - [ ] MSTest.TestFramework: 2.1.1 → 3.7.0 (or latest)
  - [ ] MSTest.TestAdapter: 2.1.0 → 3.7.0
  - [ ] Microsoft.NET.Test.Sdk: 16.5.0 → 17.12.0
- [ ] Run `dotnet build` and fix any breaking changes
- [ ] Run `dotnet test` and verify all tests pass

**Validation:**
- [ ] All projects build without warnings
- [ ] All tests pass
- [ ] No deprecated API usage

---

#### Task 1.2: Align with OoBDev Standards

**Update project files to match OoBDev conventions:**

- [ ] Add `<Nullable>enable</Nullable>` (already present ✓)
- [ ] Add `<ImplicitUsings>disable</ImplicitUsings>` (OoBDev standard)
- [ ] Update `Directory.Build.props` to match OoBDev structure
- [ ] Add `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- [ ] Configure analyzer rules (if OoBDev has standard ruleset)
- [ ] Add deterministic build settings

**Example:**
```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>disable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

**Validation:**
- [ ] Build with warnings-as-errors succeeds
- [ ] No implicit usings (all using statements explicit)
- [ ] Nullable reference types enforced

---

### Phase 2: Integration into Main Codebase

**Priority:** MEDIUM (if migration chosen)
**Dependencies:** Phase 1 complete

#### Task 2.1: Determine Layer Placement

**Options:**

**Option A: Framework Layer** - `src/Framework/OoBDev.Mocking/`
- Rationale: Core testing infrastructure, broadly useful
- Peers: OoBDev.System, OoBDev.AspNetCore.Mvc
- Pro: Central location, foundational
- Con: Framework should be stable, mocking is specialized

**Option B: Extensions Layer** - `src/Extensions/OoBDev.Extensions.Mocking/`
- Rationale: Optional testing tool, not core framework
- Peers: Other specialized extensions
- Pro: Clearly optional, easier to evolve
- Con: May not get as much visibility

**Option C: Test Utilities** - `src/Framework/OoBDev.TestUtilities.Mocking/`
- Rationale: Testing-specific infrastructure
- Peers: OoBDev.TestUtilities (NumericAsserts)
- Pro: Clear purpose, discoverable by test authors
- Con: May limit scope

**RECOMMENDATION: Option B (Extensions)** - Mocking is optional, specialized

**Decision Required:** Choose layer placement

---

#### Task 2.2: Restructure Projects

**Move to chosen location:**

**If Framework layer chosen:**
```
src/Framework/
├── OoBDev.Mocking.Abstractions/
├── OoBDev.Mocking/
└── Tests/OoBDev.Mocking.Tests/
```

**If Extensions layer chosen:**
```
src/Extensions/
├── OoBDev.Extensions.Mocking.Abstractions/
├── OoBDev.Extensions.Mocking/
└── Tests/OoBDev.Extensions.Mocking.Tests/
```

**Steps:**
- [ ] Create target directories
- [ ] Move all source files
- [ ] Update `.csproj` files
- [ ] Update namespaces (if changing from OoBDev.Oobtainium to OoBDev.Mocking)
- [ ] Add to main solution (`src/OoBDev.sln`)
- [ ] Update inter-project references

**Namespace Decision:**
- Option A: Keep `OoBDev.Oobtainium.*` (preserves identity)
- Option B: Rename to `OoBDev.Mocking.*` (clearer purpose)
- Option C: Rename to `OoBDev.Extensions.Mocking.*` (matches layer)

**RECOMMENDATION: Option B** - "Mocking" is clearer than "Oobtainium"

---

#### Task 2.3: Create Documentation

**Required files:**

**README.md** (in project root)
```markdown
# OoBDev.Mocking

Lightweight mocking and proxy framework for .NET 9.0

## Features
- Runtime proxy creation for interfaces
- Method call interception and recording
- Fluent API for behavior binding
- Full async/await support
- Dependency injection integration

## Installation
[NuGet package info]

## Quick Start
[Basic example]

## Documentation
[Link to docs]
```

**Steps:**
- [ ] Create `README.md` in project root
- [ ] Add usage examples
- [ ] Document API surface
- [ ] Create getting started guide
- [ ] Add to main documentation site
- [ ] Update `docs/architecture/layering-architecture.md` with new project

**Validation:**
- [ ] README.md exists (build-enforced in OoBDev)
- [ ] Examples compile and run
- [ ] API documentation complete

---

### Phase 3: Testing & Validation

**Priority:** HIGH (if migration chosen)
**Dependencies:** Phase 2 complete

#### Task 3.1: Verify Test Coverage

**Current State:**
- Has MSTest test project with GeneralTests.cs (178 lines)
- Includes proof-of-concept experiments

**Requirements:**
- 80% code coverage minimum (OoBDev standard)
- Tests must use OoBDev.TestUtilities.NumericAsserts where applicable
- Test categories defined

**Steps:**
- [ ] Run code coverage analysis
- [ ] Add missing test cases to reach 80%
- [ ] Ensure tests use MSTest (already ✓)
- [ ] Add test categories (Unit, Integration)
- [ ] Verify proof-of-concept tests are valuable or remove them
- [ ] Update `.runsettings` to match OoBDev standards

**Validation:**
- [ ] Code coverage ≥ 80%
- [ ] All tests pass
- [ ] Test categories applied
- [ ] No flaky tests

---

#### Task 3.2: Integration Testing

**Verify integration with OoBDev patterns:**

- [ ] Test DI registration with OoBDev's ServiceCollection patterns
- [ ] Verify compatibility with OoBDev.AspNetCore.* projects
- [ ] Test in MSTest context (existing OoBDev test projects)
- [ ] Verify logging integration with Microsoft.Extensions.Logging

**Create integration test examples:**
- [ ] Mock an OoBDev interface (e.g., ITemplateEngine)
- [ ] Record calls in an ASP.NET Core controller test
- [ ] Verify async method mocking works with OoBDev's async APIs

**Validation:**
- [ ] Integration tests pass
- [ ] No conflicts with existing OoBDev infrastructure
- [ ] Works in real-world scenarios

---

### Phase 4: Packaging & Distribution

**Priority:** MEDIUM (if migration chosen)
**Dependencies:** Phase 3 complete

#### Task 4.1: NuGet Package Configuration

**Update .csproj for NuGet:**

```xml
<PropertyGroup>
  <PackageId>OoBDev.Mocking</PackageId>
  <Version>1.0.0</Version>
  <Authors>Out-of-Band Development, LLC</Authors>
  <Description>Lightweight mocking and proxy framework for .NET 9.0</Description>
  <PackageTags>mock;mocking;proxy;testing;unittest</PackageTags>
  <RepositoryUrl>https://github.com/[your-repo]</RepositoryUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
</PropertyGroup>
```

**Steps:**
- [ ] Configure package metadata
- [ ] Create package icon
- [ ] Write package description
- [ ] Add license information
- [ ] Configure symbol packages for debugging

**Validation:**
- [ ] Package builds successfully
- [ ] Package contains correct files
- [ ] Metadata is accurate

---

#### Task 4.2: CI/CD Integration

**Add to build pipeline:**

- [ ] Add to `.github/workflows/dotnet.yml`
- [ ] Configure path filters for selective compilation
- [ ] Add NuGet publish step (if applicable)
- [ ] Configure version bumping strategy

**Validation:**
- [ ] CI build passes
- [ ] Tests run in CI
- [ ] Packages published (if enabled)

---

### Phase 5: Documentation & Examples

**Priority:** LOW (if migration chosen)
**Dependencies:** Phase 4 complete

#### Task 5.1: Usage Documentation

**Create comprehensive guides:**

- [ ] Getting Started guide
- [ ] API reference documentation
- [ ] Migration guide (if renaming from Oobtainium)
- [ ] Comparison with Moq/NSubstitute
- [ ] Best practices

**Add to OoBDev documentation site**

---

#### Task 5.2: Example Projects

**Create sample projects:**

- [ ] Basic mock creation example
- [ ] Call recording example
- [ ] Async method mocking example
- [ ] DI integration example
- [ ] ASP.NET Core integration example

**Location:** `samples/OoBDev.Mocking.Samples/`

---

## Option 2: REFERENCE as External Dependency

### Steps

1. **Verify NuGet Package Exists**
   - [ ] Check if published to nuget.org
   - [ ] Verify package metadata
   - [ ] Check download statistics

2. **If Not Published:**
   - [ ] Publish to NuGet (with author permission)
   - [ ] Or: Reference via GitHub package feed
   - [ ] Or: Local NuGet feed

3. **Reference in Consuming Projects**
   - [ ] Add `<PackageReference>` where needed
   - [ ] Update documentation to mention availability
   - [ ] Add to recommended tools list

**Effort:** LOW
**Maintenance:** MINIMAL (external)

---

## Option 3: ARCHIVE in Incomming/

### Steps

1. **Create Archive Documentation**
   - [ ] Add `README.md` to `Incomming/OoBDev.Oobtainium/`
   - [ ] Document what it is, why not migrated
   - [ ] Link to GitHub repository
   - [ ] Note alternatives (Moq, NSubstitute)

2. **Update Migration Tracking**
   - [ ] Mark as "ARCHIVED" in migration docs
   - [ ] Update TODO.md with decision

**Effort:** MINIMAL
**Maintenance:** NONE

---

## Option 4: DELETE

### Steps

1. **Verify No Dependencies**
   - [ ] Confirm no OoBDev code uses Oobtainium
   - [ ] Check if referenced anywhere

2. **Document Decision**
   - [ ] Update migration docs with rationale
   - [ ] Note in TODO.md
   - [ ] Preserve this document for reference

3. **Remove Directory**
   - [ ] `rm -rf /current/src/Incomming/OoBDev.Oobtainium`

4. **Update TODO.md**
   - [ ] Mark Oobtainium investigation complete
   - [ ] Note decision to not migrate

**Effort:** MINIMAL
**Maintenance:** NONE

---

## Recommended Action

### **RECOMMENDED: Option 4 (Delete)**

**Rationale:**

1. **Well-Solved Problem** - Moq has 460M+ downloads, NSubstitute 130M+
2. **No Strategic Value** - OoBDev's value is in binary processing, protocols, hardware - not mocking tools
3. **Maintenance Burden** - Requires continuous updates to keep pace with .NET
4. **Limited Differentiation** - Simpler than Moq but less capable
5. **Resource Allocation** - Team focus better spent on unique OoBDev features
6. **External GitHub Repo** - Already available for those who need it

**For OoBDev's Own Testing:**
- Add Moq or NSubstitute as test dependencies
- Use industry-standard tools
- Leverage extensive documentation and community support

---

## Decision Matrix

| Criteria | Migrate | Reference | Archive | Delete |
|----------|---------|-----------|---------|--------|
| **Effort** | HIGH | LOW | MINIMAL | MINIMAL |
| **Maintenance** | HIGH | MINIMAL | NONE | NONE |
| **Value Add** | LOW | LOW | NONE | NONE |
| **User Benefit** | MINIMAL | MINIMAL | NONE | NONE |
| **Strategic Fit** | POOR | NEUTRAL | N/A | N/A |
| **Risk** | MEDIUM | LOW | NONE | NONE |
| **RECOMMENDATION** | ❌ | ❌ | ⚠️ | ✅ |

---

## Questions for Final Decision

1. **Is Oobtainium currently used?**
   - In any OoBDev projects?
   - By external users?

2. **Strategic alignment?**
   - Does OoBDev need a mocking framework?
   - What's the unique value proposition?

3. **Resource commitment?**
   - Who will maintain after migration?
   - Bandwidth for .NET 9.0+ updates?

4. **User demand?**
   - Have users requested this?
   - Is there a gap Moq doesn't fill?

**If answers are mostly "No" or "Uncertain" → Delete**
**If answers are "Yes" with strong justification → Migrate**

---

## Completion Criteria

**If Migrated (Option 1):**
- [ ] All projects upgraded to .NET 9.0
- [ ] All dependencies updated to 9.0.x
- [ ] Projects moved to appropriate layer
- [ ] 80% test coverage achieved
- [ ] README.md created
- [ ] Documentation published
- [ ] CI/CD integrated
- [ ] NuGet package published (if applicable)

**If Referenced (Option 2):**
- [ ] NuGet package verified or published
- [ ] Documentation updated
- [ ] Consuming projects configured

**If Archived (Option 3):**
- [ ] Archive README.md created
- [ ] Migration docs updated
- [ ] Decision documented

**If Deleted (Option 4):**
- [ ] No dependencies verified
- [ ] Directory removed
- [ ] Decision documented in TODO.md
- [ ] Migration docs updated

---

## Related Documents

- [Oobtainium Feature Mapping](./oobtainium-feature-mapping.md) - Comprehensive analysis
- [GitHub Repository](https://github.com/OutOfBandDevelopment/oobtainium/)
- [Architectural Guidelines](../architecture/architectural-guidelines.md)

---

## Change Log

- 2026-01-12 v1.0: Initial migration plan created
