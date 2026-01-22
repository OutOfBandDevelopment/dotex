# Platform-Agnostic Background Tasks - Requirements

**Epic:** 04 - Distributed Caching
**Feature:** Platform-Agnostic Background Tasks
**Priority:** MEDIUM (Infrastructure)
**Complexity:** MEDIUM
**Estimated LOC:** ~450

---

## Overview

Platform-agnostic background task abstraction that works across hosting environments: ASP.NET Core (`IHostedService`), Azure Functions, AWS Lambda, Windows Services, Linux daemons, Quartz.NET, and Hangfire. Business logic implements `IBackgroundTask`; platform provides `IBackgroundTaskScheduler`.

---

## Business Requirements

### BR-1: Platform-Agnostic Task Abstraction
**As a** developer
**I want** to write background tasks that work on any hosting platform
**So that** I can deploy the same code to ASP.NET Core, Azure Functions, AWS Lambda, etc.

**Acceptance Criteria:**
- `IBackgroundTask` interface for business logic
- No platform-specific code in task implementations
- Same task runs on ASP.NET Core, Azure Functions, AWS Lambda, Windows Services, Linux daemons
- Platform determined by hosting configuration, not code

**Example:**
```csharp
// Platform-agnostic task
public class CacheWarmingTask : IBackgroundTask
{
    public string TaskId => "cache-warming";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        // Business logic - same code on ALL platforms
        await _productService.GetFeaturedProductsAsync();
        await _catalogService.GetCategoriesAsync();
    }
}

// ASP.NET Core: Runs via IHostedService
// Azure Functions: Runs via Timer Trigger
// AWS Lambda: Runs via EventBridge
// Windows Service: Runs via Windows Service infrastructure
// Quartz.NET: Runs via Quartz scheduler
// Hangfire: Runs via Hangfire scheduler
```

---

### BR-2: Scheduling Abstraction
**As a** developer
**I want** to schedule recurring and one-time tasks declaratively
**So that** scheduling works consistently across platforms

**Acceptance Criteria:**
- `IBackgroundTaskScheduler` interface for platform scheduling
- Recurring tasks via cron expressions
- One-time tasks at specific times
- Task cancellation support
- List scheduled tasks

**Example:**
```csharp
// Schedule recurring task (every hour)
await _scheduler.ScheduleRecurringAsync(
    new CacheWarmingTask(...),
    "0 * * * *");  // Cron: Every hour

// Schedule one-time task (tomorrow at 2 AM)
await _scheduler.ScheduleOnceAsync(
    new DataExportTask(...),
    DateTimeOffset.Now.AddDays(1).Date.AddHours(2));
```

---

### BR-3: Multiple Platform Implementations
**As a** DevOps engineer
**I want** scheduler implementations for all major platforms
**So that** I can choose hosting based on operational requirements

**Acceptance Criteria:**
- **ASP.NET Core:** `HostedServiceTaskScheduler`
- **Azure Functions:** `AzureFunctionsTaskScheduler`
- **AWS Lambda:** `AwsLambdaTaskScheduler`
- **Windows Service:** `WindowsServiceTaskScheduler`
- **Linux Daemon:** `LinuxDaemonTaskScheduler`
- **Quartz.NET:** `QuartzTaskScheduler`
- **Hangfire:** `HangfireTaskScheduler`

---

### BR-4: Cache Warming Use Case
**As a** system
**I want** to warm caches on startup and periodically
**So that** first requests are fast

**Acceptance Criteria:**
- `CacheWarmingTask` implementation
- Calls cached services to populate cache
- Results discarded (cache population only)
- Runs on startup and hourly
- Works on ALL platforms

---

### BR-5: Scheduled Cache Eviction Use Case
**As a** system
**I want** to periodically evict stale cache entries
**So that** memory usage stays bounded

**Acceptance Criteria:**
- `CacheEvictionTask` implementation
- Removes cache entries older than TTL
- Runs daily at low-traffic hours
- Logs eviction statistics
- Works on ALL platforms

