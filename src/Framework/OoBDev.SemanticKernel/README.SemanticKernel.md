# OoBDev.SemanticKernel

Two small Semantic Kernel plugins and the registration that builds the `Kernel` from every registered `IKernelPlugIn`. The contracts are in [OoBDev.SemanticKernel.Abstractions](../OoBDev.SemanticKernel.Abstractions/README.SemanticKernel.Abstractions.md).

## Registration

```csharp
services.TryAddSemanticKernelPlugins();
```

This adds `TimePlugIn` and `CurrentUserPlugIn` as `IKernelPlugIn`, and registers two transient keyed services under the key `KernelGlobal.Name` (`"kernel"`):

- `Kernel`: created with all registered `IKernelPlugIn` instances as plugins (so plugins added with `AddKernelPlugIn<T>()` are included).
- `IChatCompletionService`: taken from that kernel, so the application must register a chat completion connector (for example an Ollama or OpenAI one) separately.

```csharp
var kernel = provider.GetRequiredKeyedService<Kernel>(KernelGlobal.Name);
```

## Plugins

**Table 1 — Kernel functions**

| Function | Plugin | Returns |
|----------|--------|---------|
| `current_time` | `TimePlugIn` | The local time as a `DateTimeOffset` from the registered `TimeProvider` (the system clock by default). |
| `adjusted_time` | `TimePlugIn` | The UTC time plus an offset given as a .NET `TimeSpan` string (for example `-05:00:00`). |
| `current_user` | `CurrentUserPlugIn` | `Environment.UserName` of the process. |

Every call is logged at Information (the time values at Debug).
