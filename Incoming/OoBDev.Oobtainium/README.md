# OoBDev.Oobtainium

A powerful .NET library for creating dynamic interface proxies with call recording and method binding capabilities. Built on top of .NET's `DispatchProxy`, Oobtainium enables you to create mock instances, intercept method calls, record invocations, and bind custom behavior to interface methods at runtime.

## Features

- **Dynamic Proxy Creation**: Generate proxy instances for any interface without concrete implementations
- **Method Binding**: Bind custom behavior to interface methods using fluent syntax
- **Call Recording**: Automatically record all method invocations with arguments and return values
- **Async Support**: Full support for `Task` and `Task<T>` async methods
- **Generic Methods**: Handle generic method parameters and return types
- **Property & Indexer Support**: Automatic backing store for properties and indexers
- **Dependency Injection**: Seamless integration with Microsoft.Extensions.DependencyInjection
- **Fluent API**: Intuitive builder pattern for configuring bindings
- **.NET 10.0**: Built for the latest .NET platform

## Project Structure

The solution consists of three projects:

- **OoBDev.Oobtainium.Abstractions**: Core interfaces and contracts
- **OoBDev.Oobtainium**: Main implementation library
- **OoBDev.Oobtainium.Tests**: Unit tests and usage examples

## Installation

```bash
# Add reference to your project
dotnet add reference path/to/OoBDev.Oobtainium.csproj
```

## Quick Start

### Simple Example

```csharp
using OoBDev.Oobtainium;

// Create a proxy factory
var factory = new CaptureProxyFactory();

// Configure method bindings
var bindings = new CallBinder()
    .Build<ITargetInterface>()
        .Bind(a => a.ReturnValue(), () => "Hello World");

// Create proxy instance with handler
var instance = factory.Create<ITargetInterface>(
    handler: new CallHandler(bindings.Store)
);

// Use the proxy
var result = instance.ReturnValue(); // Returns "Hello World"
```

### With Call Recording

```csharp
var factory = new CaptureProxyFactory();

// Create instance with call recorder
var instance = factory.CreateWithRecorder<ITargetInterface>();

// Use the interface
instance.VoidReturn();
await instance.VoidReturnAsync();
instance[456] = "Hi!";
var value = instance[456]; // Returns "Hi!"

// Retrieve and examine recorded calls
if (instance.TryGetRecorder(out var recorder))
{
    foreach (var call in recorder)
    {
        Console.WriteLine(call.ToString());
        // Output: OoBDev.Oobtainium.Tests.TestTargets.ITargetInterface::Void VoidReturn()
        // Output: OoBDev.Oobtainium.Tests.TestTargets.ITargetInterface::System.Threading.Tasks.Task VoidReturnAsync()
        // Output: OoBDev.Oobtainium.Tests.TestTargets.ITargetInterface::Void set_Item(Int32, System.String) [456;Hi!]
    }
}
```

### Dynamic Binding (On/Off)

```csharp
var factory = new CaptureProxyFactory();
var instance = factory.Create<ITargetInterface>().AddRecorder();

// Get the binding builder from the instance
var builder = ((IHaveCallBindingStore)instance).Store.Build<ITargetInterface>();

// Initially not bound - returns null
var result1 = instance.ReturnValue(); // null

// Bind behavior
builder.Bind(a => a.ReturnValue(), () => "Hello World");
var result2 = instance.ReturnValue(); // "Hello World"

// Update binding
builder.Bind(a => a.ReturnValue(), () => "Hello World!");
var result3 = instance.ReturnValue(); // "Hello World!"

// Remove binding
builder.Remove(a => a.ReturnValue());
var result4 = instance.ReturnValue(); // null
```

### Dependency Injection Integration

```csharp
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Oobtainium;

var services = new ServiceCollection()
    .AddLogging(logging => logging.AddDebug())

    // Register Oobtainium services
    .AddOobtainium()

    // Register your mocked interface
    .AddTransient(sp =>
        sp.GetRequiredService<ICaptureProxyFactory>()
          .Create<IMyInterface>()
    );

var serviceProvider = services.BuildServiceProvider();

// Configure bindings
var binder = serviceProvider.GetRequiredService<ICallBinder>()
    .Build<IMyInterface>()
        .Bind(a => a.DoWork(), () => Console.WriteLine("Work done"))
        .Bind(a => a.GetValue(), () => Task.FromResult(42));

// Resolve and use
var instance = serviceProvider.GetRequiredService<IMyInterface>();
instance.DoWork();
var value = await instance.GetValue(); // Returns 42
```

## Advanced Usage

### Binding with Parameters

