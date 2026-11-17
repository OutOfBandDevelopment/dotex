using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.TestUtilities.Containers;

/// <summary>
/// Makes sure the containers of the integration test stack are up before an Integration test class runs, and stops
/// the ones the tests started when the class is done. Call <see cref="EnsureAsync"/> from a
/// <c>[ClassInitialize]</c> method and <see cref="ReleaseAsync"/> from a <c>[ClassCleanup]</c> method.
/// </summary>
/// <remarks>
/// A container that is already running and healthy is used as it is and never stopped. A stopped or missing one is
/// started (<c>docker start</c>, or <c>docker compose up</c> for one that was never created) and watched until its
/// health check passes. When that does not happen within the startup timeout, or Docker is not available, the tests
/// end <c>Inconclusive</c>. Test parameters: <c>CONTAINER_STARTUP_TIMEOUT_SECONDS</c> (default 5),
/// <c>CONTAINER_AUTOSTART</c> (default true: start what is down) and <c>CONTAINER_STOP_AFTER_TESTS</c> (default true:
/// stop what the tests started).
/// </remarks>
public static class IntegrationContainers
{
    /// <summary>The default time to wait for a container to become healthy.</summary>
    public const int DefaultStartupSeconds = 5;

    private const string ComposeFileName = "docker-compose.integration-tests.yml";
    private static readonly Lock _gate = new();
    private static readonly HashSet<string> _leased = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, DateTimeOffset> _readyUntil = new(StringComparer.Ordinal);
    private static readonly TimeSpan _readyCache = TimeSpan.FromSeconds(30);
    private static bool _exitHookRegistered;
    private static bool _stopAfterTests = true;
    private static IDockerCli _docker = new ProcessDockerCli();
    private static ContainerLeases _leases = new(ContainerLeases.DefaultDirectory);

    /// <summary>The lease directory shared between test processes; replaced in unit tests.</summary>
    internal static string LeaseDirectory
    {
        set => _leases = new ContainerLeases(value);
    }

    /// <summary>The Docker client; replaced in unit tests.</summary>
    internal static IDockerCli Docker
    {
        get => _docker;
        set
        {
            _docker = value;
            lock (_gate)
                _readyUntil.Clear();
        }
    }

    /// <summary>Settings that win over the test parameters; used by unit tests.</summary>
    internal static Dictionary<string, string> Overrides { get; } = new(StringComparer.Ordinal);

    /// <summary>Polling interval while waiting for a health check.</summary>
    internal static TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Ensures the named containers (for example <c>oobd-test-redis</c>) are ready, or ends the tests of the class as Inconclusive.
    /// </summary>
    public static async Task EnsureAsync(TestContext testContext, params string[] containers)
    {
        ArgumentNullException.ThrowIfNull(testContext);
        var seconds = Overrides.TryGetValue("CONTAINER_STARTUP_TIMEOUT_SECONDS", out var overridden)
            ? int.Parse(overridden, global::System.Globalization.CultureInfo.InvariantCulture)
            : testContext.GetPropertyOrDefault("CONTAINER_STARTUP_TIMEOUT_SECONDS", DefaultStartupSeconds);
        var autostart = Flag(testContext, "CONTAINER_AUTOSTART");
        _stopAfterTests = Flag(testContext, "CONTAINER_STOP_AFTER_TESTS");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));

        foreach (var container in containers)
        {
            lock (_gate)
            {
                if (_readyUntil.TryGetValue(container, out var until) && until > DateTimeOffset.UtcNow)
                    continue;
            }

            var started = false;
            var problem = await TryEnsureAsync(container, autostart, timeout.Token, () => started = true);
            if (problem is null)
            {
                Lease(container, started);
                continue;
            }

            if (started)
                Lease(container, true);

            if (problem is not null)
                Assert.Inconclusive($"Container {container} is not available: {problem} (waited at most {seconds} s; CONTAINER_STARTUP_TIMEOUT_SECONDS changes the wait).");
        }
    }

    /// <summary>
    /// Ends this process's use of the containers. A container the tests started is stopped only when no other test process
    /// still uses it (and <c>CONTAINER_STOP_AFTER_TESTS</c> is not false); one that was already running is never stopped.
    /// </summary>
    public static async Task ReleaseAsync(TestContext testContext, params string[] containers)
    {
        ArgumentNullException.ThrowIfNull(testContext);
        await ReleaseCoreAsync(Flag(testContext, "CONTAINER_STOP_AFTER_TESTS"), containers);
    }

    private static async Task ReleaseCoreAsync(bool stopAllowed, IEnumerable<string> containers)
    {
        foreach (var container in containers)
        {
            lock (_gate)
            {
                _readyUntil.Remove(container);
                if (!_leased.Remove(container))
                    continue;
            }

            if (_leases.Release(container) && stopAllowed)
                await Docker.StopAsync(container, TimeSpan.FromSeconds(10), CancellationToken.None);
        }
    }

    private static void Lease(string container, bool startedByTests)
    {
        _leases.Acquire(container, startedByTests);
        lock (_gate)
        {
            _leased.Add(container);
            _readyUntil[container] = DateTimeOffset.UtcNow + _readyCache;
            if (!_exitHookRegistered)
            {
                _exitHookRegistered = true;
                AppDomain.CurrentDomain.ProcessExit += (_, _) => ReleaseCoreAsync(_stopAfterTests, LeasedContainers()).GetAwaiter().GetResult();
            }
        }
    }

    private static string[] LeasedContainers()
    {
        lock (_gate)
            return [.. _leased];
    }

    private static async Task<string?> TryEnsureAsync(string container, bool autostart, CancellationToken cancellationToken, Action onStarted)
    {
        try
        {
            var state = await Docker.InspectAsync(container, cancellationToken);
            if (state is { Ready: true })
                return null;

            if (state is not { Running: true })
            {
                if (!autostart)
                    return "it is not running and CONTAINER_AUTOSTART is false";

                if (state is null)
                {
                    var compose = FindComposeFile();
                    if (compose is null)
                        return "it does not exist and the compose file was not found (start the stack with containers/testing/scripts/integration-up.sh)";
                    var service = ServiceOf(compose, container);
                    if (service is null)
                        return "it is not defined in the integration compose file";
                    await Docker.ComposeUpAsync(compose, service, cancellationToken);
                }
                else
                {
                    await Docker.StartAsync(container, cancellationToken);
                }

                onStarted();
            }

            while (true)
            {
                state = await Docker.InspectAsync(container, cancellationToken);
                if (state is null)
                    return "Docker does not know the container";
                if (state.Ready)
                    return null;
                if (state.Health == "unhealthy")
                    return "its health check reports unhealthy";
                await Task.Delay(PollInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            return "it did not become healthy in time";
        }
    }

    private static bool Flag(TestContext testContext, string name) =>
        !bool.TryParse(Overrides.TryGetValue(name, out var overridden) ? overridden : testContext.GetPropertyOrDefault(name, "true"), out var value) || value;

    internal static string? ServiceOf(string composeFile, string container)
    {
        string? service = null;
        foreach (var line in File.ReadLines(composeFile))
        {
            if (line.Length > 2 && line[0] == ' ' && line[1] == ' ' && line[2] != ' ' && line.TrimEnd().EndsWith(':'))
                service = line.Trim().TrimEnd(':');
            else if (line.Trim() == "container_name: " + container)
                return service;
        }

        return null;
    }

    private static string? FindComposeFile()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "containers", "testing", ComposeFileName);
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }
}
