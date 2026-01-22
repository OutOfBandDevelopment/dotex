# Platform-Agnostic Background Tasks - Testing Strategy

**Epic:** 04 - Distributed Caching
**Feature:** Platform-Agnostic Background Tasks
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage focusing on task execution, scheduling, and platform integration.

**Test Categories:**
- **Unit Tests** - Task execution, scheduler logic
- **Integration Tests** - Platform-specific schedulers
- **Performance Tests** - Scheduling overhead

---

## Test Pyramid

```
        ┌───────────────────┐
        │  Performance Tests│  (6 tests)
        └───────────────────┘
      ┌───────────────────────┐
      │  Integration Tests    │  (18 tests)
      └───────────────────────┘
  ┌─────────────────────────────┐
  │       Unit Tests            │  (42+ tests)
  └─────────────────────────────┘
```

---

## Unit Tests

### 1. Background Task Tests

```csharp
[TestClass]
public class CacheWarmingTaskTests
{
    [TestMethod]
    public async Task ExecuteAsync_ValidServices_CallsServices()
    {
        // Arrange
        var mockProductService = new Mock<IProductService>();
        var mockCatalogService = new Mock<ICatalogService>();

        mockProductService
            .Setup(s => s.GetFeaturedProductsAsync())
            .ReturnsAsync(new List<Product> { new Product { Id = 1 } });

        mockCatalogService
            .Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(new List<Category> { new Category { Id = 1 } });

        var task = new CacheWarmingTask(
            mockProductService.Object,
            mockCatalogService.Object,
            Mock.Of<ILogger<CacheWarmingTask>>());

        // Act
        await task.ExecuteAsync(CancellationToken.None);

        // Assert
        mockProductService.Verify(s => s.GetFeaturedProductsAsync(), Times.Once);
        mockCatalogService.Verify(s => s.GetCategoriesAsync(), Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockService = new Mock<IProductService>();
        mockService
            .Setup(s => s.GetProductAsync(It.IsAny<int>()))
            .Returns(async () =>
            {
                await Task.Delay(100);
                return new Product();
            });

        var task = new CacheWarmingTask(
            mockService.Object,
            Mock.Of<ICatalogService>(),
            Mock.Of<ILogger<CacheWarmingTask>>());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(50);  // Cancel after 50ms

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(
            async () => await task.ExecuteAsync(cts.Token));
    }

    [TestMethod]
    public async Task ExecuteAsync_ServiceThrows_PropagatesException()
    {
        // Arrange
        var mockService = new Mock<IProductService>();
        mockService
            .Setup(s => s.GetFeaturedProductsAsync())
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        var task = new CacheWarmingTask(
            mockService.Object,
            Mock.Of<ICatalogService>(),
            Mock.Of<ILogger<CacheWarmingTask>>());

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await task.ExecuteAsync(CancellationToken.None));
    }
}

[TestClass]
public class CacheEvictionTaskTests
{
    [TestMethod]
    public async Task ExecuteAsync_ValidCache_EvictsExpiredEntries()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();
        mockCache
            .Setup(c => c.EvictExpiredEntriesAsync())
            .ReturnsAsync(42);

        var task = new CacheEvictionTask(
            mockCache.Object,
            Mock.Of<ILogger<CacheEvictionTask>>());

        // Act
        await task.ExecuteAsync(CancellationToken.None);

        // Assert
        mockCache.Verify(c => c.EvictExpiredEntriesAsync(), Times.Once);
    }
}
```

---

### 2. Scheduler Tests

