# HealthCheckOptionsFactory Documentation

## Introduction

The `HealthCheckOptionsFactory` class is responsible for creating and configuring health check options for an ASP.NET Core application. This class is part of the `OoBDev.AspNetCore.Mvc.Diagnostics.HealthChecks` namespace.

## Class Diagram

```plantuml
@startuml
class HealthCheckOptionsFactory {
  - HealthCheckOptions Create()
}
class HealthCheckOptions {
  - Predicate
  - ResponseWriter
  - ResultStatusCodes
}
HealthCheckOptionsFactory -->> HealthCheckOptions
@enduml
```

## Component Model

The `HealthCheckOptionsFactory` class is a factory that creates instances of the `HealthCheckOptions` class. The `HealthCheckOptions` class contains properties for configuring health checks, such as a predicate for filtering results and a response writer for customizing the response.

```plantuml
@startuml
component HealthCheckOptionsFactory {
  .. HealthCheckOptions Create()
}
component HealthCheckOptions {
  <<Predicate>>: Predicate
  <<ResponseWriter>>: ResponseWriter
  <<ResultStatusCodes>>: ResultStatusCodes
}
HealthCheckOptionsFactory -> HealthCheckOptions: returns
@enduml
```

## Sequence Diagram

The `HealthCheckOptionsFactory` class is used to create a new instance of the `HealthCheckOptions` class. This instance is then used to configure health checks.

```plantuml
@startuml
actor HealthCheckOptionsFactory
node HealthCheckOptions
node HealthCheckOptions instance

HealthCheckOptionsFactory ->> HealthCheckOptions: Create()
HealthCheckOptions ->> HealthCheckOptions instance: returns
HealthCheckOptions instance ->> HealthCheckOptions: Configure health checks
@enduml
```

## Code Documentation

### HealthCheckOptionsFactory Class

The `HealthCheckOptionsFactory` class contains a single method called `Create`, which returns a new instance of the `HealthCheckOptions` class.

```csharp
public class HealthCheckOptionsFactory
{
    public static HealthCheckOptions Create()
    {
        return new()
        {
            // Configuration for the health check
        };
    }
}
```

### HealthCheckOptions Class

The `HealthCheckOptions` class contains properties for configuring health checks.

```csharp
public class HealthCheckOptions
{
    public Func<HealthCheckEvaluation, bool> Predicate { get; set; }
    public ResponseWriter ResponseWriter { get; set; }
    public Dictionary<HealthStatus, int> ResultStatusCodes { get; set; }
}
```

### Create() Method

The `Create()` method returns a new instance of the `HealthCheckOptions` class.

```csharp
public static HealthCheckOptions Create()
{
    return new()
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            // Custom response writer logic
        },
        ResultStatusCodes = new Dictionary<HealthStatus, int>()
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    };
}
```

### ResponseWriter Delegate

The `ResponseWriter` delegate is called to customize the response for the health check.

```csharp
ResponseWriter = async (context, report) =>
{
    // Custom response writer logic
},
```

The `ResponseWriter` delegate is called with two parameters: an `HttpContext` instance and an `IHealthReports` instance. The delegate is responsible for writing the response to the `HttpContext.FindAsync` method.

### ResultStatusCodes Dictionary

The `ResultStatusCodes` dictionary maps health statuses to HTTP status codes.

```csharp
ResultStatusCodes = new Dictionary<HealthStatus, int>()
{
    [HealthStatus.Healthy] = StatusCodes.Status200OK,
    [HealthStatus.Degraded] = StatusCodes.Status200OK,
    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
}
```

This dictionary is used to determine the HTTP status code to return for each health status.