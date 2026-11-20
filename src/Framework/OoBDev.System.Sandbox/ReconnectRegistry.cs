using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>Why a reconnect was refused.</summary>
public enum ReconnectRefusal
{
    /// <summary>Accepted.</summary>
    None,

    /// <summary>The token is not known (never issued, already used or ended).</summary>
    Unknown,

    /// <summary>The token belongs to another plugin.</summary>
    WrongPlugin,

    /// <summary>The reconnect window passed.</summary>
    Expired,
}

/// <summary>A new session for a recovered caller.</summary>
/// <param name="Refusal">None on success.</param>
/// <param name="SessionNumber">Counts up per plugin instance; the plugin sees <c>lifecycle.reconnected</c> with it.</param>
/// <param name="SessionKey">A fresh frame key; sequences restart at 1.</param>
public sealed record ReconnectResult(ReconnectRefusal Refusal, int SessionNumber, byte[]? SessionKey);

/// <summary>
/// The Survive lifetime of REQ-011: when a caller dies the plugin is kept for a window; a recovered caller presents the token the old
/// session left and gets a new session with a new key. The token is single use and only its hash is kept. When the window passes with no
/// caller, <see cref="Sweep"/> reports the plugin as expired so the host watchdog can kill it.
/// </summary>
public sealed class ReconnectRegistry
{
    private readonly TimeProvider _time;
    private readonly TimeSpan _window;
    private readonly Lock _gate = new();
    private readonly Dictionary<string, Entry> _entries = [];

    /// <summary>Creates the registry with the reconnect <paramref name="window"/>.</summary>
    public ReconnectRegistry(TimeProvider time, TimeSpan window)
    {
        ArgumentNullException.ThrowIfNull(time);
        _time = time;
        _window = window;
    }

    /// <summary>Called when the caller is gone: returns the token the caller's protected store must have kept.</summary>
    public string Detach(string pluginId, int lastSessionNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        lock (_gate)
        {
            _entries[Hash(token)] = new Entry(pluginId, lastSessionNumber, _time.GetUtcNow() + _window);
        }

        return token;
    }

    /// <summary>Resumes the plugin for a recovered caller; the token works once.</summary>
    public ReconnectResult Reconnect(string pluginId, string token)
    {
        lock (_gate)
        {
            var hash = Hash(token ?? string.Empty);
            if (!_entries.TryGetValue(hash, out var entry))
            {
                return new ReconnectResult(ReconnectRefusal.Unknown, 0, null);
            }

            if (!string.Equals(entry.PluginId, pluginId, StringComparison.Ordinal))
            {
                return new ReconnectResult(ReconnectRefusal.WrongPlugin, 0, null);
            }

            _entries.Remove(hash);
            return _time.GetUtcNow() > entry.Expires
                ? new ReconnectResult(ReconnectRefusal.Expired, 0, null)
                : new ReconnectResult(ReconnectRefusal.None, entry.Session + 1, RandomNumberGenerator.GetBytes(32));
        }
    }

    /// <summary>Removes tokens whose window passed and returns the plugin ids to kill.</summary>
    public IReadOnlyList<string> Sweep()
    {
        lock (_gate)
        {
            var now = _time.GetUtcNow();
            var expired = new List<string>();
            foreach (var (hash, entry) in _entries)
            {
                if (now > entry.Expires)
                {
                    expired.Add(hash);
                }
            }

            var plugins = new List<string>();
            foreach (var hash in expired)
            {
                plugins.Add(_entries[hash].PluginId);
                _entries.Remove(hash);
            }

            return plugins;
        }
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private sealed record Entry(string PluginId, int Session, DateTimeOffset Expires);
}
