# Incomming/BotChat Feature Mapping

**Version:** 1.0
**Last Updated:** 2026-01-13
**Source:** Incomming/BotChat
**Target:** Main OoBDev Codebase
**Status:** 🔍 INVESTIGATION COMPLETE - Sample application analysis

---

## Executive Summary

**BotChat** is a **sample/demo console application** (12 files, ~393 LOC) that demonstrates how to build an interactive chat application using Microsoft SemanticKernel with Ollama local LLMs.

### Classification

- **Type:** Exe (Console Application)
- **Purpose:** Sample/Reference Implementation
- **Target Framework:** .NET 9.0 (already upgraded)
- **Status:** Working demonstration of SemanticKernel + Ollama integration

### Critical Finding

BotChat is NOT a library to migrate - it's a **working example** that shows developers how to:
1. Integrate Ollama with SemanticKernel
2. Build interactive CLI chat applications
3. Use the generic hosted service pattern
4. Register kernel plugins

### Relationship to Main Codebase

| Aspect | BotChat | Main (OoBDev.Ollama) | Relationship |
|--------|---------|---------------------|--------------|
| **Purpose** | Sample/demo CLI app | Production library | BotChat demonstrates usage |
| **Project Type** | Exe (Console) | Class library | Different targets |
| **LOC** | ~393 | ~526 | Main is larger |
| **Dependencies** | SK 1.32.0 | SK 1.40.0-alpha | Main is newer |
| **Integration** | Direct Ollama factories | Abstracted providers | Main is more robust |

**Key Insight:** BotChat appears to be an **earlier prototype** that informed the design of the production `OoBDev.Ollama` library. Some patterns were refined and extracted into the main codebase.

---

## Project Structure

### BotChat Architecture

```
BotChat/
├── Program.cs (303 bytes)
    Entry point: Sets up Host, DI container, runs application
├── BotChat.csproj
    .NET 9.0 Console Application
├── ServiceCollectionExtensions.cs (4,010 bytes)
    DI setup for Ollama, Kernel, and Plugins
├── BotChatOptions.cs (153 bytes)
    Configuration for external BotChat service (stub)
├── appsettings.json (530 bytes)
    Configuration file with Ollama settings
├── HostRunner/
│   ├── IRunner.cs
│       Interface for background task execution
│   ├── RunnerHost<T>.cs
│       Generic hosted service container
│   └── KernelRunner.cs (2,873 bytes)
│       Interactive CLI chat loop using SemanticKernel
├── KernelHost/
│   └── IKernelPlugIn.cs
│       Interface for kernel plugin registration
└── Ollama/
    ├── IOllamaApiClientFactory.cs
        Factory interface for OllamaApiClient
    ├── OllamaApiClientFactory.cs (788 bytes)
        Factory implementation with API key support
    ├── IOllamaServiceClientFactory.cs
        Factory for SemanticKernel text generation service
    ├── OllamaServiceClientFactory.cs
        Wraps OllamaApiClient in SK service
    └── OllamaOptions.cs (265 bytes)
        Configuration: Model, Endpoint, ApiKey
```

**Total:** 12 C# files, ~393 LOC

---

## Feature Breakdown

### 1. Interactive CLI Chat Loop

**Location:** `HostRunner/KernelRunner.cs`

**Purpose:** Demonstrates real-time chat with Ollama LLM

**Features:**
- Console-based user input/output
- Chat history management
- Commands: `/done`, `/exit` to quit
- Integration with SemanticKernel chat completion service
- Automatic function calling support (FunctionChoiceBehavior.Auto)
- Comprehensive error handling and logging

**Key Code:**
```csharp
while (true)
{
    Console.Write("User> ");
    var prompt = Console.ReadLine()?.Trim();
    if (string.Compare(prompt, "/done", ignoreCase: true) == 0)
        break;

    chatHistory.AddUserMessage(prompt);
    var result = await chatCompletionService.GetChatMessageContentAsync(
       chatHistory,
       executionSettings: executionSettings,
       kernel: _kernel);

    _assistant.LogInformation("Assistant>{response}", result.Content);
    chatHistory.AddAssistantMessage(result.Content);
}
```

**Migration Value:** **MEDIUM** - Good reference for building CLI tools

---

### 2. Generic Hosted Service Pattern

**Location:** `HostRunner/RunnerHost<T>.cs`, `HostRunner/IRunner.cs`