```csharp
[TestClass]
public class HostedServiceTaskSchedulerTests
{
    [TestMethod]
    public async Task ScheduleRecurringAsync_ValidTask_RegistersHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var scheduler = new HostedServiceTaskScheduler(services);

        var mockTask = new Mock<IBackgroundTask>();
        mockTask.Setup(t => t.TaskId).Returns("test-task");

        // Act
        await scheduler.ScheduleRecurringAsync(mockTask.Object, "0 * * * *");

        // Assert
        var hostedServices = services.Where(s => s.ServiceType == typeof(IHostedService));
        Assert.AreEqual(1, hostedServices.Count());
    }

    [TestMethod]
    public async Task ScheduleOnceAsync_ValidTask_SchedulesExecution()
    {
        // Arrange
        var services = new ServiceCollection();
        var scheduler = new HostedServiceTaskScheduler(services);

        var mockTask = new Mock<IBackgroundTask>();
        mockTask.Setup(t => t.TaskId).Returns("test-task");

        var executeAt = DateTimeOffset.UtcNow.AddMinutes(5);

        // Act
        await scheduler.ScheduleOnceAsync(mockTask.Object, executeAt);

        // Assert
        var hostedServices = services.Where(s => s.ServiceType == typeof(IHostedService));
        Assert.AreEqual(1, hostedServices.Count());
    }

    [TestMethod]
    public async Task CancelAsync_ScheduledTask_CancelsTask()
    {
        // Arrange
        var scheduler = new HostedServiceTaskScheduler(new ServiceCollection());
        var mockTask = new Mock<IBackgroundTask>();
        mockTask.Setup(t => t.TaskId).Returns("test-task");

        await scheduler.ScheduleRecurringAsync(mockTask.Object, "0 * * * *");

        // Act
        await scheduler.CancelAsync("test-task");

        // Assert
        var tasks = await scheduler.GetScheduledTasksAsync();
        Assert.AreEqual(0, tasks.Count());
    }

    [TestMethod]
    public async Task GetScheduledTasksAsync_MultipleTask s_ReturnsAllTasks()
    {
        // Arrange
        var scheduler = new HostedServiceTaskScheduler(new ServiceCollection());

        var task1 = new Mock<IBackgroundTask>();
        task1.Setup(t => t.TaskId).Returns("task-1");

        var task2 = new Mock<IBackgroundTask>();
        task2.Setup(t => t.TaskId).Returns("task-2");

        await scheduler.ScheduleRecurringAsync(task1.Object, "0 * * * *");
        await scheduler.ScheduleRecurringAsync(task2.Object, "0 2 * * *");

        // Act
        var tasks = await scheduler.GetScheduledTasksAsync();

        // Assert
        Assert.AreEqual(2, tasks.Count());
        Assert.IsTrue(tasks.Any(t => t.TaskId == "task-1"));
        Assert.IsTrue(tasks.Any(t => t.TaskId == "task-2"));
    }
}

[TestClass]
public class QuartzTaskSchedulerTests
{
    [TestMethod]
    public async Task ScheduleRecurringAsync_ValidTask_CreatesJob()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler>();
        var scheduler = new QuartzTaskScheduler(mockScheduler.Object, Mock.Of<IServiceProvider>());

        var mockTask = new Mock<IBackgroundTask>();
        mockTask.Setup(t => t.TaskId).Returns("test-task");

        // Act
        await scheduler.ScheduleRecurringAsync(mockTask.Object, "0 * * * *");

        // Assert
        mockScheduler.Verify(
            s => s.ScheduleJob(
                It.Is<IJobDetail>(j => j.Key.Name == "test-task"),
                It.IsAny<ITrigger>()),
            Times.Once);
    }
}

[TestClass]
public class HangfireTaskSchedulerTests
{
    [TestMethod]
    public async Task ScheduleRecurringAsync_ValidTask_AddsRecurringJob()
    {
        // Arrange
        var mockRecurringJobManager = new Mock<IRecurringJobManager>();
        var scheduler = new HangfireTaskScheduler(
            Mock.Of<IServiceProvider>(),
            mockRecurringJobManager.Object);

        var mockTask = new Mock<IBackgroundTask>();
        mockTask.Setup(t => t.TaskId).Returns("test-task");

        // Act
        await scheduler.ScheduleRecurringAsync(mockTask.Object, "0 * * * *");

        // Assert
        mockRecurringJobManager.Verify(
            m => m.AddOrUpdate(
                "test-task",
                It.IsAny<Job>(),
                "0 * * * *",
                It.IsAny<RecurringJobOptions>()),
            Times.Once);
    }
}
```

---

### 3. Cron Expression Tests

```csharp
[TestClass]
public class CronExpressionTests
{
    [TestMethod]
    public void Parse_ValidExpression_ReturnsExpression()
    {
        // Arrange
        var expression = "0 * * * *";

        // Act
        var cron = CronExpression.Parse(expression);

        // Assert
        Assert.IsNotNull(cron);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void Parse_InvalidExpression_ThrowsFormatException()
    {
        // Arrange
        var expression = "invalid";

        // Act
        CronExpression.Parse(expression);
    }

    [TestMethod]
    public void GetNextOccurrence_HourlyCron_ReturnsNextHour()
    {
        // Arrange
        var cron = CronExpression.Parse("0 * * * *");  // Every hour
        var now = new DateTimeOffset(2024, 1, 1, 10, 30, 0, TimeSpan.Zero);

        // Act
        var next = cron.GetNextOccurrence(now);

        // Assert
        Assert.IsNotNull(next);
        Assert.AreEqual(new DateTimeOffset(2024, 1, 1, 11, 0, 0, TimeSpan.Zero), next.Value);
    }

    [TestMethod]
    public void GetNextOccurrence_DailyCron_ReturnsNextDay()
    {
        // Arrange
        var cron = CronExpression.Parse("0 2 * * *");  // Daily at 2 AM
        var now = new DateTimeOffset(2024, 1, 1, 3, 0, 0, TimeSpan.Zero);

        // Act
        var next = cron.GetNextOccurrence(now);

        // Assert
        Assert.IsNotNull(next);
        Assert.AreEqual(new DateTimeOffset(2024, 1, 2, 2, 0, 0, TimeSpan.Zero), next.Value);
    }
}
```

