using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.TestUtilities.Tests;

[TestClass]
[DoNotParallelize]
public class IntegrationContainersTests
{
    private const string Timeout = "CONTAINER_STARTUP_TIMEOUT_SECONDS";
    private const string Autostart = "CONTAINER_AUTOSTART";
    private const string Stop = "CONTAINER_STOP_AFTER_TESTS";

    public required TestContext TestContext { get; set; }

    private sealed class FakeDocker : IDockerCli
    {
        public ContainerState? State { get; set; }

        public ContainerState? StateAfterStart { get; set; }

        public List<string> Calls { get; } = [];

        public Task<ContainerState?> InspectAsync(string container, CancellationToken cancellationToken) => Task.FromResult(State);

        public Task StartAsync(string container, CancellationToken cancellationToken)
        {
            Calls.Add("start " + container);
            State = StateAfterStart;
            return Task.CompletedTask;
        }

        public Task ComposeUpAsync(string composeFile, string service, CancellationToken cancellationToken)
        {
            Calls.Add("up " + service);
            State = StateAfterStart;
            return Task.CompletedTask;
        }

        public Task StopAsync(string container, TimeSpan grace, CancellationToken cancellationToken)
        {
            Calls.Add("stop " + container);
            return Task.CompletedTask;
        }
    }