---

## Technical Requirements

### TR-1: Interface Design
```csharp
/// <summary>
/// Platform-agnostic background task abstraction.
/// </summary>
public interface IBackgroundTask
{
    /// <summary>
    /// Task identifier (unique per task type).
    /// </summary>
    string TaskId { get; }

    /// <summary>
    /// Executes the task logic (platform-agnostic).
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);
}

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
    /// Schedules one-time task for future execution.
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

/// <summary>
/// Information about scheduled task.
/// </summary>
public class ScheduledTaskInfo
{
    public string TaskId { get; set; }
    public string CronExpression { get; set; }
    public DateTimeOffset? NextRun { get; set; }
    public DateTimeOffset? LastRun { get; set; }
}
```

---

### TR-2: Platform Scheduler Implementations

**ASP.NET Core (IHostedService):**
```csharp
public class HostedServiceTaskScheduler : IBackgroundTaskScheduler
{
    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        // Register BackgroundService that executes task on cron schedule
        var hostedService = new CronBackgroundService(task, cronExpression);
        _hostedServiceManager.AddService(hostedService);
    }
}
```

**Azure Functions (Timer Trigger):**
```csharp
public class AzureFunctionsTaskScheduler : IBackgroundTaskScheduler
{
    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        // Register timer trigger function
        // NOTE: Actual implementation uses Azure Functions attribute registration
        // at compile-time, not runtime
        _functionRegistry.RegisterTimerFunction(task.TaskId, cronExpression, task);
    }
}

// Generated Azure Function
public class CacheWarmingFunction
{
    [FunctionName("CacheWarmingTask")]
    public async Task Run(
        [TimerTrigger("0 * * * *")] TimerInfo timer,
        CancellationToken ct)
    {
        var task = _serviceProvider.GetRequiredService<CacheWarmingTask>();
        await task.ExecuteAsync(ct);
    }
}
```

**AWS Lambda (EventBridge):**
```csharp
public class AwsLambdaTaskScheduler : IBackgroundTaskScheduler
{
    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        // Create EventBridge rule with cron schedule
        var rule = new PutRuleRequest
        {
            Name = task.TaskId,
            ScheduleExpression = $"cron({cronExpression})",
            State = RuleState.ENABLED
        };

        await _eventBridgeClient.PutRuleAsync(rule);
        await _eventBridgeClient.PutTargetsAsync(new PutTargetsRequest
        {
            Rule = task.TaskId,
            Targets = new List<Target>
            {
                new Target
                {
                    Arn = _lambdaArn,
                    Input = JsonSerializer.Serialize(new { TaskId = task.TaskId })
                }
            }
        });
    }
}
```

**Quartz.NET:**
```csharp
public class QuartzTaskScheduler : IBackgroundTaskScheduler
{
    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        var job = JobBuilder.Create<BackgroundTaskJob>()
            .WithIdentity(task.TaskId)
            .UsingJobData("TaskId", task.TaskId)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{task.TaskId}-trigger")
            .WithCronSchedule(cronExpression)
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }
}
```

**Hangfire:**
```csharp
public class HangfireTaskScheduler : IBackgroundTaskScheduler
{
    public Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        RecurringJob.AddOrUpdate(
            task.TaskId,
            () => ExecuteTaskAsync(task.TaskId, CancellationToken.None),
            cronExpression);

        return Task.CompletedTask;
    }

    public async Task ExecuteTaskAsync(string taskId, CancellationToken ct)
    {
        var task = _taskFactory.GetTask(taskId);
        await task.ExecuteAsync(ct);
    }
}
```

---

### TR-3: Cron Expression Support

**Format:** Standard cron (5-field or 6-field depending on platform)

