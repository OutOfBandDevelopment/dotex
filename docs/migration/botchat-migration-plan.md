# BotChat Migration Plan

**Version:** 1.0
**Last Updated:** 2026-01-13
**Source:** Incomming/BotChat
**Type:** Sample/Demo Application
**Status:** 🔍 INVESTIGATION COMPLETE

---

## Executive Summary

BotChat is a **sample console application** demonstrating how to build interactive chat applications using Microsoft SemanticKernel with Ollama. This plan outlines three migration options with detailed steps for each.

**Recommended Approach:** **Option 1 (Archive)** or **Option 2 (Enhance as Official Demo)**

---

## Migration Options

### Option 1: ARCHIVE as Reference ⭐ RECOMMENDED (Low Effort)
### Option 2: ENHANCE as Official Demo Tool (Medium Effort)
### Option 3: EXTRACT Patterns Only (Low Effort)

---

## Option 1: ARCHIVE as Reference Implementation

**Effort:** LOW
**Maintenance:** MINIMAL
**Outcome:** Preserve as learning resource

### Benefits
- Keeps valuable reference implementation
- Developers can learn from working example
- Zero migration effort
- Can be enhanced later if needed
- No ongoing maintenance burden

### Drawbacks
- Not integrated into main codebase
- May become outdated over time
- Developers need to find it in Incomming/

---

### Phase 1: Documentation

**Effort:** 1-2 hours

- [ ] **Create Comprehensive README.md**
  - [ ] Create `Incomming/BotChat/README.md`
  - [ ] Add project description and purpose
  - [ ] Explain relationship to `OoBDev.Ollama`
  - [ ] List prerequisites (Ollama installed, .NET 9.0 SDK)
  - [ ] Add setup instructions
  - [ ] Add configuration guide (appsettings.json)
  - [ ] Add usage examples
  - [ ] Document commands (/done, /exit)
  - [ ] Add architecture diagram
  - [ ] List key files and their purposes
  - [ ] Add "What You Can Learn" section
  - [ ] Link to main OoBDev.Ollama documentation

- [ ] **Add Code Comments**
  - [ ] Add XML documentation to public types
  - [ ] Add inline comments explaining patterns
  - [ ] Document the generic RunnerHost<T> pattern
  - [ ] Explain kernel plugin registration

- [ ] **Create Architectural Diagram**
  - [ ] Create diagram showing DI flow
  - [ ] Show relationship between components
  - [ ] Illustrate chat loop flow

---

### Phase 2: Enhancement Opportunities (Optional)

**Effort:** 2-4 hours

If keeping as reference, consider minor improvements:

- [ ] **Update Dependencies**
  - [ ] Upgrade SemanticKernel to 1.40.0-alpha (match main)
  - [ ] Verify compatibility with latest OllamaSharp

- [ ] **Add More Examples**
  - [ ] Add sample plugin implementation
  - [ ] Add example appsettings.json configurations
  - [ ] Add Dockerfile for containerization

- [ ] **Improve Error Messages**
  - [ ] Add helpful error messages when Ollama not running
  - [ ] Add connection retry logic
  - [ ] Add model availability check

---

### Phase 3: Integration References

**Effort:** 30 minutes

- [ ] **Update Main Documentation**
  - [ ] Add link to BotChat in main README.md
  - [ ] Add to "Examples" section in OoBDev.Ollama README
  - [ ] Add to Incomming/CHECKLIST.md as "ARCHIVED"

- [ ] **Update TODO.md**
  - [ ] Mark BotChat as "ARCHIVED"
  - [ ] Document decision and rationale
  - [ ] Link to BotChat README

---

## Option 2: ENHANCE as Official Demo Tool

**Effort:** MEDIUM
**Maintenance:** ONGOING
**Outcome:** Polished official example

### Benefits
- Official demonstration of OoBDev.Ollama
- Maintained and up-to-date
- Demonstrates best practices
- Useful for developer onboarding
- Professional quality

### Drawbacks
- Requires polish and testing
- Ongoing maintenance required
- Need to keep in sync with OoBDev.Ollama updates

---

### Phase 1: Extract Reusable Patterns

**Effort:** 2-3 hours

- [ ] **Extract RunnerHost<T> Pattern**
  - [ ] Create `src/Framework/OoBDev.System.Hosting/` project
  - [ ] Move `IRunner.cs` to new project
  - [ ] Move `RunnerHost<T>.cs` to new project
  - [ ] Change namespace to `OoBDev.System.Hosting`
  - [ ] Add XML documentation
  - [ ] Add unit tests for RunnerHost<T>
  - [ ] Update BotChat to reference new project

- [ ] **Enhance OoBDev.Ollama with API Key Support**
  - [ ] Add `ApiKey` property to `OllamaApiClientOptions`
  - [ ] Update `OllamaApiClientFactory.Build()` to apply auth header
  - [ ] Add XML documentation for ApiKey usage
  - [ ] Add unit tests for authentication
  - [ ] Update README with API key examples

