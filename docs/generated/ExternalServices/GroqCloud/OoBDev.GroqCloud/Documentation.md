Here is the documentation for the given source code files, including class diagrams in PlantUML:

**Class Diagrams:**

First, let's create the class diagrams for the given source code files. We can use PlantUML to create these diagrams.

GroqCloudApiClientFactory:
```plantuml
@startuml
class GroqCloudApiClientFactory {
  - IOptions<GroqCloudApiClientOptions> _options
  + GroqClient Build()
}

interface IGroqCloudApiClientFactory {
  + GroqClient Build()
}

@enduml
```
GroqCloudApiClientOptions:
```plantuml
@startuml
class GroqCloudApiClientOptions {
  + string? ApiKey
  + string Model
}

@enduml
```
GroqCloudHealthCheck:
```plantuml
@startuml
class GroqCloudHealthCheck {
  - GroqClient _client
  + HealthCheckResult CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
}

interface IHealthCheck {
  + HealthCheckResult CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
}

@enduml
```
GroqCloudMessageCompletion:
```plantuml
@startuml
class GroqCloudMessageCompletion {
  - GroqClient _client
  - IGroqCloudModelMapper _mapper
  + Task<string> GetCompletionAsync(string modelName, string prompt)
}

interface IMessageCompletion {
  + Task<string> GetCompletionAsync(string modelName, string prompt)
}

@enduml
```
GroqCloudModelMapper:
```plantuml
@startuml
class GroqCloudModelMapper {
  + IGroqCloudModelMapper.Map(CompletionRequest request)
  + IGroqCloudModelMapper.Map(GroqChatCompletions response)
}

interface IGroqCloudModelMapper {
  + GroqChatHistory Map(CompletionRequest request)
  + CompletionResponse Map(GroqChatCompletions response)
}

@enduml
```
ServiceCollectionExtensions:
```plantuml
@startuml
class ServiceCollectionExtensions {
  + static IServiceCollection TryAddGroqCloudServices(IServiceCollection services, IConfiguration configuration, string groqCloudApiClientOptionSection)
}

class IServiceCollection {
  - IServiceCollection AddHealthChecks()
  - IServiceCollection AddCheck<GroqCloudHealthCheck>("groqCloud")
  - IServiceCollection TryAddTransient<IGroqCloudApiClientFactory, GroqCloudApiClientFactory>()
  - IServiceCollection TryAddTransient<IGroqCloudModelMapper, GroqCloudModelMapper>()
  - IServiceCollection TryAddTransient<IMessageCompletion, GroqCloudMessageCompletion>()
  - IServiceCollection TryAddTransient(sp => ActivatorUtilities.CreateInstance<GroqCloudMessageCompletion>(sp))
  - IServiceCollection TryAddTransient(sp => sp.GetRequiredService<IGroqCloudApiClientFactory>().Build())
  - IServiceCollection TryAddKeyedTransient<IMessageCompletion, GroqCloudMessageCompletion>("GroqCloud")
}

@enduml
```
**Documentation:**

Here is the documentation for the given source code files:
```markdown
**GroqCloudApiClientFactory**

The `GroqCloudApiClientFactory` class is responsible for creating instances of the `GroqCloudApiClient`. It takes an instance of `IOptions<GroqCloudApiClientOptions>` in its constructor and provides a `Build` method that returns a new instance of the `GroqCloudApiClient`.

**GroqCloudApiClientOptions**

The `GroqCloudApiClientOptions` class represents the configurable options for the `GroqCloudApiClient`. It has two properties: `ApiKey` and `Model`, which represent the API key and model name for the `GroqCloudApiClient`, respectively.

**GroqCloudHealthCheck**

The `GroqCloudHealthCheck` class is responsible for checking the health of the `GroqCloudApiClient`. It takes an instance of `GroqClient` in its constructor and provides a `CheckHealthAsync` method that returns a `HealthCheckResult` indicating the health status of the `GroqCloudApiClient`.

**GroqCloudMessageCompletion**

The `GroqCloudMessageCompletion` class is responsible for generating message completions using the `GroqCloudApiClient`. It takes instances of `GroqClient` and `IGroqCloudModelMapper` in its constructor and provides a `GetCompletionAsync` method that returns a `Task<string>` representing the computed completion.

**GroqCloudModelMapper**

The `GroqCloudModelMapper` class is responsible for mapping `CompletionRequest` objects to `GroqChatHistory` objects and vice versa. It implements the `IGroqCloudModelMapper` interface and provides two methods: `Map` and `Map`, which perform the mapping.

**ServiceCollectionExtensions**

The `ServiceCollectionExtensions` class provides extension methods for configuring services related to `GroqCloud`. It provides a `TryAddGroqCloudServices` method that configures services for `GroqCloud` and adds instances of `IGroqCloudApiClient