**Purpose:** Generic background service pattern for .NET Host

**Architecture:**
```csharp
public interface IRunner
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public class RunnerHost<T> : BackgroundService where T : IRunner
{
    private readonly T _runner;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _runner.ExecuteAsync(stoppingToken);
    }
}
```

**Usage:**
```csharp
services.AddHostedService<RunnerHost<KernelRunner>>();
```

**Migration Value:** **HIGH** - Reusable pattern for any background task

**Migration Decision:** **EXTRACT** to `OoBDev.System.Hosting` or similar

---

### 3. Kernel Plugin Infrastructure

**Location:** `KernelHost/IKernelPlugIn.cs`

**Purpose:** Interface for registering custom kernel plugins

**Architecture:**
```csharp
public interface IKernelPlugIn { }

public static IServiceCollection AddKernelPlugIn<T>(this IServiceCollection services)
    where T : class, IKernelPlugIn
{
    services.AddTransient<IKernelPlugIn, T>();
    return services;
}
```

**Comparison with Main:**
- Main codebase has similar pattern in `OoBDev.SemanticKernel.Abstractions/IKernelPlugIn.cs`
- **100% identical interface**
- ServiceCollection extension exists in main: `OoBDev.SemanticKernel/ServiceCollectionExtensions.cs`

**Migration Decision:** **ALREADY EXISTS** in main codebase

---

### 4. Ollama Integration Factories

**Location:** `Ollama/` directory

#### 4.1 OllamaApiClientFactory

**BotChat Implementation:**
```csharp
public interface IOllamaApiClientFactory
{
    OllamaApiClient Create(); // Returns concrete type
}

public class OllamaApiClientFactory : IOllamaApiClientFactory
{
    public OllamaApiClient Create()
    {
        var client = new OllamaApiClient(_options.Value.Endpoint, _options.Value.Model);
        if (!string.IsNullOrWhiteSpace(_options.Value.ApiKey))
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.Value.ApiKey}");
        return client;
    }
}
```

**Main OoBDev.Ollama Implementation:**
```csharp
public interface IOllamaApiClientFactory
{
    IOllamaApiClient Build(); // Returns interface
}
```

**Key Differences:**
- BotChat: Returns `OllamaApiClient` (concrete), method named `Create()`
- Main: Returns `IOllamaApiClient` (interface), method named `Build()`
- BotChat: Has **API key/authorization support** ⭐
- Main: No authentication support (missing feature)

**Migration Decision:** **ENHANCE** main OoBDev.Ollama with API key support

---

#### 4.2 OllamaServiceClientFactory

**BotChat Implementation:**
```csharp
public interface IOllamaServiceClientFactory
{
    ITextGenerationService GetTextGenerationService();
}
```

**Purpose:** Wraps `OllamaApiClient` in SemanticKernel's `ITextGenerationService`

**Main OoBDev.Ollama:**
- Does NOT have this specific factory
- Uses different approach: `OllamaChatProvider` implements `IChatProvider`
- More abstracted and flexible

**Migration Decision:** **NOT NEEDED** - Main has better abstraction

---

### 5. Ollama Configuration

**BotChat Options:**
```csharp
public record OllamaOptions
{
    public required string Model { get; init; }
    public required Uri Endpoint { get; init; }
    public string? ApiKey { get; init; } // Optional API key
}
```

**Main OoBDev.Ollama Options:**
```csharp
public record OllamaApiClientOptions
{
    public required Uri Url { get; init; }
    public required string DefaultModel { get; init; }
    // No API key support
}
```

**Migration Decision:** **ENHANCE** main options with API key support

---

### 6. Dependency Injection Setup

**BotChat ServiceCollectionExtensions:**
- `AddKernelPlugIns()` - Register plugins
- `AddOllamaServices(configuration)` - Configure Ollama
- `AddKernelHostExtensions(configuration)` - Full setup
- `AddBotChatClient(configuration)` - External client (stub/incomplete)

**Main OoBDev.Ollama ServiceCollectionExtensions:**
- `TryAddOllamaServices(configuration, url)` - Configure with optional URL check
- Health check integration
- Multiple provider registrations (keyed services)
- More robust and production-ready

**Migration Decision:** **MAIN IS BETTER** - No action needed

---

## Comparison Matrix: BotChat vs OoBDev.Ollama