---

### Phase 2: Update BotChat to Use Main Library

**Effort:** 2-3 hours

- [ ] **Remove Duplicate Code**
  - [ ] Delete `BotChat/Ollama/` directory (use main OoBDev.Ollama)
  - [ ] Add project reference to `OoBDev.Ollama`
  - [ ] Update namespace imports
  - [ ] Update ServiceCollectionExtensions to use main library

- [ ] **Update Dependencies**
  - [ ] Upgrade SemanticKernel to 1.40.0-alpha
  - [ ] Add reference to extracted OoBDev.System.Hosting
  - [ ] Remove redundant packages

- [ ] **Verify Functionality**
  - [ ] Test chat loop works correctly
  - [ ] Test with multiple Ollama models
  - [ ] Test error handling

---

### Phase 3: Polish UX

**Effort:** 4-6 hours

- [ ] **Add Enhanced Commands**
  - [ ] `/help` - Show available commands
  - [ ] `/model <name>` - Switch Ollama model
  - [ ] `/models` - List available models
  - [ ] `/save <filename>` - Save conversation to file
  - [ ] `/load <filename>` - Load conversation from file
  - [ ] `/clear` - Clear chat history
  - [ ] `/system <message>` - Set system prompt
  - [ ] `/quit` or `/exit` - Exit application (already exists)

- [ ] **Improve Output**
  - [ ] Add colored console output (Spectre.Console?)
  - [ ] Add timestamp to messages
  - [ ] Add token count display
  - [ ] Add response time display
  - [ ] Format markdown in responses
  - [ ] Add progress indicator during generation

- [ ] **Add Configuration**
  - [ ] Support multiple Ollama instances in config
  - [ ] Add default model selection
  - [ ] Add max tokens configuration
  - [ ] Add temperature configuration
  - [ ] Support loading prompts from files

---

### Phase 4: Move to Tools

**Effort:** 1-2 hours

- [ ] **Create New Project Location**
  - [ ] Create `src/Tools/OoBDev.Ollama.Cli/`
  - [ ] Move BotChat files to new location
  - [ ] Rename project: `BotChat.csproj` → `OoBDev.Ollama.Cli.csproj`
  - [ ] Update namespace: `BotChat` → `OoBDev.Ollama.Cli`
  - [ ] Update assembly name and description

- [ ] **Add to Solution**
  - [ ] Add project to OoBDev.sln
  - [ ] Add to Tools solution folder
  - [ ] Configure build order/dependencies

- [ ] **CI/CD Integration**
  - [ ] Add to .github/workflows/dotnet.yml
  - [ ] Configure as optional build (path filters)
  - [ ] Add packaging for releases

---

### Phase 5: Documentation

**Effort:** 2-3 hours

- [ ] **Create Comprehensive README**
  - [ ] Add `src/Tools/OoBDev.Ollama.Cli/README.md`
  - [ ] Installation instructions
  - [ ] Configuration guide
  - [ ] Command reference
  - [ ] Usage examples
  - [ ] Troubleshooting section
  - [ ] Architecture overview

- [ ] **Add to Main Documentation**
  - [ ] Add to main README.md Tools section
  - [ ] Add to OoBDev.Ollama README as example
  - [ ] Create tutorial: "Building Ollama Apps with OoBDev"
  - [ ] Add screenshots/GIFs of CLI in action

- [ ] **Code Documentation**
  - [ ] Add XML doc comments to all public types
  - [ ] Add inline comments explaining patterns
  - [ ] Document extension points for customization

---

### Phase 6: Testing

**Effort:** 2-3 hours

- [ ] **Create Test Project**
  - [ ] Create `OoBDev.Ollama.Cli.Tests`
  - [ ] Test command parsing
  - [ ] Test chat history management
  - [ ] Test conversation save/load
  - [ ] Mock Ollama responses for testing

- [ ] **Manual Testing**
  - [ ] Test all commands
  - [ ] Test with multiple Ollama models
  - [ ] Test error scenarios (Ollama not running, etc.)
  - [ ] Test on Windows, Linux, macOS

- [ ] **Performance Testing**
  - [ ] Test with long conversations
  - [ ] Test memory usage
  - [ ] Test response streaming

---

### Phase 7: Packaging

**Effort:** 1-2 hours

- [ ] **Create Distribution**
  - [ ] Configure for dotnet tool install
  - [ ] Create NuGet package metadata
  - [ ] Add icon and branding
  - [ ] Test global tool installation

- [ ] **Release Preparation**
  - [ ] Create CHANGELOG.md
  - [ ] Add version numbering
  - [ ] Create release notes template

---

## Option 3: EXTRACT Patterns Only