```csharp
var binder = new CallBinder()
    .Build<ITargetInterface>()
        // Bind with parameter access
        .Bind(a => a.InvokeAsync(new { Test = "" }),
              args => new { Test = args[0].ToString() ?? "" })

        // Bind async methods
        .Bind(a => a.VoidReturnAsync(), async args =>
        {
            await Task.Delay(1000);
            Console.WriteLine("Done");
        })

        // Bind with return values
        .Bind(a => a.ReturnValue(), () => Guid.NewGuid().ToString());
```

### Multiple Interface Bindings

```csharp
var binder = new CallBinder()
    .Build<IFirstInterface>()
        .Bind(a => a.Method1(), () => "Result 1")
    .Build<ISecondInterface>()
        .Bind(a => a.Method2(), () => "Result 2");

var factory = new CaptureProxyFactory();
var instance1 = factory.Create<IFirstInterface>(new CallHandler(binder.Store));
var instance2 = factory.Create<ISecondInterface>(new CallHandler(binder.Store));
```

### Generic Method Support

```csharp
var instance = factory.CreateWithRecorder<ITargetInterface>();

// Generic methods work seamlessly
await instance.VoidReturnWithGenericInputAsync(234);
await instance.VoidReturnWithGenericInputAsync("hello");
var result = instance.ReturnWithGenericInput(42); // Returns int
var text = instance.ReturnWithGenericInput("test"); // Returns string
```

## Core Interfaces

### ICaptureProxyFactory
Creates proxy instances for interfaces.

```csharp
public interface ICaptureProxyFactory
{
    T Create<T>(ICallHandler? handler = null);
    T CreateWithRecorder<T>(ICallHandler? handler = null);
}
```

### ICallBinder
Entry point for configuring method bindings.

```csharp
public interface ICallBinder
{
    IBindingBuilder<T> Build<T>();
}
```

### IBindingBuilder<T>
Fluent builder for binding methods to delegates.

```csharp
public interface IBindingBuilder<S>
{
    IBindingBuilder<S> Bind(MethodInfo? method, Delegate? callback);
    IBindingBuilder<U> Build<U>();
    ICallBindingStore Store { get; }
}
```

### ICallRecorder
Records method invocations for inspection and testing.

```csharp
public interface ICallRecorder : IEnumerable<IRecordedCall>
{
    CaptureHandler? Capture { get; }
    void Clear();
}
```

### IRecordedCall
Represents a single recorded method call.

```csharp
public interface IRecordedCall
{
    object Instance { get; }
    Type InstanceAs { get; }
    MethodInfo Method { get; }
    object[] Arguments { get; }
    object? Response { get; }
}
```

## Extension Methods

### Object Extensions

```csharp
// Add call recorder to an existing proxy
var proxy = factory.Create<IMyInterface>();
proxy.AddRecorder();

// Try to get the recorder from a proxy
if (proxy.TryGetRecorder(out var recorder))
{
    // Use recorder
}
```

## Dependencies

- **.NET 10.0**
- **Microsoft.Extensions.DependencyInjection.Abstractions** (v10.0.2)
- **System.ServiceModel.Primitives** (v10.0.652802)
- **Microsoft.Extensions.Logging.Abstractions** (v10.0.2)

## Use Cases

- **Unit Testing**: Create mock implementations for dependency injection
- **Integration Testing**: Record and verify API calls
- **Debugging**: Track method invocations and parameters
- **Prototyping**: Quickly stub out interfaces during development
- **Dynamic Behavior**: Modify interface behavior at runtime
- **Call Interception**: Add cross-cutting concerns without modifying implementations

## Testing

The project includes comprehensive unit tests demonstrating various usage scenarios:

```bash
dotnet test OoBDev.Oobtainium.Tests/OoBDev.Oobtainium.Tests.csproj
```

## How It Works

Oobtainium leverages .NET's `DispatchProxy` to create dynamic proxies. When you call a method on a proxy:

1. The call is intercepted by `CaptureProxy<I>`
2. The `CallHandler` looks up any bound delegate for the method
3. If a binding exists, the delegate is invoked
4. If call recording is enabled, the call is logged to `CallRecorder`
5. The result is type-converted and returned to the caller
6. Properties and indexers use an internal `ConcurrentDictionary` as a backing store

## Comparison with Other Frameworks

### vs. Moq (Mocking Framework)

**Similarities:**
- Both create proxies for interfaces
- Both support method interception and custom behavior
- Both can record method calls
- Both integrate with DI containers

**Key Differences:**