**Examples:**
```
0 * * * *           Every hour
0 0 * * *           Daily at midnight
0 2 * * *           Daily at 2 AM
*/15 * * * *        Every 15 minutes
0 0 * * 0           Weekly on Sunday
0 0 1 * *           Monthly on the 1st
```

---

### TR-4: Cache Warming Task Implementation

**Example:**
```csharp
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
        _logger.LogInformation("Starting cache warming...");

        try
        {
            // Warm categories (result discarded)
            _ = await _catalogService.GetCategoriesAsync();

            // Warm featured products
            _ = await _productService.GetFeaturedProductsAsync();

            // Warm common product IDs
            var commonProductIds = new[] { 1, 2, 3, 5, 10 };
            foreach (var productId in commonProductIds)
            {
                ct.ThrowIfCancellationRequested();
                _ = await _productService.GetProductAsync(productId);
            }

            _logger.LogInformation("Cache warming completed successfully");
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

### TR-5: Cache Eviction Task Implementation

**Example:**
```csharp
public class CacheEvictionTask : IBackgroundTask
{
    private readonly ICacheService _cache;
    private readonly ILogger<CacheEvictionTask> _logger;

    public string TaskId => "cache-eviction";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting cache eviction...");

        try
        {
            // Evict expired entries
            var evicted = await _cache.EvictExpiredEntriesAsync();

            _logger.LogInformation("Evicted {Count} expired cache entries", evicted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache eviction failed");
            throw;
        }
    }
}
```

---

### TR-6: Performance Requirements

- **Task execution overhead:** < 100ms
- **Scheduling overhead:** < 50ms per task
- **Concurrent task support:** 10+ tasks
- **Graceful shutdown:** < 30 seconds

---

### TR-7: Thread Safety

- Tasks executed one at a time per TaskId
- Concurrent execution of different tasks supported
- Cancellation token propagated correctly
- No orphaned tasks on shutdown

---

## Non-Functional Requirements

### NFR-1: Compatibility
- Works with .NET 10.0
- ASP.NET Core 10.0
- Azure Functions v4
- AWS Lambda .NET Runtime
- Quartz.NET 3.x
- Hangfire 1.8+

### NFR-2: Observability
- Logging of task execution (start, end, errors)
- Metrics collection (execution time, failure rate)
- Health checks for schedulers

### NFR-3: Testability
- Mock IBackgroundTaskScheduler for unit tests
- Test tasks without scheduler
- Integration tests with real schedulers

---

## Constraints

### C-1: Platform Limitations
- Azure Functions: Cron expressions limited to 5-field format
- AWS Lambda: Maximum execution time 15 minutes
- Windows Service: Requires service installation
- Quartz.NET/Hangfire: External dependencies

### C-2: Task Constraints
- Tasks must be idempotent (may execute multiple times)
- Tasks should handle cancellation gracefully
- Long-running tasks (> 15 min) not suitable for serverless

---

## Success Criteria

- ✅ `IBackgroundTask` and `IBackgroundTaskScheduler` interfaces
- ✅ 7 platform implementations (ASP.NET Core, Azure Functions, AWS Lambda, Windows Service, Linux Daemon, Quartz, Hangfire)
- ✅ Cache warming and eviction tasks
- ✅ Cron expression scheduling
- ✅ Works identically on all platforms
- ✅ 80%+ test coverage

---

## Out of Scope

- ❌ Distributed task coordination (use dedicated job scheduler)
- ❌ Task history/auditing (use logging)
- ❌ Task retry logic (implement in task or use platform features)
- ❌ Task priority queues (future enhancement)

---

## Dependencies

### Internal
- OoBDev.Framework.Caching (for cache tasks)
- OoBDev.System.Hosting (platform abstractions)

### External
- Microsoft.Extensions.Hosting (ASP.NET Core)
- Azure Functions SDK (Azure Functions)
- AWS Lambda SDK (AWS Lambda)
- Quartz (optional, for Quartz.NET)
- Hangfire (optional, for Hangfire)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 04 Overview](../README.md)
