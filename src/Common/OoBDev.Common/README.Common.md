# OoBDev.Common

Aggregator package for the framework implementations that need no web host or third-party service. It references the `Common.Abstractions` package and the Framework projects, and its single entry point registers them.

## Use

```csharp
services.TryCommonExtensions(configuration, builder: null);   // namespace OoBDev.Common; builder is optional in release builds
```

`TryCommonExtensions(configuration, SystemExtensionBuilder?)` calls `TryAddSystemExtensions` (the system services, shaped by the builder) and then registers message queueing, communication queues, send/receive, identity, search, documents, common data services, Semantic Kernel plugins and caching. Each call uses `TryAdd*`, so earlier registrations win. In Debug builds the builder parameter is required (rule: the `#if DEBUG` required-parameter pattern), in Release it defaults to null.

Use [OoBDev.Common.Complete](../OoBDev.Common.Complete/README.Common.Complete.md) for the ASP.NET Core, hosting, external service and hardware parts as well.