**Effort:** LOW
**Maintenance:** MINIMAL
**Outcome:** Clean extraction, delete sample

### Benefits
- Extract reusable patterns to framework
- Clean up Incomming/ directory
- No sample app maintenance burden
- Enhance main OoBDev.Ollama

### Drawbacks
- Lose working reference implementation
- Developers don't have example to learn from
- May need to recreate sample later

---

### Phase 1: Extract RunnerHost<T>

**Effort:** 1-2 hours

- [ ] **Create Hosting Project**
  - [ ] Create `src/Framework/OoBDev.System.Hosting/`
  - [ ] Create `OoBDev.System.Hosting.csproj`
  - [ ] Add to OoBDev.sln

- [ ] **Migrate Pattern**
  - [ ] Copy `IRunner.cs` to new project
  - [ ] Copy `RunnerHost<T>.cs` to new project
  - [ ] Update namespace: `BotChat.HostRunner` → `OoBDev.System.Hosting`
  - [ ] Add XML documentation
  - [ ] Add generic constraints documentation

- [ ] **Create Tests**
  - [ ] Create `OoBDev.System.Hosting.Tests` project
  - [ ] Test RunnerHost<T> initialization
  - [ ] Test runner execution
  - [ ] Test cancellation handling
  - [ ] Test error propagation

---

### Phase 2: Enhance OoBDev.Ollama

**Effort:** 1-2 hours

- [ ] **Add API Key Support**
  - [ ] Update `OollamaApiClientOptions`:
    ```csharp
    public string? ApiKey { get; init; }
    ```
  - [ ] Update `OllamaApiClientFactory.Build()`:
    ```csharp
    if (!string.IsNullOrWhiteSpace(options.ApiKey))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
    }
    ```
  - [ ] Add XML documentation explaining premium services
  - [ ] Add unit tests for authentication

- [ ] **Update Documentation**
  - [ ] Add API key usage to README
  - [ ] Add examples for cloud/premium Ollama
  - [ ] Document when API keys are needed

---

### Phase 3: Update Documentation

**Effort:** 30 minutes

- [ ] **Document Extraction**
  - [ ] Update TODO.md with extraction results
  - [ ] Update Incomming/CHECKLIST.md
  - [ ] Document where patterns were moved
  - [ ] Link to new OoBDev.System.Hosting documentation

- [ ] **Create Migration Guide**
  - [ ] Document for users who might have used BotChat
  - [ ] Show how to use OoBDev.Ollama + OoBDev.System.Hosting instead
  - [ ] Provide code examples

---

### Phase 4: Cleanup

**Effort:** 5 minutes

- [ ] **Delete BotChat**
  - [ ] Backup if desired: `tar -czf botchat-backup-$(date +%Y%m%d).tar.gz Incomming/BotChat/`
  - [ ] Delete `Incomming/BotChat/` directory
  - [ ] Verify deletion

- [ ] **Final Documentation**
  - [ ] Add deletion note to migration docs
  - [ ] Update CHANGELOG if exists

---

## Recommended Approach: Decision Tree

```
START
  │
  ├─ Do you want developers to have a working Ollama chat example?
  │   ├─ YES → Go to Option 2 (ENHANCE as Official Demo)
  │   └─ NO  → Continue
  │
  ├─ Is the RunnerHost<T> pattern valuable for other tools?
  │   ├─ YES → Go to Option 3 (EXTRACT Patterns Only)
  │   └─ NO  → Continue
  │
  └─ Want to keep BotChat for future reference?
      ├─ YES → Go to Option 1 (ARCHIVE)
      └─ NO  → Go to Option 3 (DELETE after extraction)
```

---

## Effort Summary

| Option | Total Effort | Ongoing Maintenance |
|--------|--------------|-------------------|
| **Option 1: Archive** | 2-6 hours | Minimal |
| **Option 2: Enhance** | 14-22 hours | Ongoing |
| **Option 3: Extract** | 2-4 hours | Minimal |

---

## Implementation Checklist

### Pre-Migration Tasks
- [ ] Review all three options
- [ ] Decide on approach (consult with team if needed)
- [ ] Verify Ollama is installed for testing
- [ ] Backup Incomming/BotChat if desired

### Post-Migration Tasks
- [ ] Update Incomming/CHECKLIST.md with decision
- [ ] Update TODO.md with completion status
- [ ] Test all extracted patterns (if Option 2 or 3)
- [ ] Update main documentation
- [ ] Announce changes to team (if applicable)

---

## Related Documents

- [Feature Mapping](./botchat-feature-mapping.md) - Comprehensive BotChat analysis
- [Incomming Checklist](../../Incomming/CHECKLIST.md) - Overall project tracking
- [Framework Migration Plan](./framework-migration-plan.md) - Related framework work

---

## Change Log

- 2026-01-13 v1.0: Initial BotChat migration plan created
