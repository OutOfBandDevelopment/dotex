using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.TestUtilities;

/// <summary>
/// Refuses to let an Integration test touch anything that is not a throwaway test instance.
/// </summary>
public static class TestInstanceGuard
{
    private static readonly string[] _loopbackHosts = ["localhost", "127.0.0.1", "::1", "[::1]", "host.docker.internal", "."];
    private static readonly string[] _hostKeys = ["server", "data source", "host", "address", "addr", "network address", "endpoint", "hostname"];

    /// <summary>
    /// Whether every host named in the connection string or URL is a loopback or container host.
    /// </summary>
    /// <param name="connectionString">A key/value connection string or an absolute URL.</param>
    public static bool IsTestInstance(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var hosts = Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.Host)
            ? [uri.Host]
            : HostsFromPairs(connectionString);

        return hosts.Count > 0 && hosts.All(IsLoopback);
    }

    /// <summary>
    /// Fails the test (<see cref="Assert.Fail(string)"/>) when the connection string is not a test instance, otherwise returns it.
    /// </summary>
    /// <param name="connectionString">A key/value connection string or an absolute URL.</param>
    /// <returns>The same connection string.</returns>
    public static string Require(string connectionString)
    {
        if (!IsTestInstance(connectionString))
            Assert.Fail("Refusing to run: the connection string does not point at a local test instance.");
        return connectionString;
    }

    private static List<string> HostsFromPairs(string connectionString)
    {
        var hosts = new List<string>();
        foreach (var pair in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = pair.IndexOf('=', StringComparison.Ordinal);
            if (index <= 0 || !_hostKeys.Contains(pair[..index].Trim(), StringComparer.OrdinalIgnoreCase))
                continue;
            foreach (var entry in pair[(index + 1)..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                // "tcp:host\instance,1433" and "host:6379": keep the host part only; the next comma entry is a port or another endpoint.
                var host = entry.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase) ? entry[4..] : entry;
                host = host.Split('\\')[0];
                if (!host.StartsWith('[') && host.Count(c => c == ':') == 1)
                    host = host.Split(':')[0];
                if (!int.TryParse(host, out _))
                    hosts.Add(host);
            }
        }
        return hosts;
    }

    private static bool IsLoopback(string host) => _loopbackHosts.Contains(host, StringComparer.OrdinalIgnoreCase);
}
