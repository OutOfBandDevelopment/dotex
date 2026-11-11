# OoBDev.SemanticKernel.Abstractions

The contracts the other projects use to plug into Semantic Kernel without depending on a connector. The namespace is `OoBDev.SemanticKernel`.

**Table 1 — Types**

| Type | Role |
|------|------|
| `IKernelPlugIn` | Marker for a class whose `[KernelFunction]` methods become a kernel plugin. |
| `AddKernelPlugIn<T>()` | `IServiceCollection` extension that registers `T` as a transient `IKernelPlugIn`. |
| `IChatProvider` | `OneShotAsync(prompt)` sends one prompt and returns the reply text (or `null`); implemented by `OllamaChatProvider` in `OoBDev.Ollama`. |
| `KernelGlobal.Name` | The service key `"kernel"` for the keyed `Kernel` and `IChatCompletionService`. |

```csharp
public sealed class WeatherPlugIn : IKernelPlugIn
{
    [KernelFunction("forecast"), Description("get the forecast for a city")]
    public Task<string> ForecastAsync(string city) => Task.FromResult("sunny");
}

services.AddKernelPlugIn<WeatherPlugIn>();
```

The kernel itself is built by `services.TryAddSemanticKernelPlugins()` in [OoBDev.SemanticKernel](../OoBDev.SemanticKernel/README.SemanticKernel.md).