### Feature Parity

| Feature | BotChat | OoBDev.Ollama | Winner | Notes |
|---------|---------|---------------|--------|-------|
| **Factory Pattern** | ✓ | ✓ | TIE | Different approaches |
| **API Key Support** | ✓ | ✗ | **BotChat** | Missing in main ⭐ |
| **Chat Completion** | ✓ (via SK) | ✓ | MAIN | More abstracted |
| **Embedding Support** | ✗ | ✓ | **MAIN** | IEmbeddingProvider |
| **Health Checks** | ✗ | ✓ | **MAIN** | Production feature |
| **Extension Methods** | ✗ | ✓ | **MAIN** | Rich helpers |
| **Model Mapping** | ✗ | ✓ (stubbed) | **MAIN** | Infrastructure |
| **Keyed Services** | ✗ | ✓ | **MAIN** | Multiple providers |
| **Interactive CLI** | ✓ | ✗ | **BotChat** | Demo feature |
| **Plugin System** | ✓ | ✓ | TIE | Main has it too |
| **Generic Runner** | ✓ | ✗ | **BotChat** | Useful pattern |
| **Documentation** | ✗ | ✓ | **MAIN** | XML docs + README |
| **SemanticKernel Version** | 1.32.0 | 1.40.0-alpha | **MAIN** | Newer version |

### Overall Assessment

**Main (OoBDev.Ollama) is superior** for production use:
- Better abstractions
- More features (embeddings, health checks, extensions)
- Newer dependencies
- Professional documentation

**BotChat has value** as:
- Reference implementation
- Learning resource
- CLI demo showing interactive chat
- Source of missing features (API key support, generic runner pattern)

---

## Dependencies Analysis

### BotChat Dependencies

```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />
<PackageReference Include="Microsoft.SemanticKernel" Version="1.32.0" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.Ollama" Version="1.32.0-alpha" />
```

**Status:** .NET 9.0, up-to-date Microsoft.Extensions, older SemanticKernel

### Main OoBDev.Ollama Dependencies

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.7" />
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="9.0.7" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.Ollama" Version="1.40.0-alpha" />
<PackageReference Include="OllamaSharp" Version="5.3.3" />
```

**Status:** .NET 9.0, latest Microsoft.Extensions, newer SemanticKernel

**Compatibility:** Good - Both on .NET 9.0, compatible versions

---

## Migration Decision Matrix

| Component | Priority | Complexity | Recommendation |
|-----------|----------|------------|----------------|
| **Interactive CLI Chat** | MEDIUM | LOW | ARCHIVE as reference |
| **Generic RunnerHost<T>** | HIGH | LOW | EXTRACT to framework |
| **IKernelPlugIn** | N/A | N/A | ALREADY EXISTS in main |
| **API Key Support** | HIGH | LOW | ENHANCE main OoBDev.Ollama |
| **OllamaOptions with ApiKey** | HIGH | LOW | ENHANCE main options |
| **Ollama Factories** | LOW | N/A | MAIN IS BETTER - No action |
| **Sample Application** | HIGH | MEDIUM | POLISH and publish as demo |

---

## Unique Features Worth Extracting

### 1. API Key/Authorization Support ⭐⭐⭐

**Priority:** HIGH
**Effort:** LOW

**Current BotChat Implementation:**
```csharp
if (!string.IsNullOrWhiteSpace(_options.Value.ApiKey))
    ollamaClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.Value.ApiKey}");