    private FakeDocker _docker = new();
    private string _leaseDirectory = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _docker = new FakeDocker();
        IntegrationContainers.Docker = _docker;
        _leaseDirectory = Path.Combine(Path.GetTempPath(), "oobdev-lease-tests-" + Guid.NewGuid().ToString("N"));
        IntegrationContainers.LeaseDirectory = _leaseDirectory;
        IntegrationContainers.PollInterval = TimeSpan.FromMilliseconds(10);
        IntegrationContainers.Overrides[Timeout] = "1";
    }

    [TestCleanup]
    public void Cleanup()
    {
        IntegrationContainers.Docker = new ProcessDockerCli();
        IntegrationContainers.PollInterval = TimeSpan.FromMilliseconds(250);
        IntegrationContainers.LeaseDirectory = Path.Combine(Path.GetTempPath(), "oobdev-lease-tests-reset");
        if (Directory.Exists(_leaseDirectory))
            Directory.Delete(_leaseDirectory, true);
        IntegrationContainers.Overrides.Clear();
    }

    private static async Task<string> InconclusiveMessageAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (AssertInconclusiveException ex)
        {
            return ex.Message;
        }

        return string.Empty;
    }

    private static int AnotherLiveProcessId()
    {
        foreach (var process in global::System.Diagnostics.Process.GetProcesses())
        {
            try
            {
                if (process.Id != Environment.ProcessId && !process.HasExited)
                    return process.Id;
            }
            catch (global::System.ComponentModel.Win32Exception)
            {
            }
        }

        throw new InvalidOperationException("No other process to borrow an id from.");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Ensure_AlreadyHealthy_StartsNothingAndNeverStopsItAsync()
    {
        // Stage
        _docker.State = new ContainerState(true, "healthy");

        // Test
        await IntegrationContainers.EnsureAsync(TestContext, "oobd-test-redis");
        await IntegrationContainers.ReleaseAsync(TestContext, "oobd-test-redis");

        // Assert
        Assert.AreEqual(0, _docker.Calls.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Ensure_RunningWithoutHealthCheck_IsReadyAsync()
    {
        _docker.State = new ContainerState(true, string.Empty);

        await IntegrationContainers.EnsureAsync(TestContext, "oobd-test-mosquitto");

        Assert.AreEqual(0, _docker.Calls.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Ensure_StoppedContainer_StartsItAndReleaseStopsItAsync()
    {
        // Stage
        _docker.State = new ContainerState(false, string.Empty);
        _docker.StateAfterStart = new ContainerState(true, "healthy");

        // Test
        await IntegrationContainers.EnsureAsync(TestContext, "oobd-test-redis");
        await IntegrationContainers.ReleaseAsync(TestContext, "oobd-test-redis");

        // Assert
        CollectionAssert.AreEqual(new[] { "start oobd-test-redis", "stop oobd-test-redis" }, _docker.Calls);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Release_StopDisabled_LeavesTheContainerRunningAsync()
    {
        _docker.State = new ContainerState(false, string.Empty);
        _docker.StateAfterStart = new ContainerState(true, "healthy");
        IntegrationContainers.Overrides[Stop] = "false";

        await IntegrationContainers.EnsureAsync(TestContext, "oobd-test-redis");
        await IntegrationContainers.ReleaseAsync(TestContext, "oobd-test-redis");

        CollectionAssert.AreEqual(new[] { "start oobd-test-redis" }, _docker.Calls);
        IntegrationContainers.Overrides.Remove(Stop);
        await IntegrationContainers.ReleaseAsync(TestContext, "oobd-test-redis");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Release_AnotherLiveProcessStillUsesIt_LeavesTheContainerRunningAsync()
    {
        // Stage: this process starts the container; another live process also holds a lease
        _docker.State = new ContainerState(false, string.Empty);
        _docker.StateAfterStart = new ContainerState(true, "healthy");
        await IntegrationContainers.EnsureAsync(TestContext, "oobd-test-redis");
        File.WriteAllText(Path.Combine(_leaseDirectory, "oobd-test-redis", AnotherLiveProcessId() + ".lease"), string.Empty);

        // Test
        await IntegrationContainers.ReleaseAsync(TestContext, "oobd-test-redis");

        // Assert: not stopped, the marker stays for whoever releases last
        CollectionAssert.AreEqual(new[] { "start oobd-test-redis" }, _docker.Calls);
        Assert.IsTrue(File.Exists(Path.Combine(_leaseDirectory, "oobd-test-redis", "started")));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Release_LastUserOfAContainerAnotherProcessStarted_StopsItAsync()
    {
        // Stage: another (dead) process started it and left the marker; this process only uses it
        _docker.State = new ContainerState(true, "healthy");
        Directory.CreateDirectory(Path.Combine(_leaseDirectory, "oobd-test-redis"));
        File.WriteAllText(Path.Combine(_leaseDirectory, "oobd-test-redis", "started"), "1");
        await IntegrationContainers.EnsureAsync(TestContext, "oobd-test-redis");

        // Test
        await IntegrationContainers.ReleaseAsync(TestContext, "oobd-test-redis");

        // Assert
        CollectionAssert.AreEqual(new[] { "stop oobd-test-redis" }, _docker.Calls);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Release_ContainerTheTestsDidNotStart_IsNeverStoppedAsync()
    {
        // Stage: running but still starting, so the helper waits for it without having started it
        _docker.State = new ContainerState(true, "starting");
        _docker.StateAfterStart = null;
        var task = IntegrationContainers.EnsureAsync(TestContext, "oobd-test-redis");
        await Task.Delay(30);
        _docker.State = new ContainerState(true, "healthy");
        await task;

        // Test
        await IntegrationContainers.ReleaseAsync(TestContext, "oobd-test-redis");

        // Assert
        Assert.AreEqual(0, _docker.Calls.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Ensure_NeverBecomesHealthy_IsInconclusiveWithinTheTimeoutAsync()
    {
        // Stage
        _docker.State = new ContainerState(false, string.Empty);
        _docker.StateAfterStart = new ContainerState(true, "starting");
        var watch = global::System.Diagnostics.Stopwatch.StartNew();

        // Test
        var message = await InconclusiveMessageAsync(() => IntegrationContainers.EnsureAsync(TestContext, "oobd-test-keycloak"));

        // Assert
        StringAssert.Contains(message, "did not become healthy in time");
        Assert.IsTrue(watch.Elapsed < TimeSpan.FromSeconds(4), $"Took {watch.Elapsed}");
        await IntegrationContainers.ReleaseAsync(TestContext, "oobd-test-keycloak");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Ensure_UnhealthyContainer_IsInconclusiveAsync()
    {
        _docker.State = new ContainerState(true, "unhealthy");

        var message = await InconclusiveMessageAsync(() => IntegrationContainers.EnsureAsync(TestContext, "oobd-test-redis"));

        StringAssert.Contains(message, "unhealthy");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Ensure_DockerUnavailable_IsInconclusiveAsync()
    {
        _docker.State = null;
        IntegrationContainers.Overrides[Autostart] = "false";

        var message = await InconclusiveMessageAsync(() => IntegrationContainers.EnsureAsync(TestContext, "oobd-test-redis"));

        StringAssert.Contains(message, "CONTAINER_AUTOSTART is false");
        Assert.AreEqual(0, _docker.Calls.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ServiceOf_IntegrationComposeFile_MapsContainerNameToServiceName()
    {
        // Stage
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "containers", "testing", "docker-compose.integration-tests.yml")))
            directory = directory.Parent;
        var file = Path.Combine(directory!.FullName, "containers", "testing", "docker-compose.integration-tests.yml");

        // Test and Assert
        Assert.AreEqual("sql-server", IntegrationContainers.ServiceOf(file, "oobd-test-sqlserver"));
        Assert.AreEqual("redis", IntegrationContainers.ServiceOf(file, "oobd-test-redis"));
        Assert.IsNull(IntegrationContainers.ServiceOf(file, "oobd-test-none"));
    }
}