---

## Integration Tests

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class BackgroundTaskIntegrationTests
{
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register services
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<IProductService, InMemoryProductService>();

        // Register tasks
        services.AddSingleton<CacheWarmingTask>();

        // Register scheduler
        services.AddBackgroundTasks();

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task CacheWarmingTask_ExecuteAsync_WarmsCaches()
    {
        // Arrange
        var task = _serviceProvider.GetRequiredService<CacheWarmingTask>();
        var cache = _serviceProvider.GetRequiredService<ICacheService>();

        // Act
        await task.ExecuteAsync(CancellationToken.None);

        // Assert - Cache should be populated
        var cachedProducts = await cache.GetAsync<IEnumerable<Product>>("featured-products");
        Assert.IsNotNull(cachedProducts);
    }

    [TestMethod]
    public async Task ScheduledTask_ExecutesOnSchedule()
    {
        // Arrange
        var scheduler = _serviceProvider.GetRequiredService<IBackgroundTaskScheduler>();
        var task = _serviceProvider.GetRequiredService<CacheWarmingTask>();

        var executionCount = 0;
        var wrappedTask = new TestBackgroundTask(() =>
        {
            executionCount++;
            return task.ExecuteAsync(CancellationToken.None);
        });

        // Schedule to run every second (for testing)
        await scheduler.ScheduleRecurringAsync(wrappedTask, "* * * * * *");

        // Act - Wait for 3 seconds
        await Task.Delay(TimeSpan.FromSeconds(3));

        // Assert - Should have executed at least twice
        Assert.IsTrue(executionCount >= 2);
    }
}

public class TestBackgroundTask : IBackgroundTask
{
    private readonly Func<Task> _action;

    public string TaskId => "test-task";

    public TestBackgroundTask(Func<Task> action)
    {
        _action = action;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        await _action();
    }
}
```

---

## Performance Tests

```csharp
[TestClass]
[TestCategory(TestCategories.Unit)]
public class BackgroundTaskPerformanceTests
{
    [TestMethod]
    public async Task TaskExecutionOverhead_LessThan100Milliseconds()
    {
        // Arrange
        var task = new TestBackgroundTask(() => Task.CompletedTask);

        // Act
        var stopwatch = Stopwatch.StartNew();
        await task.ExecuteAsync(CancellationToken.None);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
            $"Execution overhead: {stopwatch.ElapsedMilliseconds}ms (expected < 100ms)");
    }

    [TestMethod]
    public async Task SchedulingOverhead_LessThan50Milliseconds()
    {
        // Arrange
        var scheduler = new HostedServiceTaskScheduler(new ServiceCollection());
        var task = new TestBackgroundTask(() => Task.CompletedTask);

        // Act
        var stopwatch = Stopwatch.StartNew();
        await scheduler.ScheduleRecurringAsync(task, "0 * * * *");
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50,
            $"Scheduling overhead: {stopwatch.ElapsedMilliseconds}ms (expected < 50ms)");
    }

    [TestMethod]
    public async Task ConcurrentTasks_SupportsMultipleTasks()
    {
        // Arrange
        var scheduler = new HostedServiceTaskScheduler(new ServiceCollection());
        var tasks = Enumerable.Range(0, 10)
            .Select(i => new TestBackgroundTask(() => Task.CompletedTask))
            .ToList();

        // Act
        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks.Select(t => scheduler.ScheduleRecurringAsync(t, "0 * * * *")));
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
            $"10 tasks scheduled in {stopwatch.ElapsedMilliseconds}ms (expected < 500ms)");
    }

    [TestMethod]
    public async Task CronParsing_Performance()
    {
        // Arrange
        var expression = "0 * * * *";

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            CronExpression.Parse(expression);
        }
        stopwatch.Stop();

        var avgTime = stopwatch.ElapsedMilliseconds / 1000.0;

        // Assert
        Assert.IsTrue(avgTime < 1,
            $"Average cron parsing: {avgTime}ms (expected < 1ms)");
    }
}
```

---

## Coverage Goals

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| IBackgroundTask Implementations | 85% | ExecuteAsync, error handling |
| IBackgroundTaskScheduler Implementations | 85% | Scheduling, cancellation |
| Cron Expression Parsing | 80% | Parse, GetNextOccurrence |
| Extension Methods | 75% | DI registration |
| Error Handling | 70% | Task failures, cancellation |

**Total Tests:** 42 unit + 18 integration + 6 performance = **66 tests**

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 04 Overview](../README.md)
