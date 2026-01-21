# Tools - CLI Utilities Collection

**Status:** 📋 SPECIFICATION - Requirements Gathering
**Priority:** MIXED - See individual tool priorities
**Last Updated:** 2026-01-20

---

## Overview

The Incoming/Tools directory contains 4 standalone CLI applications for various development and hardware tasks. This document captures their features, use cases, and migration recommendations.

**Tools Found:**
1. OoBDev.De5000.Ble.Cli - Bluetooth LE device scanner (Hardware)
2. OoBDev.ImageConverter.Cli - Image format batch converter (Utility)
3. OobDev.BulkLlm.GroqNet.Cli - Cloud LLM code documentation generator (Developer Tool)
4. OobDev.BulkLlm.Ollama.Cli - Local LLM code documentation generator (Developer Tool)

---

## Tool 1: OoBDev.De5000.Ble.Cli

### Overview

**Purpose:** Bluetooth Low Energy device scanner for discovering GATT services and characteristics on BLE hardware

**Target Hardware:** DE-5000 LCR meter with wireless serial bridge

**Status:** Incomplete/Early-stage

**Files:** 4 | **LOC:** 75 | **Framework:** net8.0-windows10.0.19041.0

### Use Cases

**UC-1: Hardware Device Discovery**
- **Actor:** Hardware Engineer / QA Tester
- **Goal:** Enumerate available Bluetooth LE devices
- **Flow:**
  1. Run CLI tool
  2. Tool scans for BLE devices
  3. Displays list of discovered devices with names and IDs
  4. Engineer identifies target device

**UC-2: GATT Service Inspection**
- **Actor:** Embedded Systems Developer
- **Goal:** Inspect GATT services, characteristics, and descriptors
- **Flow:**
  1. Run tool with device filter
  2. Tool connects to device
  3. Enumerates all services
  4. Lists all characteristics per service
  5. Displays descriptor information
  6. Developer documents protocol for integration

### Requirements

**FR-1:** Scan for Bluetooth LE devices within range
**FR-2:** Display device names, IDs, and signal strength
**FR-3:** Connect to specific device by ID
**FR-4:** Enumerate GATT services
**FR-5:** List characteristics for each service
**FR-6:** Display characteristic properties (read, write, notify)
**FR-7:** Read descriptor values

### Dependencies

- InTheHand.BluetoothLE v4.0.37
- Windows 10 SDK 19041+

### Migration Decision

**Priority:** LOW - Specialized hardware, incomplete implementation

**Options:**
1. **Complete & Migrate** - Finish implementation, add to ExternalServices/Hardware
2. **Archive** - Keep as reference for future BLE integration
3. **Delete** - Remove if no BLE hardware integration planned

**Recommendation:** Archive - Specialized use case, incomplete state

---

## Tool 2: OoBDev.ImageConverter.Cli

### Overview

**Purpose:** Batch converts HEIC and NEF image files to JPEG format

**Status:** Basic but functional

**Files:** 2 | **LOC:** 32 | **Framework:** net10.0

### Use Cases

**UC-1: Bulk Image Format Conversion**
- **Actor:** Photographer / Content Manager
- **Goal:** Convert camera RAW files to web-friendly JPEG
- **Flow:**
  1. Place images in source directory
  2. Run ImageConverter.Cli
  3. Tool processes all .heic and .nef files
  4. Outputs JPEG files with quality settings
  5. Skips already-converted files

**UC-2: Photography Workflow Automation**
- **Actor:** Professional Photographer
- **Goal:** Automate post-processing workflow
- **Flow:**
  1. Import photos from camera (HEIC/NEF format)
  2. Run converter as part of import script
  3. JPEG versions ready for client preview
  4. Original RAW files preserved

### Requirements

**FR-1:** Support HEIC format input
**FR-2:** Support NEF (Nikon RAW) format input
**FR-3:** Output JPEG with configurable quality
**FR-4:** Skip files that already have output
**FR-5:** Process directories recursively
**FR-6:** Report progress and errors

### Current Limitations

- Hard-coded input path: `C:\Images\Too Close`
- No configuration file support
- No CLI arguments for paths/quality
- No logging or progress reporting
- No error handling for malformed images

### Dependencies

- Magick.NET-Q16-x64 v14.10.1 (ImageMagick wrapper)

### Migration Decision

**Priority:** LOW - Similar functionality exists in DocumentConverter.Cli

**Overlap:** Main codebase has `OoBDev.DocumentConverter.Cli` with document conversion. Both use external libraries for format conversion.

**Options:**
1. **Merge into DocumentConverter** - Add image formats to existing converter
2. **Keep Separate** - Maintain as standalone image-specific tool
3. **Archive** - Keep as reference, too specialized

**Recommendation:** Archive - Functionality can be added to DocumentConverter if needed

---

## Tool 3: OobDev.BulkLlm.GroqNet.Cli

### Overview

**Purpose:** Batch processes source code directories using Groq's cloud LLM API to generate AI-assisted documentation

**Status:** Functional for API interaction, file generation incomplete

