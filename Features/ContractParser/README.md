# ContractParser - Service Contract DSL Feature

**Status:** 📋 SPECIFICATION - Requirements Gathering
**Priority:** MEDIUM - Future Implementation Candidate
**Last Updated:** 2026-01-20

---

## Overview

ContractParser is a proposed Domain-Specific Language (DSL) and parser for describing service contracts using a text-based format with ANTLR4 grammar. The project currently exists as grammar specifications and example data files without implementation code.

**Current State:**
- ANTLR4 grammar specifications (2 versions)
- 5 real-world example service contracts
- NO implementation code (parser, code generator)

**Proposed Value:**
- Human-readable API contract documentation
- Code generation from contracts (DTOs, service interfaces)
- Contract-first API development
- Single source of truth for service specifications

---

## Use Cases

### UC-1: API Documentation Generation

**Actor:** Software Developer / Technical Writer

**Goal:** Generate comprehensive API documentation from contract files

**Flow:**
1. Write service contract in DSL format (.ServiceContract file)
2. Run parser to generate markdown/HTML documentation
3. Publish documentation to wiki or developer portal

**Business Value:**
- Consistent documentation format across all services
- Documentation in sync with contract specification
- Reduced manual documentation effort

**Example:**
```
AccountPortfolioService @service
  | Retrieves complete account portfolio information including leads, vehicles, and notes
  | https://confluence.example.com/display/API/AccountPortfolio

  + GetPortfolioDetails
    | Fetches all portfolio data for a given account ID
    > AccountId : string
    < Portfolio : AccountPortfolioDto
    < LeadList : List<LeadDto>
    < VehicleList : List<VehicleDto>
```

---

### UC-2: DTO Code Generation

**Actor:** Backend Developer

**Goal:** Generate C# DTO classes from contract specifications

**Flow:**
1. Define data structures in contract DSL
2. Run code generator tool
3. Generate C# classes with proper attributes (serialization, validation)
4. Include generated code in service project

**Business Value:**
- Consistent DTO structure across services
- Reduced boilerplate code
- Type-safe contracts shared between teams

**Example Input:**
```
AccountDto @dto
  AccountId : string
  AccountNumber : string
  Balance : decimal
  CreatedDate : DateTime
  Status : AccountStatus

AccountStatus @enum
  Active = 1
  Inactive = 2
  Suspended = 3
  Closed = 4
```

