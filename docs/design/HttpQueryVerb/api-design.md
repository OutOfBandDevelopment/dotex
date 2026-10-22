# HTTP QUERY Verb — API Design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

**Status:** Draft · **Date:** 2026-10-22

## Contents

- [Types](#types)
- [Registration](#registration)

## Types

```csharp
namespace OoBDev.AspNetCore.Mvc.Middleware;

public interface IQueryVerbFeature
{
    string OriginalMethod { get; }   // "QUERY" or "POST"
}

public class QueryVerbMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context);
}
```

Header: `X-HTTP-Method-Override: QUERY`. The feature is set on `HttpContext.Features` only for rewritten requests, so a handler can tell a fallback from a real `GET`.

## Registration

`app.UseHttpQueryVerb()` in `ApplicationBuilderExtensions`, before `UseRouting()` (or first in minimal hosting). It is not part of `UseAspNetCoreExtensionMiddleware`, which runs after routing.

[Testing strategy →](testing-strategy.md)
