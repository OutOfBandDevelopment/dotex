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
    private static readonly Dictionary<string, int> _startedByTests = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, DateTimeOffset> _readyUntil = new(StringComparer.Ordinal);
    private static readonly TimeSpan _readyCache = TimeSpan.FromSeconds(30);
    private static bool _exitHookRegistered;
    private static bool _stopAfterTests = true;
    private static IDockerCli _docker = new ProcessDockerCli();

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

    /// <summary>Polling interval while waiting for a health check.</summary>
    internal static TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Ensures the named containers (for example <c>oobd-test-redis</c>) are ready, or ends the tests of the class as Inconclusive.
    /// </summary>
    public static async Task EnsureAsync(TestContext testContext, params string[] containers)
    {
        ArgumentNullException.ThrowIfNull(testContext);
        var seconds = testContext.GetPropertyOrDefault("CONTAINER_STARTUP_TIMEOUT_SECONDS", DefaultStartupSeconds);
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

            var problem = await TryEnsureAsync(container, autostart, timeout.Token);
            if (problem is null)
            {
                lock (_gate)
                    _readyUntil[container] = DateTimeOffset.UtcNow + _readyCache;
                continue;
            }

            if (problem is not null)
                Assert.Inconclusive($"Container {container} is not available: {problem} (waited at most {seconds} s; CONTAINER_STARTUP_TIMEOUT_SECONDS changes the wait).");
        }
    }

    /// <summary>Stops the containers this test run started, unless <c>CONTAINER_STOP_AFTER_TESTS</c> is false.</summary>
    public static async Task ReleaseAsync(TestContext testContext, params string[] containers)
    {
        ArgumentNullException.ThrowIfNull(testContext);
        if (!Flag(testContext, "CONTAINER_STOP_AFTER_TESTS"))
            return;

        foreach (var container in containers)
        {
            bool stop;
            lock (_gate)
            {
                if (!_startedByTests.TryGetValue(container, out var users))
                    continue;
                stop = users <= 1;
                if (stop)
                    _startedByTests.Remove(container);
                else
                    _startedByTests[container] = users - 1;
            }

            if (stop)
            {
                lock (_gate)
                    _readyUntil.Remove(container);
                await Docker.StopAsync(container, TimeSpan.FromSeconds(10), CancellationToken.None);
            }
        }
    }

    private static void StopStartedContainers()
    {
        string[] started;
        lock (_gate)
            started = [.. _startedByTests.Keys];
        if (!_stopAfterTests)
            return;
        foreach (var container in started)
            Docker.StopAsync(container, TimeSpan.FromSeconds(10), CancellationToken.None).GetAwaiter().GetResult();
    }

    private static async Task<string?> TryEnsureAsync(string container, bool autostart, CancellationToken cancellationToken)
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

                lock (_gate)
                {
                    _startedByTests[container] = _startedByTests.GetValueOrDefault(container) + 1;
                    if (!_exitHookRegistered)
                    {
                        _exitHookRegistered = true;
                        AppDomain.CurrentDomain.ProcessExit += (_, _) => StopStartedContainers();
                    }
                }
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
        !bool.TryParse(testContext.GetPropertyOrDefault(name, "true"), out var value) || value;

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