**Files:** 5 | **LOC:** 189 | **Framework:** net8.0

### Use Cases

**UC-1: Automated Code Documentation**
- **Actor:** Software Developer
- **Goal:** Generate documentation for legacy code using AI
- **Flow:**
  1. Developer prepares Handlebars template (GenerateDocumentationForThisCode.md.hbs)
  2. Configures source code directory
  3. Runs BulkLlm.GroqNet.Cli
  4. Tool reads each source file (<10KB)
  5. Sends code + template to Groq LLaMA3-8b API
  6. Saves AI-generated documentation as markdown
  7. Saves raw JSON responses

**UC-2: API Documentation Generation**
- **Actor:** Technical Writer
- **Goal:** Generate API documentation for microservices
- **Flow:**
  1. Point tool at service contract files
  2. Use custom template for API docs
  3. LLM generates OpenAPI/Swagger descriptions
  4. Export to documentation portal

**UC-3: Code Review Automation**
- **Actor:** Team Lead
- **Goal:** Get AI-assisted code review comments
- **Flow:**
  1. Run tool on recent commit diffs
  2. Template asks LLM for code review feedback
  3. LLM identifies potential issues, suggests improvements
  4. Export feedback for PR comments

### Requirements

**FR-1:** Read source code files from directory (recursive)
**FR-2:** Filter files by size (<10KB) and type (exclude PDFs)
**FR-3:** Compile Handlebars templates with file context
**FR-4:** Send templated prompts to Groq API
**FR-5:** Parse LLM responses (markdown code blocks)
**FR-6:** Save responses as files (disabled - needs completion)
**FR-7:** Support custom templates via Handlebars

**NFR-1:** API key management via environment variable
**NFR-2:** Rate limiting / throttling for API calls
**NFR-3:** Error handling for API failures
**NFR-4:** Progress reporting for large batches

### Dependencies

- Handlebars.Net v2.1.6
- Handlebars.Net.Helpers v2.4.5
- Handlebars.Net.Helpers.Humanizer v2.4.5
- GroqNet v1.0.1

### Current Limitations

- File extraction from LLM responses disabled (lines 83-89 commented)
- Hard-coded directory paths (not configurable via CLI)
- No retry logic for API failures
- No rate limiting (could hit API quotas)
- No cost tracking (Groq API has usage limits)

### Migration Decision

**Priority:** MEDIUM-HIGH - Valuable developer tool, needs refactoring

**Overlap:** Main codebase has:
- `OoBDev.TemplateEngine.Cli` - Uses Handlebars templating
- `OoBDev.FileRagEngine.Cli` - File scanning + templating + external processing

**Options:**
1. **Refactor & Consolidate** - Extract LLM provider pattern, merge with existing tools
2. **Standalone Migration** - Migrate as-is, fix limitations
3. **Archive** - Keep as reference for LLM integration patterns

**Recommendation:** Option 1 (Refactor & Consolidate) - Create ILlmProvider abstraction, consolidate Groq and Ollama variants

---

## Tool 4: OobDev.BulkLlm.Ollama.Cli

### Overview

**Purpose:** Identical to GroqNet variant but uses local Ollama instance for LLM processing

**Status:** Functional for local LLM integration, file generation incomplete

**Files:** 7 | **LOC:** 178 | **Framework:** net8.0

### Use Cases

**All use cases from GroqNet.Cli apply, PLUS:**

**UC-4: Offline Code Documentation**
- **Actor:** Developer in air-gapped environment
- **Goal:** Generate documentation without cloud API
- **Flow:**
  1. Run local Ollama instance with LLaMA model
  2. Use BulkLlm.Ollama.Cli
  3. All processing happens locally
  4. No internet required, no API costs

**UC-5: Unit Test Generation**
- **Actor:** Developer
- **Goal:** Generate unit tests for existing code
- **Template:** GenerateUnitTests.md.hbs
- **Flow:**
  1. Point tool at source files
  2. LLM generates MSTest + Moq unit tests
  3. Developer reviews and integrates tests

**UC-6: Frontend Framework Migration**
- **Actor:** Frontend Developer
- **Goal:** Convert Angular components to React
- **Template:** Angular2React.md.hbs
- **Flow:**
  1. Run tool on Angular component files
  2. LLM generates equivalent React components
  3. Developer validates and integrates

### Prompt Templates

**3 Handlebars templates included:**

1. **GenerateDocumentationForThisCode.md.hbs**
   - Same as GroqNet variant
   - Generates code documentation

2. **GenerateUnitTests.md.hbs** (UNIQUE)
   - Generates MSTest + Moq unit tests
   - Follows OoBDev testing patterns

3. **Angular2React.md.hbs** (UNIQUE)
   - Converts Angular components to React
   - Handles component lifecycle, props, state

### Requirements

Same as GroqNet.Cli, PLUS:

**FR-8:** Connect to local Ollama instance
**FR-9:** Support multiple prompt templates
**FR-10:** Template selection via configuration