| Feature | Oobtainium | Moq |
|---------|-----------|-----|
| **Primary Use Case** | Runtime behavior modification + testing | Unit testing only |
| **Proxy Technology** | `DispatchProxy` (built-in .NET) | Castle DynamicProxy (IL generation) |
| **Binding Mutability** | Mutable - add/remove bindings at runtime | Immutable - setup is fixed after creation |
| **Shared State** | Supports shared `CallBindingStore` across proxies | Each mock is independent |
| **Class Mocking** | Interfaces only | Interfaces + classes with virtual members |
| **Verification API** | Basic recording only | Rich verification (`.Verify()`, `.VerifyAll()`, times, etc.) |
| **Setup Syntax** | Fluent binding with delegates | `.Setup()` with `.Returns()`, `.Throws()`, `.Callback()` |
| **Performance** | Lightweight (DispatchProxy) | Heavier (IL generation) |
| **Production Use** | Designed for runtime use | Testing only |

**Example Comparison:**

```csharp
// Moq
var mock = new Mock<IService>();
mock.Setup(x => x.GetValue()).Returns(42);
mock.Setup(x => x.DoWork()).Verifiable();
var service = mock.Object;
service.DoWork();
mock.Verify(x => x.DoWork(), Times.Once());

// Oobtainium
var factory = new CaptureProxyFactory();
var binder = new CallBinder()
    .Build<IService>()
        .Bind(x => x.GetValue(), () => 42)
        .Bind(x => x.DoWork(), () => Console.WriteLine("Work"));
var service = factory.CreateWithRecorder<IService>(handler: new CallHandler(binder.Store));
service.DoWork();
if (service.TryGetRecorder(out var recorder))
    Assert.IsTrue(recorder.Any(c => c.Method.Name == "DoWork"));
```

**When to Choose Oobtainium over Moq:**
- You need to modify mock behavior at runtime (add/remove bindings dynamically)
- You want to use mocks in production code for plugin systems or strategy patterns
- You need shared binding configuration across multiple proxy instances
- You prefer using pure `DispatchProxy` without external dependencies

**When to Choose Moq over Oobtainium:**
- Pure unit testing scenarios
- You need rich verification capabilities (`Times.AtLeast()`, `Times.Between()`, etc.)
- You need to mock classes with virtual members
- You want extensive community support and documentation

### vs. NSubstitute & FakeItEasy

Similar to the Moq comparison, these are testing-focused mocking frameworks with immutable setups and rich verification APIs. Oobtainium differs in its runtime mutability and production-readiness.

### vs. AOP Frameworks (PostSharp, Castle DynamicProxy, AspectCore)

**Similarities:**
- Both support method interception
- Both can add cross-cutting concerns
- Both work at runtime (for runtime-based AOP frameworks)

**Key Differences:**

| Feature | Oobtainium | AOP Frameworks |
|---------|-----------|----------------|
| **Primary Purpose** | Proxy creation & behavior binding | Cross-cutting concerns (logging, caching, security) |
| **Target** | Interfaces only | Classes, methods, properties (with attributes/conventions) |
| **Interception Point** | Entire interface implementation | Specific methods/aspects |
| **Weaving** | Runtime only | Compile-time or runtime |
| **Existing Implementations** | Creates new proxies | Wraps/intercepts existing implementations |
| **Configuration** | Programmatic fluent API | Attributes + configuration |
| **Concerns** | General behavior binding | Specific aspects (logging, transactions, etc.) |
| **Learning Curve** | Simple - just proxies & bindings | Steeper - AOP concepts & patterns |

**Example Comparison:**

```csharp
// Castle DynamicProxy (AOP)
var generator = new ProxyGenerator();
var interceptor = new LoggingInterceptor();
var proxy = generator.CreateInterfaceProxyWithTarget<IService>(
    new ServiceImplementation(),
    interceptor
);

// Oobtainium
var factory = new CaptureProxyFactory();
var service = factory.CreateWithRecorder<IService>();
((IHaveCallBindingStore)service).Store.Build<IService>()
    .Bind(x => x.GetValue(), () => {
        Console.WriteLine("Logging...");
        return 42;
    });
```

**When to Choose Oobtainium over AOP:**
- You only need to work with interfaces (no class interception needed)
- You want lightweight proxies without compile-time weaving
- You need dynamic, runtime-modifiable behavior
- You don't have existing implementations to wrap
- You want simple delegate-based behavior binding

**When to Choose AOP over Oobtainium:**
- You need to intercept existing class implementations
- You want compile-time weaving for performance
- You need conventional aspect application (attributes, naming patterns)
- You're implementing standard cross-cutting concerns
- You need to intercept specific methods rather than entire interfaces

### Unique Oobtainium Features

