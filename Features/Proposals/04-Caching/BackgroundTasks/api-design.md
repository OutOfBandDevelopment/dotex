# Platform-Agnostic Background Tasks - API Design

**Epic:** 04 - Distributed Caching
**Feature:** Platform-Agnostic Background Tasks
**Last Updated:** 2026-01-22

---

## API Overview

Platform-agnostic background tasks with two primary interfaces:
1. **IBackgroundTask** - Task business logic
2. **IBackgroundTaskScheduler** - Platform-specific scheduling

---

## Core Interfaces

### IBackgroundTask

```csharp
namespace OoBDev.Framework.BackgroundTasks;

/// <summary>
/// Platform-agnostic background task.
/// </summary>
public interface IBackgroundTask
{
    /// <summary>
    /// Task identifier (unique per task type).
    /// </summary>
    string TaskId { get; }

    /// <summary>
    /// Executes task logic (platform-agnostic).
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);
}
```

---

### IBackgroundTaskScheduler

```csharp
namespace OoBDev.Framework.BackgroundTasks;

/// <summary>
/// Platform-agnostic task scheduler.
/// </summary>
public interface IBackgroundTaskScheduler
{
    /// <summary>
    /// Schedules recurring task with cron expression.
    /// </summary>
    Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression);

    /// <summary>
    /// Schedules one-time task.
    /// </summary>
    Task ScheduleOnceAsync(IBackgroundTask task, DateTimeOffset executeAt);

    /// <summary>
    /// Cancels scheduled task.
    /// </summary>
    Task CancelAsync(string taskId);

    /// <summary>
    /// Gets all scheduled tasks.
    /// </summary>
    Task<IEnumerable<ScheduledTaskInfo>> GetScheduledTasksAsync();
}
```

---

### ScheduledTaskInfo

```csharp
namespace OoBDev.Framework.BackgroundTasks;

/// <summary>
/// Information about scheduled task.
/// </summary>
public class ScheduledTaskInfo
{
    public string TaskId { get; set; }
    public string? CronExpression { get; set; }
    public DateTimeOffset? NextRun { get; set; }
    public DateTimeOffset? LastRun { get; set; }
    public TaskStatus Status { get; set; }
}

public enum TaskStatus
{
    Scheduled,
    Running,
    Completed,
    Failed,
    Cancelled
}
```

---

## Usage Examples

### Example 1: Cache Warming Task

```csharp
using OoBDev.Framework.BackgroundTasks;

public class CacheWarmingTask : IBackgroundTask
{
    private readonly IProductService _productService;
    private readonly ICatalogService _catalogService;
    private readonly ILogger<CacheWarmingTask> _logger;

    public string TaskId => "cache-warming";

    public CacheWarmingTask(
        IProductService productService,
        ICatalogService catalogService,
        ILogger<CacheWarmingTask> logger)
    {
        _productService = productService;
        _catalogService = catalogService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting cache warming");

        try
        {
            // Warm categories
            _ = await _catalogService.GetCategoriesAsync();

            // Warm featured products
            _ = await _productService.GetFeaturedProductsAsync();

            // Warm common products
            var commonIds = new[] { 1, 2, 3, 5, 10 };
            foreach (var id in commonIds)
            {
                ct.ThrowIfCancellationRequested();
                _ = await _productService.GetProductAsync(id);
            }

            _logger.LogInformation("Cache warming completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warming failed");
            throw;
        }
    }
}
```

---

### Example 2: Cache Eviction Task

```csharp
public class CacheEvictionTask : IBackgroundTask
{
    private readonly ICacheService _cache;
    private readonly ILogger<CacheEvictionTask> _logger;

    public string TaskId => "cache-eviction";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting cache eviction");

        var evicted = await _cache.EvictExpiredEntriesAsync();

        _logger.LogInformation("Evicted {Count} entries", evicted);
    }
}
```

---

### Example 3: Scheduling with ASP.NET Core

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddSingleton<IProductService, ProductService>();
        services.AddSingleton<ICatalogService, CatalogService>();

        // Register tasks
        services.AddSingleton<CacheWarmingTask>();
        services.AddSingleton<CacheEvictionTask>();

        // Register hosted service scheduler
        services.AddSingleton<IBackgroundTaskScheduler, HostedServiceTaskScheduler>();
    }

    public void Configure(IApplicationBuilder app)
    {
        var scheduler = app.ApplicationServices.GetRequiredService<IBackgroundTaskScheduler>();

        // Schedule cache warming every hour
        var warmingTask = app.ApplicationServices.GetRequiredService<CacheWarmingTask>();
        scheduler.ScheduleRecurringAsync(warmingTask, "0 * * * *");

        // Schedule cache eviction daily at 2 AM
        var evictionTask = app.ApplicationServices.GetRequiredService<CacheEvictionTask>();
        scheduler.ScheduleRecurringAsync(evictionTask, "0 2 * * *");
    }
}
```

---

### Example 4: Scheduling with Quartz.NET

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register tasks
        services.AddSingleton<CacheWarmingTask>();

        // Register Quartz
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Register Quartz scheduler
        services.AddSingleton<IBackgroundTaskScheduler, QuartzTaskScheduler>();
    }

    public async Task Configure(IApplicationBuilder app)
    {
        var scheduler = app.ApplicationServices.GetRequiredService<IBackgroundTaskScheduler>();
        var warmingTask = app.ApplicationServices.GetRequiredService<CacheWarmingTask>();

        await scheduler.ScheduleRecurringAsync(warmingTask, "0 * * * *");
    }
}
```