```

**Target:** `src/ExternalServices/Ollama/OoBDev.Ollama/OllamaApiClientFactory.cs`

**Action:**
1. Add `ApiKey` property to `OllamaApiClientOptions`
2. Apply authorization header in factory if provided
3. Add XML documentation explaining premium Ollama services

---

### 2. Generic RunnerHost<T> Pattern ⭐⭐⭐

**Priority:** HIGH
**Effort:** LOW

**Current Implementation:**
```csharp
public interface IRunner
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public class RunnerHost<T> : BackgroundService where T : IRunner
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await _runner.ExecuteAsync(stoppingToken);
}
```

**Target:** `src/Framework/OoBDev.System.Hosting/` or `src/Framework/OoBDev.AspNetCore.Mvc/Hosting/`

**Action:**
1. Extract `IRunner`, `RunnerHost<T>` to new location
2. Add XML documentation
3. Add unit tests
4. Update BotChat to reference extracted version

**Benefits:**
- Reusable pattern for CLI tools
- Simplifies background service creation
- Type-safe runner pattern

---

### 3. Interactive Chat CLI Pattern ⭐⭐

**Priority:** MEDIUM
**Effort:** LOW

**Purpose:** Reference implementation for CLI tools

**Action:**
1. Polish BotChat as official demo
2. Move to `src/Tools/OoBDev.Ollama.Cli/` or similar
3. Improve UX:
   - Add help command
   - Add model switching
   - Add conversation save/load
   - Add colored output
4. Document as official sample

---

## Recommended Migration Approach

### Option 1: ARCHIVE as Reference (RECOMMENDED)

**Action:**
1. Create comprehensive README.md in `Incomming/BotChat/`
2. Document what BotChat demonstrates
3. Explain relationship to OoBDev.Ollama
4. Provide setup and usage instructions
5. Keep as reference for developers building Ollama apps

**Pros:**
- Preserves valuable reference implementation
- Zero migration effort
- Developers can learn from it
- Can be enhanced later if needed

**Cons:**
- Not integrated into main codebase
- May become outdated

---

### Option 2: ENHANCE as Official Demo Tool

**Action:**
1. Extract `RunnerHost<T>` to framework
2. Update BotChat to use main OoBDev.Ollama library
3. Polish UX (help, colors, features)
4. Move to `src/Tools/OoBDev.Ollama.Cli/`
5. Add to solution and CI/CD
6. Publish as official demo application

**Pros:**
- Official example for OoBDev.Ollama
- Maintained and up-to-date
- Demonstrates best practices
- Useful for developers

**Cons:**
- MEDIUM effort to polish
- Ongoing maintenance required

---

### Option 3: EXTRACT Patterns Only

**Action:**
1. Extract `RunnerHost<T>` to `OoBDev.System.Hosting`
2. Add API key support to main OoBDev.Ollama
3. Delete BotChat directory
4. Reference main OoBDev.Ollama in documentation

**Pros:**
- Extract reusable patterns
- Clean up Incomming/
- No sample app maintenance

**Cons:**
- Lose reference implementation
- Developers don't have working example

---

## Questions Requiring Answers

### 1. Sample Application Strategy

**Question:** What should we do with BotChat?

**Options:**
- **A.** Archive in Incomming/ with comprehensive README (LOW effort)
- **B.** Enhance and move to Tools/ as official demo (MEDIUM effort)
- **C.** Extract patterns only and delete (LOW effort)

**Recommendation:** **Option A** (Archive) or **Option B** (Enhance as demo)

---

### 2. RunnerHost<T> Pattern Location

**Question:** Where should the generic runner pattern live?

**Options:**
- **A.** `src/Framework/OoBDev.System.Hosting/` - Generic hosting utilities
- **B.** `src/Framework/OoBDev.AspNetCore.Mvc/Hosting/` - ASP.NET Core specific
- **C.** Keep in BotChat only (if archiving)

**Recommendation:** **Option A** - Useful for any hosted service, not ASP.NET specific

---

### 3. API Key Support Priority

**Question:** Should we add API key support to OoBDev.Ollama?

**Options:**
- **A.** Yes - Add immediately (enables premium Ollama services)
- **B.** Yes - Add in future enhancement
- **C.** No - Not needed for local Ollama instances

**Recommendation:** **Option A** - Simple addition, enables cloud/premium Ollama

---

### 4. SemanticKernel Version

**Question:** Should we upgrade BotChat to SemanticKernel 1.40.0-alpha?

**Context:** Main OoBDev.Ollama uses 1.40.0-alpha, BotChat uses 1.32.0

**Options:**
- **A.** Yes - Keep consistent with main (if keeping BotChat)
- **B.** No - Leave as-is if archiving

**Recommendation:** **Option A** if Option 2 (Enhance as demo) chosen, otherwise **Option B**

---

## Related Documents

- [BotChat Migration Plan](./botchat-migration-plan.md) - Detailed execution steps
- [Framework Feature Mapping](./framework-feature-mapping.md) - Related framework analysis
- [Incomming Checklist](../../Incomming/CHECKLIST.md) - Overall tracking

---

## Change Log

- 2026-01-13 v1.0: Initial BotChat feature mapping created