1. **Dynamic Binding Modification**: Change mock behavior after creation
```csharp
var proxy = factory.Create<IService>();
var builder = ((IHaveCallBindingStore)proxy).Store.Build<IService>();
builder.Bind(x => x.GetValue(), () => 1);  // Returns 1
builder.Bind(x => x.GetValue(), () => 2);  // Now returns 2
builder.Remove(x => x.GetValue());         // Now returns null/default
```

2. **Shared Binding Store**: Multiple proxies share the same configuration
```csharp
var store = new CallBindingStore();
var proxy1 = factory.Create<IService1>(new CallHandler(store));
var proxy2 = factory.Create<IService2>(new CallHandler(store));
// Both proxies can be configured through the same store
```

3. **Property/Indexer Backing Store**: Automatic state management
```csharp
var proxy = factory.Create<IService>();
proxy[123] = "value";
Assert.AreEqual("value", proxy[123]);  // Automatically stored
```

4. **Separation of Concerns**: Recording and binding are independent
```csharp
var proxy = factory.Create<IService>();  // No recording
proxy.AddRecorder();                     // Add recording later
```

## Architectural Philosophy

Oobtainium occupies a unique position between mocking frameworks and AOP frameworks:

- **Lighter than AOP**: No compile-time weaving, no complex aspect configuration
- **More flexible than mocking**: Runtime-mutable bindings, production-ready
- **Simpler than both**: Pure delegate-based binding with minimal ceremony

It's ideal for scenarios where you need the flexibility of dynamic proxies without the testing-only constraints of mocking frameworks or the complexity of full AOP solutions.

## vs. Just Using Microsoft DI?

**TL;DR:** For most production scenarios, use concrete implementations with DI. Oobtainium shines when you need runtime behavior modification, call recording, or implementations-less prototyping.

### What DI Can Do (Without Oobtainium)

```csharp
// Register concrete implementations
services.AddScoped<IUserService, UserService>();
services.AddScoped<IUserService, CachedUserService>(); // Swap implementation

// Use decorators for cross-cutting concerns
services.Decorate<IUserService, LoggingUserServiceDecorator>();

// Change behavior via configuration
services.AddScoped<IUserService>(sp =>
    sp.GetRequiredService<IOptions<Settings>>().Value.UseCache
        ? new CachedUserService()
        : new UserService()
);
```

**This works fine when:**
- You have concrete implementations
- Behavior is determined at registration time
- You're okay creating new classes for behavior variations

### What Oobtainium Adds

**1. Runtime Behavior Modification (After Resolution)**

The key advantage: change behavior of already-injected instances.

```csharp
// Standard DI approach
public class FeatureController
{
    private readonly IFeatureService _service;

    public FeatureController(IFeatureService service)
    {
        _service = service; // Behavior is FIXED at this point
    }

    public void EnableBetaMode()
    {
        // With DI: Can't change _service behavior
        // You'd need to request a new instance or use Strategy pattern
    }
}

// Oobtainium approach
public class FeatureController
{
    private readonly IFeatureService _service;
    private readonly ICallBindingStore _bindings;

    public FeatureController(IFeatureService service, ICallBindingStore bindings)
    {
        _service = service;
        _bindings = bindings;
    }

    public void EnableBetaMode()
    {
        // Change behavior of the SAME instance at runtime
        _bindings.Build<IFeatureService>()
            .Bind(x => x.GetFeatures(), () => GetBetaFeatures());
    }

    public void DisableBetaMode()
    {
        _bindings.Build<IFeatureService>()
            .Remove(x => x.GetFeatures());
    }
}
```

**2. No Implementation Required**

```csharp
// DI: Must create concrete class
public class UserService : IUserService
{
    public string GetName() => "John";
    public int GetAge() => 30;
    // ... 20 more methods you need to implement
}

// Oobtainium: Just bind what you need
var binder = new CallBinder()
    .Build<IUserService>()
        .Bind(x => x.GetName(), () => "John")
        .Bind(x => x.GetAge(), () => 30);
    // Other methods return default values automatically
```

**Useful for:**
- Rapid prototyping
- Testing (though Moq is better here)
- Stubbing large interfaces when you only care about a few methods

**3. Built-in Call Recording**

```csharp
// DI: Need to create decorator/wrapper for logging
public class LoggingUserService : IUserService
{
    private readonly IUserService _inner;
    private readonly ILogger _logger;

    public LoggingUserService(IUserService inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public string GetName()
    {
        _logger.LogInformation("GetName called");
        return _inner.GetName();
    }
    // ... repeat for every method
}

// Oobtainium: Recording is built-in
var service = factory.CreateWithRecorder<IUserService>();
service.GetName();
if (service.TryGetRecorder(out var recorder))
{
    foreach (var call in recorder)
        Console.WriteLine($"{call.Method.Name} called with {call.Arguments}");
}
```