**Example Output (C#):**
```csharp
[DataContract]
public class AccountDto
{
    [DataMember]
    public string AccountId { get; set; }

    [DataMember]
    public string AccountNumber { get; set; }

    [DataMember]
    public decimal Balance { get; set; }

    [DataMember]
    public DateTime CreatedDate { get; set; }

    [DataMember]
    public AccountStatus Status { get; set; }
}

public enum AccountStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Closed = 4
}
```

---

### UC-3: Service Interface Generation

**Actor:** Backend Developer

**Goal:** Generate service interfaces from contract operations

**Flow:**
1. Define service operations with input/output parameters
2. Run code generator
3. Generate C# interfaces
4. Implement service classes based on generated interfaces

**Business Value:**
- Contract-first development approach
- Clear separation of interface and implementation
- Easier to maintain service contracts

**Example Input:**
```
PaymentService @service
  + ProcessPayment
    > PaymentRequest : PaymentDto
    > CustomerInfo : CustomerDto
    < PaymentResult : PaymentResultDto
    < TransactionId : string

  * RefundPayment
    > TransactionId : string
    > RefundAmount : decimal
    < RefundStatus : RefundResultDto
```

**Example Output:**
```csharp
public interface IPaymentService
{
    Task<(PaymentResultDto PaymentResult, string TransactionId)> ProcessPaymentAsync(
        PaymentDto paymentRequest,
        CustomerDto customerInfo,
        CancellationToken cancellationToken = default);

    Task<RefundResultDto> RefundPaymentAsync(
        string transactionId,
        decimal refundAmount,
        CancellationToken cancellationToken = default);
}
```

---

### UC-4: Client SDK Generation

**Actor:** Frontend Developer / Integration Partner

**Goal:** Generate TypeScript/JavaScript client SDK from contracts

**Flow:**
1. Service team publishes contract file
2. Client team runs generator targeting TypeScript
3. Generated SDK includes models and service proxies
4. Client integrates SDK into frontend application

**Business Value:**
- Consistent client APIs across all consumers
- Type safety in TypeScript clients
- Reduced integration errors

---

### UC-5: Contract Validation & Testing

**Actor:** QA Engineer

**Goal:** Validate service implementations match contracts

**Flow:**
1. Parse service contract to extract expected operations
2. Use reflection to verify service implementation
3. Generate test stubs for contract validation
4. Run automated tests to ensure compliance

**Business Value:**
- Ensures services honor their contracts
- Catches contract violations early
- Automated compliance testing

---

## User Journeys

### Journey 1: New Service Development (Contract-First)

**Persona:** Sarah - Backend Developer

**Scenario:** Sarah needs to create a new microservice for customer onboarding

**Steps:**
1. **Design Phase:**
   - Sarah writes CustomerOnboarding.ServiceContract file
   - Defines 3 operations: StartOnboarding, ValidateIdentity, CompleteOnboarding
   - Defines 8 DTOs for the domain model
   - Reviews contract with product owner

2. **Code Generation:**
   - Runs `contractparser generate --input CustomerOnboarding.ServiceContract --output ./Generated`
   - Tool generates:
     - `ICustomerOnboardingService.cs` (interface)
     - 8 DTO classes
     - 3 enum definitions
     - API documentation (markdown)

3. **Implementation:**
   - Sarah implements `CustomerOnboardingService : ICustomerOnboardingService`
   - All types already defined - just write business logic
   - Compiles and runs immediately

4. **Documentation:**
   - Generated markdown published to developer portal
   - Contract file checked into repo alongside code
   - Single source of truth maintained

**Pain Points Solved:**
- ❌ No more manual DTO creation
- ❌ No more interface/implementation drift
- ❌ No more outdated documentation

---

### Journey 2: Legacy Service Documentation

**Persona:** Mike - Technical Writer

**Scenario:** Mike needs to document 50 legacy services

**Steps:**
1. **Contract Extraction:**
   - Mike examines existing service implementations
   - Writes contract files based on actual DTOs/interfaces
   - Validates contracts match implementation

2. **Bulk Documentation:**
   - Runs batch generator across all 50 contracts
   - Generates consistent API documentation
   - Reviews and publishes to wiki

3. **Maintenance:**
   - Developers update contract files when APIs change
   - Documentation auto-regenerates
   - Technical debt reduced

**Pain Points Solved:**
- ❌ No more reverse-engineering API documentation
- ❌ No more inconsistent documentation formats
- ❌ No more documentation drift

---

## Requirements

### Functional Requirements

**FR-1: Parser**
- SHALL parse ServiceContract DSL files using ANTLR4 grammar
- SHALL support services, operations, DTOs, and enumerations
- SHALL handle comments and documentation links
- SHALL validate syntax and report errors

**FR-2: Code Generation - C# DTOs**
- SHALL generate C# classes from @dto definitions
- SHALL include DataContract/DataMember attributes
- SHALL support primitive types, collections, and nested DTOs
- SHALL generate XML documentation from comments

**FR-3: Code Generation - C# Interfaces**
- SHALL generate service interfaces from @service definitions
- SHALL map operations to async methods
- SHALL use tuple returns for multiple output parameters
- SHALL include CancellationToken parameters

**FR-4: Code Generation - TypeScript Models**
- SHALL generate TypeScript interfaces from DTOs
- SHALL map C# types to TypeScript types (string, number, boolean, Date)
- SHALL support generic types (List<T> → Array<T>)

**FR-5: Documentation Generation**
- SHALL generate markdown documentation from contracts
- SHALL include operation descriptions and parameter documentation
- SHALL link to confluence/external docs from contract comments

**FR-6: Contract Validation**
- SHALL validate service implementations against contracts
- SHALL report missing operations or parameter mismatches
- SHALL support integration testing

### Non-Functional Requirements

**NFR-1: Performance**
- SHALL parse contracts in <100ms for files <100KB
- SHALL generate code in <500ms per contract file

**NFR-2: Compatibility**
- SHALL support .NET 10.0+
- SHALL generate code compatible with .NET 9.0+
- SHALL work on Windows, Linux, macOS

**NFR-3: Extensibility**
- SHALL support custom code generation templates
- SHALL allow pluggable generators for new target languages
- SHALL support custom validation rules

**NFR-4: Usability**
- SHALL provide CLI tool for batch processing
- SHALL provide clear error messages with line/column numbers
- SHALL include usage examples and templates

---

## Technical Specifications

### Grammar Version

**Current:** v2 (ServiceContract_v2.g4)

**Features:**
- Service definitions with @service decorator
- Operation definitions with + (read) or * (write) prefix
- Parameter direction: > (input), < (output)
- DTO definitions with @dto decorator
- Enum definitions with @enum decorator and numeric values
- Comment support for documentation
- Type system with generic support (List<T>)
- Numeric literals: binary, octal, hex, decimal

**Known Limitations:**
- Multiple type parameters not supported (e.g., Dictionary<K,V>)
- No nullable type syntax
- No default value syntax for properties
- No attribute/annotation support beyond @service/@dto/@enum

**Proposed Enhancements:**
```
// Multiple type parameters
AccountCache @dto
  Cache : Dictionary<string, AccountDto>  // Currently unsupported

// Nullable types
OptionalField : string?
RequiredField : string!

// Default values
Status : AccountStatus = Active
MaxRetries : int = 3

// Validation attributes
EmailAddress : string [Email, Required]
Age : int [Range(0, 120)]
```

### Example Service Contracts (Real Data)

The incoming directory contains 5 production service contracts from automotive/financial domain:

1. **AccountPortfolioDetail.ServiceContract** (225 LOC)
   - Service: Get portfolio details, leads, available vehicles, customer notes
   - DTOs: 15 complex DTOs with 20-50 properties each
   - Domain: Account management

2. **DealerToCustomerPayment.ServiceContract** (15 LOC)
   - Service: Orchestrate payment processing
   - DTOs: Simple payment DTOs
   - Domain: Payments

3. **PayoffQuote.ServiceContract** (60 LOC)
   - Service: Generate payoff quotes with promotions
   - DTOs: Quote DTOs with promotion support
   - Domain: Finance

4. **ReassignAccount.ServiceContract** (37 LOC)
   - Service: Reassign accounts to advisers
   - DTOs: Account and adviser DTOs
   - Domain: Account management

5. **TaxRateLookup.ServiceContract** (27 LOC)
   - Service: Look up tax rates by location
   - DTOs: Location and tax rate DTOs
   - Domain: Tax calculation

---

## Architecture

### Proposed Components

```
OoBDev.Contracts
├── OoBDev.Contracts (Framework layer)
│   ├── Parser/
│   │   ├── ServiceContractLexer.g4 (ANTLR)
│   │   ├── ServiceContractParser.g4 (ANTLR)
│   │   ├── ServiceContractVisitor.cs
│   │   └── ContractModel/ (AST classes)
│   ├── Validation/
│   │   ├── ContractValidator.cs
│   │   └── Rules/ (validation rules)
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
│
└── OoBDev.Contracts.CodeGen (Extensions layer)
    ├── Generators/
    │   ├── CSharpDtoGenerator.cs
    │   ├── CSharpInterfaceGenerator.cs
    │   ├── TypeScriptGenerator.cs
    │   └── DocumentationGenerator.cs
    ├── Templates/ (Handlebars/Scriban templates)
    └── Cli/
        └── ContractParserCli (tool)
```

### Integration Points

**Existing OoBDev Infrastructure:**
- ANTLR4: Already used in ExpressionCalculator and JsonPath
- Template Engine: Use existing OoBDev.Handlebars for code generation
- Roslyn: Integrate with existing CodeAnalysis for runtime generation
- Schema Framework: Align with existing schema patterns

---

## Dependencies

**ANTLR4:**
- Antlr4.Runtime.Standard 4.13.1+
- Antlr4BuildTasks 12.10.0+

**Template Engine:**
- OoBDev.Handlebars (internal) or
- Scriban (lightweight alternative)

**Code Analysis:**
- Microsoft.CodeAnalysis.CSharp 4.x (if runtime generation needed)

**Testing:**
- MSTest
- FluentAssertions (for validation testing)

---

## Migration Complexity

| Aspect | Complexity | Effort Estimate |
|--------|-----------|-----------------|
| ANTLR Parser | MEDIUM | 40 hours |
| AST Model | LOW | 20 hours |
| C# DTO Generator | MEDIUM | 30 hours |
| C# Interface Generator | MEDIUM | 25 hours |
| TypeScript Generator | MEDIUM | 25 hours |
| Documentation Generator | LOW | 15 hours |
| CLI Tool | LOW | 15 hours |
| Tests | MEDIUM | 40 hours |
| Documentation | LOW | 20 hours |
| **Total** | **MEDIUM-HIGH** | **230 hours (~6 weeks)** |

---

## Decision Points

### Decision 1: Implement Now or Later?

**Options:**
1. **Full Implementation** - 6 weeks effort, HIGH value for contract-first development
2. **Specification Only** - Keep as reference, minimal effort, LOW immediate value
3. **Extract Patterns** - Document use cases, implement later when needed

**Recommendation:** Option 3 (Extract Patterns) - Capture requirements now, implement when contract-first development is prioritized

---

### Decision 2: Grammar Enhancement?

**Options:**
1. **Minimal** - Use v2 grammar as-is
2. **Enhanced** - Add nullable types, default values, validation attributes
3. **Complete** - Full type system with generics, attributes, inheritance

**Recommendation:** Option 2 (Enhanced) - Add most-requested features before implementation

---

### Decision 3: Code Generation Approach?

**Options:**
1. **Template-Based** - Use Handlebars/Scriban templates (flexible, maintainable)
2. **Roslyn-Based** - Generate syntax trees directly (type-safe, complex)
3. **Hybrid** - Templates for structure, Roslyn for validation

**Recommendation:** Option 1 (Template-Based) - Aligns with existing OoBDev patterns, easier to extend

---

## Next Steps (When Prioritized)

1. [ ] Complete grammar v2 with enhanced features
2. [ ] Create comprehensive test suite for grammar
3. [ ] Implement ANTLR parser following ExpressionCalculator pattern
4. [ ] Build AST model classes
5. [ ] Create C# code generators (DTOs + interfaces)
6. [ ] Add TypeScript generator
7. [ ] Build CLI tool
8. [ ] Write comprehensive documentation
9. [ ] Migrate 5 example contracts to tests

---

## Related Documentation

- [Incoming/ContractParser/](../../Incoming/ContractParser/) - Grammar and examples
- [TODO-migrations.md](../../TODO-migrations.md) - Migration tracking
- [docs/migration/](../../docs/migration/) - Migration planning

---

**Status:** Awaiting prioritization decision
