# OData Query — API Design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

**Status:** Built · **Date:** 2026-11-11 (REQ-001 to REQ-010 in `OoBDev.AspNetCore.OData`, covered by its Simulate tests)

## Contents

- [Types](#types)
- [Configuration](#configuration)
- [Registration](#registration)

## Types

```csharp
namespace OoBDev.AspNetCore.OData;

public class ODataQueryLimits
{
    public int DefaultPageSize { get; set; } = 50;
    public int MaxTop { get; set; } = 100;
    public int MaxNodeCount { get; set; } = 100;
    public bool AllowExpand { get; set; }
    public int MaxExpansionDepth { get; set; } = 2;
}

public sealed class OoBDevEnableQueryAttribute : EnableQueryAttribute { }

public class ODataQueryBodyMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context);
}
```

## Configuration

```json
{ "OoBDev": { "OData": { "DefaultPageSize": 50, "MaxTop": 100, "MaxNodeCount": 100, "AllowExpand": false, "MaxExpansionDepth": 2 } } }
```

## Registration

- `services.TryAddODataQueryServices(configuration)` binds and validates `OoBDev:OData` and registers the OData query services.
- `app.UseODataQueryBody()` after `UseHttpQueryVerb()` and before routing.

[Testing strategy →](testing-strategy.md)
