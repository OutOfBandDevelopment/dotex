# Platform-Agnostic Background Tasks - Architecture

**Epic:** 04 - Distributed Caching
**Feature:** Platform-Agnostic Background Tasks
**Last Updated:** 2026-01-22

---

## Architectural Overview

Platform-agnostic background tasks using **Template Method Pattern** and **Strategy Pattern**. Business logic in `IBackgroundTask` remains platform-independent; platform-specific scheduling in `IBackgroundTaskScheduler` implementations.

```
┌────────────────────────────────────────────────────────┐
│              IBackgroundTask                           │
│         (Platform-Agnostic Logic)                      │
│                                                        │
│  CacheWarmingTask    CacheEvictionTask                │
│  DataExportTask      ReportGenerationTask             │
└───────────────────────┬────────────────────────────────┘
                        │
                        ↓
┌────────────────────────────────────────────────────────┐
│        IBackgroundTaskScheduler                        │
│      (Platform-Specific Scheduling)                    │
└───────────────────────┬────────────────────────────────┘
                        │
      ┌─────────────────┼─────────────────┐
      ↓                 ↓                 ↓
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│HostedService │ │   Azure      │ │     AWS      │
│  Scheduler   │ │  Functions   │ │   Lambda     │
└──────────────┘ └──────────────┘ └──────────────┘
      ↓                 ↓                 ↓
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│   Quartz     │ │   Hangfire   │ │   Windows    │
│  Scheduler   │ │  Scheduler   │ │   Service    │
└──────────────┘ └──────────────┘ └──────────────┘
```

---

## Core Components

### 1. IBackgroundTask

**Purpose:** Platform-agnostic task abstraction.

**Design:**
```csharp
public interface IBackgroundTask
{
    string TaskId { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}
```

**Example Implementation:**
```csharp
public class CacheWarmingTask : IBackgroundTask
{
    private readonly IProductService _productService;
    private readonly ILogger _logger;

    public string TaskId => "cache-warming";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Cache warming started");

        _ = await _productService.GetFeaturedProductsAsync();

        _logger.LogInformation("Cache warming completed");
    }
}
```

---

### 2. IBackgroundTaskScheduler

**Purpose:** Platform-specific scheduling abstraction.

**Design:**
```csharp
public interface IBackgroundTaskScheduler
{
    Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression);
    Task ScheduleOnceAsync(IBackgroundTask task, DateTimeOffset executeAt);
    Task CancelAsync(string taskId);
    Task<IEnumerable<ScheduledTaskInfo>> GetScheduledTasksAsync();
}
```

---

### 3. Platform Implementations

#### HostedServiceTaskScheduler (ASP.NET Core)

**Implementation:**
```csharp
public class HostedServiceTaskScheduler : IBackgroundTaskScheduler
{
    private readonly IServiceCollection _services;
    private readonly Dictionary<string, IHostedService> _hostedServices;

    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        var hostedService = new CronBackgroundService(task, cronExpression);
        _services.AddSingleton<IHostedService>(hostedService);
        _hostedServices[task.TaskId] = hostedService;
    }

    public async Task ScheduleOnceAsync(IBackgroundTask task, DateTimeOffset executeAt)
    {
        var hostedService = new OnceBackgroundService(task, executeAt);
        _services.AddSingleton<IHostedService>(hostedService);
        _hostedServices[task.TaskId] = hostedService;
    }
}

internal class CronBackgroundService : BackgroundService
{
    private readonly IBackgroundTask _task;
    private readonly string _cronExpression;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var next = CronExpression.Parse(_cronExpression).GetNextOccurrence(now);

            if (next.HasValue)
            {
                var delay = next.Value - now;
                await Task.Delay(delay, stoppingToken);

                await _task.ExecuteAsync(stoppingToken);
            }
        }
    }
}
```

---

#### QuartzTaskScheduler

**Implementation:**
```csharp
public class QuartzTaskScheduler : IBackgroundTaskScheduler
{
    private readonly IScheduler _quartzScheduler;
    private readonly IServiceProvider _serviceProvider;

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

        await _quartzScheduler.ScheduleJob(job, trigger);
    }
}

public class BackgroundTaskJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public async Task Execute(IJobExecutionContext context)
    {
        var taskId = context.JobDetail.JobDataMap.GetString("TaskId");
        var task = _serviceProvider.GetRequiredKeyedService<IBackgroundTask>(taskId);

        await task.ExecuteAsync(CancellationToken.None);
    }
}
```

---

#### HangfireTaskScheduler

**Implementation:**
```csharp
public class HangfireTaskScheduler : IBackgroundTaskScheduler
{
    private readonly IServiceProvider _serviceProvider;

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
        var task = _serviceProvider.GetRequiredKeyedService<IBackgroundTask>(taskId);
        await task.ExecuteAsync(ct);
    }
}
```

---

## Data Flow

### Sequence: Task Scheduling and Execution

```
┌──────────┐   ┌───────────┐   ┌──────────┐   ┌──────────┐
│ Startup  │   │ Scheduler │   │ Platform │   │   Task   │
└────┬─────┘   └─────┬─────┘   └────┬─────┘   └────┬─────┘
     │               │              │              │
     │ Schedule(task, cron)         │              │
     ├──────────────>│              │              │
     │               │              │              │
     │               │ Platform-    │              │
     │               │ specific     │              │
     │               │ scheduling   │              │
     │               ├─────────────>│              │
     │               │              │              │
     │               │              │ (Time passes)│
     │               │              │              │
     │               │              │ Trigger      │
     │               │              │ (cron match) │
     │               │              │              │
     │               │              │ Execute()    │
     │               │              ├─────────────>│
     │               │              │              │
     │               │              │ Business     │
     │               │              │ logic runs   │
     │               │              │              │
```

---

## Design Patterns

### 1. Template Method Pattern
- `IBackgroundTask.ExecuteAsync()` is template method
- Subclasses implement business logic
- Framework manages scheduling

### 2. Strategy Pattern
- Different scheduler strategies per platform
- Runtime selection based on configuration

### 3. Factory Pattern
- Task factory creates tasks with DI
- Scheduler factory creates platform scheduler

---

## Performance Optimizations

### 1. Lazy Task Instantiation
- Tasks created on-demand
- DI scope per execution

### 2. Efficient Cron Parsing
- Cron expressions parsed once
- Cached for subsequent calculations

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