---

### Example 5: Azure Functions

```csharp
// Azure Function (generated or manual)
public class CacheWarmingFunction
{
    private readonly CacheWarmingTask _task;

    public CacheWarmingFunction(CacheWarmingTask task)
    {
        _task = task;
    }

    [FunctionName("CacheWarming")]
    public async Task Run(
        [TimerTrigger("0 * * * *")] TimerInfo timer,
        CancellationToken ct)
    {
        await _task.ExecuteAsync(ct);
    }
}
```

---

### Example 6: AWS Lambda

```csharp
// AWS Lambda handler
public class CacheWarmingHandler
{
    private readonly CacheWarmingTask _task;

    public CacheWarmingHandler()
    {
        // Initialize DI
        var services = new ServiceCollection();
        services.AddSingleton<CacheWarmingTask>();
        var provider = services.BuildServiceProvider();

        _task = provider.GetRequiredService<CacheWarmingTask>();
    }

    public async Task HandleAsync(ScheduledEvent evt, ILambdaContext context)
    {
        await _task.ExecuteAsync(context.CancellationToken);
    }
}
```

---

### Example 7: Hangfire

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register tasks
        services.AddSingleton<CacheWarmingTask>();

        // Register Hangfire
        services.AddHangfire(config =>
            config.UseMemoryStorage());
        services.AddHangfireServer();

        // Register Hangfire scheduler
        services.AddSingleton<IBackgroundTaskScheduler, HangfireTaskScheduler>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseHangfireDashboard();

        var scheduler = app.ApplicationServices.GetRequiredService<IBackgroundTaskScheduler>();
        var warmingTask = app.ApplicationServices.GetRequiredService<CacheWarmingTask>();

        scheduler.ScheduleRecurringAsync(warmingTask, "0 * * * *");
    }
}
```

---

### Example 8: One-Time Scheduled Task

```csharp
public class DataExportTask : IBackgroundTask
{
    public string TaskId => "data-export";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        // Export data logic
        await _exportService.ExportDataAsync();
    }
}

// Schedule one-time task for tomorrow at 2 AM
var exportTask = new DataExportTask(...);
var executeAt = DateTimeOffset.Now.AddDays(1).Date.AddHours(2);

await scheduler.ScheduleOnceAsync(exportTask, executeAt);
```

---

### Example 9: Task Cancellation

```csharp
// Schedule task
await scheduler.ScheduleRecurringAsync(warmingTask, "0 * * * *");

// Cancel task
await scheduler.CancelAsync("cache-warming");
```

---

## Extension Methods

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class BackgroundTaskExtensions
{
    /// <summary>
    /// Adds background task infrastructure.
    /// </summary>
    public static IServiceCollection AddBackgroundTasks(
        this IServiceCollection services)
    {
        services.TryAddSingleton<IBackgroundTaskScheduler, HostedServiceTaskScheduler>();
        return services;
    }

    /// <summary>
    /// Adds background task with Quartz.NET.
    /// </summary>
    public static IServiceCollection AddBackgroundTasksWithQuartz(
        this IServiceCollection services)
    {
        services.AddQuartz();
        services.AddQuartzHostedService();
        services.TryAddSingleton<IBackgroundTaskScheduler, QuartzTaskScheduler>();
        return services;
    }

    /// <summary>
    /// Adds background task with Hangfire.
    /// </summary>
    public static IServiceCollection AddBackgroundTasksWithHangfire(
        this IServiceCollection services,
        Action<IGlobalConfiguration> configure)
    {
        services.AddHangfire(configure);
        services.AddHangfireServer();
        services.TryAddSingleton<IBackgroundTaskScheduler, HangfireTaskScheduler>();
        return services;
    }
}
```

---

## Best Practices

### 1. Idempotent Tasks
```csharp
// ✅ GOOD: Idempotent (can run multiple times)
public async Task ExecuteAsync(CancellationToken ct)
{
    var data = await _service.GetDataAsync();
    await _service.ProcessDataAsync(data);  // Idempotent operation
}

// ❌ BAD: Not idempotent (duplicates data)
public async Task ExecuteAsync(CancellationToken ct)
{
    await _service.CreateRecordAsync();  // Creates new record each time
}
```

### 2. Cancellation Support
```csharp
// ✅ GOOD: Handles cancellation
public async Task ExecuteAsync(CancellationToken ct)
{
    foreach (var item in items)
    {
        ct.ThrowIfCancellationRequested();
        await ProcessItemAsync(item);
    }
}
```

### 3. Error Handling
```csharp
// ✅ GOOD: Logs errors, allows retry
public async Task ExecuteAsync(CancellationToken ct)
{
    try
    {
        await _service.ProcessAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Task failed");
        throw;  // Platform handles retry
    }
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