**4. Shared Behavior Across Different Interfaces**

```csharp
// DI: Each implementation is separate
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<ISmsService, SmsService>();
// If you want to disable both, you need feature flags or configuration

// Oobtainium: Shared binding store
var store = new CallBindingStore();
services.AddSingleton<ICallBindingStore>(store);
services.AddScoped(sp => factory.Create<IEmailService>(new CallHandler(store)));
services.AddScoped(sp => factory.Create<ISmsService>(new CallHandler(store)));

// Now you can disable all notifications at runtime
store.Build<IEmailService>().Bind(x => x.Send(...), () => {}); // No-op
store.Build<ISmsService>().Bind(x => x.Send(...), () => {}); // No-op
```

### Honest Assessment: When NOT to Use Oobtainium

**Don't use Oobtainium when:**

1. **You have straightforward implementations** - Just use DI
```csharp
// This is fine - you don't need Oobtainium
services.AddScoped<IUserRepository, UserRepository>();
```

2. **Behavior is determined at startup** - Use DI configuration
```csharp
// This is better than Oobtainium
services.AddScoped<ICache>(sp =>
    config.UseRedis ? new RedisCache() : new MemoryCache()
);
```

3. **You need strong typing and compile-time safety**
```csharp
// DI: Compile-time errors if method signature changes
public class UserService : IUserService
{
    public string GetName() => "John"; // Compiler verifies signature
}

// Oobtainium: Runtime errors if you get the binding wrong
.Bind(x => x.GetName(), () => 42); // Compiles but fails at runtime!
```

4. **Performance is critical** - Concrete classes are faster
- DI with concrete implementations: Direct method calls
- Oobtainium: DispatchProxy interception + dictionary lookup

### When Oobtainium Provides Real Value

**Use Oobtainium when:**

1. **Hot-swappable behavior in production**
```csharp
// Feature flags that modify behavior without restart
public void ToggleFeature(string feature, bool enabled)
{
    if (enabled)
        _bindings.Build<IFeatureService>()
            .Bind(x => x.IsEnabled(feature), () => true);
    else
        _bindings.Build<IFeatureService>()
            .Remove(x => x.IsEnabled(feature));
}
```

2. **Plugin systems with runtime loading**
```csharp
// Load plugin behavior from configuration without concrete classes
public void LoadPlugin(PluginConfig config)
{
    _bindings.Build<IPlugin>()
        .Bind(x => x.Execute(), () => RunScript(config.ScriptPath));
}
```

3. **Debugging/auditing in specific environments**
```csharp
// Only record calls in staging environment
if (env.IsStaging())
{
    var service = factory.CreateWithRecorder<IPaymentService>();
    // All calls are logged for debugging
}
else
{
    var service = factory.Create<IPaymentService>();
    // No overhead in production
}
```

4. **Testing when you don't want Moq dependency**
```csharp
// Lightweight testing without third-party mocking framework
var service = factory.Create<IUserService>();
((IHaveCallBindingStore)service).Store
    .Build<IUserService>()
    .Bind(x => x.GetUser(123), () => new User { Id = 123 });
```

5. **A/B testing different implementations**
```csharp
// Switch between algorithm implementations at runtime
public void SetAlgorithm(string version)
{
    var algo = version switch
    {
        "v1" => AlgorithmV1,
        "v2" => AlgorithmV2,
        _ => AlgorithmDefault
    };

    _bindings.Build<IRecommendationService>()
        .Bind(x => x.GetRecommendations(user), args => algo((User)args[0]));
}
```

### The Verdict

**DI frameworks** solve the problem of **what** implementation to use (determined at registration).

**Oobtainium** solves the problem of **changing behavior at runtime** (determined after resolution).

They're complementary, not competitive. You can (and often should) use both:
- Use DI for standard service registration and dependency management
- Use Oobtainium for services that need runtime behavior modification

**Most production code doesn't need Oobtainium.** But when you need runtime mutability, it's significantly cleaner than alternatives like:
- Recreating the entire DI container
- Complex Strategy pattern hierarchies
- Manual decorator chains
- Reflection-based method invocation

## License

Copyright Out-of-Band Development, LLC 2021

## Author

Matthew Whited

## Repository

https://github.com/OutOfBandDevelopment/oobtainium/

## Contributing

Contributions are welcome! Please submit pull requests or issues to the GitHub repository.