**NFR-5:** Local network dependency (Ollama server)
**NFR-6:** No API costs (local inference)

### Dependencies

- Handlebars.Net v2.1.6
- Handlebars.Net.Helpers v2.4.5
- Handlebars.Net.Helpers.Humanizer v2.4.5
- OllamaSharp v2.0.10

### Current Limitations

- File extraction disabled (lines 72-78 commented)
- Hard-coded Ollama endpoint: `http://192.168.1.170:11434`
- No fallback if Ollama server unavailable
- No model selection (hardcoded to default)

### Migration Decision

**Priority:** MEDIUM-HIGH - Valuable for offline/local LLM usage

**Duplication:** 95% identical to GroqNet.Cli - only provider differs

**Options:**
1. **Consolidate with GroqNet** - Single tool with provider selection
2. **Keep Separate** - Maintain cloud vs. local distinction
3. **Archive** - Keep as reference

**Recommendation:** Option 1 (Consolidate) - Create unified LLM tool with pluggable providers

---

## Consolidated Migration Plan: BulkLlm Tools

### Problem

- Two duplicate implementations (GroqNet + Ollama)
- Incomplete file generation (disabled code in both)
- Hard-coded configuration (paths, endpoints, API keys)
- Tight coupling to specific providers

### Proposed Solution

**Create Abstraction Layer:**

```csharp
// Framework: OoBDev.Extensions.Llm
public interface ILlmProvider
{
    Task<string> GetCompletionAsync(string prompt, CancellationToken ct);
}

public class GroqLlmProvider : ILlmProvider
{
    // Uses GroqNet library
}

public class OllamaLlmProvider : ILlmProvider
{
    // Uses OllamaSharp library
}

// Future providers:
// - OpenAI
// - Anthropic Claude
// - Azure OpenAI
// - Local LLaMA via llama.cpp
```

**Refactored CLI:**

```csharp
// Single tool: OoBDev.LlmCodeGen.Cli
// Configuration via appsettings.json:
{
  "Provider": "Groq",  // or "Ollama", "OpenAI", etc.
  "GroqSettings": { "ApiKey": "..." },
  "OllamaSettings": { "Endpoint": "http://localhost:11434" },
  "SourcePath": "./src",
  "OutputPath": "./docs",
  "Templates": ["GenerateDocumentation", "GenerateTests"]
}
```

### Implementation Tasks

**Phase 1: Abstraction (20 hours)**
- [ ] Create `OoBDev.Extensions.Llm` project
- [ ] Define `ILlmProvider` interface
- [ ] Implement `GroqLlmProvider`
- [ ] Implement `OllamaLlmProvider`
- [ ] Add ServiceCollection extensions

**Phase 2: Orchestrator (15 hours)**
- [ ] Create file processing engine
- [ ] Integrate with existing TemplateEngine
- [ ] Complete file extraction from responses
- [ ] Add error handling and retry logic
- [ ] Add progress reporting

**Phase 3: CLI Tool (10 hours)**
- [ ] Create unified CLI: `OoBDev.LlmCodeGen.Cli`
- [ ] Add configuration support (appsettings.json + CLI args)
- [ ] Remove hard-coded paths
- [ ] Add provider selection
- [ ] Add template selection

**Phase 4: Testing (10 hours)**
- [ ] Unit tests for providers
- [ ] Integration tests with mock LLM responses
- [ ] End-to-end tests
- [ ] 80%+ coverage

**Phase 5: Documentation (5 hours)**
- [ ] README with usage examples
- [ ] Template authoring guide
- [ ] Provider configuration guide
- [ ] Migration guide from old tools

**Total Effort:** 60 hours (~1.5 weeks)

---

## Summary & Recommendations

| Tool | Status | Priority | Effort | Recommendation |
|------|--------|----------|--------|----------------|
| De5000.Ble.Cli | Incomplete | LOW | 40h (complete) or 2h (archive) | **Archive** - Specialized, incomplete |
| ImageConverter.Cli | Basic | LOW | 2h (archive) | **Archive** - Overlaps with DocumentConverter |
| BulkLlm.GroqNet.Cli | Functional | MEDIUM-HIGH | Part of 60h consolidation | **Consolidate** with Ollama |
| BulkLlm.Ollama.Cli | Functional | MEDIUM-HIGH | Part of 60h consolidation | **Consolidate** with GroqNet |

**Recommended Actions:**
1. ✅ Archive De5000.Ble.Cli and ImageConverter.Cli
2. ✅ Consolidate both BulkLlm tools into unified `OoBDev.LlmCodeGen` solution
3. ✅ Create `ILlmProvider` abstraction for future extensibility
4. ✅ Complete file generation feature (currently disabled)
5. ✅ Add proper configuration management

---

## Related Documentation

- [Incoming/Tools/](../../Incoming/Tools/) - Source code
- [TODO-migrations.md](../../TODO-migrations.md) - Migration tracking
- [Features/ContractParser/](../ContractParser/) - Related code generation feature

---

**Status:** Awaiting prioritization decision on BulkLlm consolidation
