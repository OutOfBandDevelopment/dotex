using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// Enforces the grants on every resource request (REQ-003, REQ-004, REQ-007). The plugin has no network of its own, so a host call goes
/// through <see cref="FetchAsync"/>: exact host and port, and every redirect target is checked again. A file request must lie inside a granted
/// folder (and the folder must be <c>rw</c> to write). Every decision is recorded in the grant store's audit trail.
/// Not covered: links inside a granted folder that point outside it, which the caller must refuse when it opens the file.
/// </summary>
public sealed class GrantBroker(GrantStore grants, HttpMessageHandler? handler = null)
{
    private const int MaxRedirects = 3;
    private readonly GrantStore _grants = grants ?? throw new ArgumentNullException(nameof(grants));
    private readonly HttpMessageHandler _handler = handler ?? new SocketsHttpHandler { AllowAutoRedirect = false };

    /// <summary>Decides a file read or write.</summary>
    public BrokerDecision CheckFile(string pluginId, string path, bool write)
    {
        var grant = _grants.Find(pluginId);
        if (grant is null)
        {
            return Log(pluginId, "file", path, false, "no grant");
        }

        var full = LinkResolver.Real(path);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var folder = grant.Files.FirstOrDefault(f => Inside(full, LinkResolver.Real(f.Path), comparison));
        if (folder is null)
        {
            return Log(pluginId, "file", path, false, "outside the granted folders");
        }

        return write && folder.Access != "rw" ? Log(pluginId, "file", path, false, "folder is read only") : Log(pluginId, "file", path, true, write ? "write" : "read");
    }

    /// <summary>Decides a network connection to <paramref name="host"/> and <paramref name="port"/>.</summary>
    public BrokerDecision CheckNetwork(string pluginId, string host, int port)
    {
        var grant = _grants.Find(pluginId);
        if (grant is null)
        {
            return Log(pluginId, "net", $"{host}:{port}", false, "no grant");
        }

        return grant.Network.Any(n => string.Equals(n.Host, host, StringComparison.OrdinalIgnoreCase) && n.Port == port)
            ? Log(pluginId, "net", $"{host}:{port}", true, "granted")
            : Log(pluginId, "net", $"{host}:{port}", false, "host and port are not granted");
    }

    /// <summary>Fetches <paramref name="uri"/> for the plugin if its host and port are granted; each redirect is checked again.</summary>
    public async Task<BrokerFetch> FetchAsync(string pluginId, Uri uri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        using var client = new HttpClient(_handler, disposeHandler: false) { Timeout = TimeSpan.FromSeconds(10) };
        var target = uri;
        for (var hop = 0; hop <= MaxRedirects; hop++)
        {
            var decision = CheckNetwork(pluginId, target.Host, target.Port);
            if (!decision.Allowed)
            {
                return new BrokerFetch(decision, 0, []);
            }

            using var response = await client.GetAsync(target, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if ((int)response.StatusCode is >= 300 and < 400 && response.Headers.Location is { } next)
            {
                target = next.IsAbsoluteUri ? next : new Uri(target, next);
                continue;
            }

            var body = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            return new BrokerFetch(decision, (int)response.StatusCode, body);
        }

        return new BrokerFetch(Log(pluginId, "net", uri.ToString(), false, "too many redirects"), 0, []);
    }

    private static bool Inside(string path, string folder, StringComparison comparison)
    {
        var root = folder.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return string.Equals(path, folder.TrimEnd(Path.DirectorySeparatorChar), comparison) || path.StartsWith(root, comparison);
    }

    private BrokerDecision Log(string pluginId, string action, string target, bool allowed, string reason)
    {
        _grants.Record(pluginId, action, target, allowed, reason);
        return new BrokerDecision(allowed, reason);
    }
}
