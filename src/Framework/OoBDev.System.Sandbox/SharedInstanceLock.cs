using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// The named mutex that serialises start and join of a shared plugin instance across processes (REQ-011). The grant hash is part of the
/// name, so callers with different approved grants never meet. A holder that died leaves the mutex abandoned, which tells the next caller
/// to restart the plugin under the normal restart policy. A named mutex exists only while some process holds a handle, so a caller that has
/// joined keeps its <see cref="Mutex"/> open for as long as it uses the instance; without one the abandonment is lost with the last handle.
/// </summary>
public static partial class SharedInstanceLock
{
    /// <summary>The mutex name: per user (<c>Local</c>) or machine wide (<c>Global</c>, for <paramref name="singleton"/> plugins that own a device).</summary>
    public static string Name(string pluginId, string grantHash, bool singleton = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
        ArgumentException.ThrowIfNullOrWhiteSpace(grantHash);
        if (!PluginIdPattern().IsMatch(pluginId))
        {
            throw new ArgumentException("A plugin id is letters, digits, '-', '_' and '.' only.", nameof(pluginId));
        }

        // A singleton is one per machine whatever the grants, so its name leaves the grant hash out.
        var suffix = singleton ? string.Empty : "." + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(grantHash)))[..16];
        return $"{(singleton ? "Global" : "Local")}\\OoBDev.Plugin.{pluginId}{suffix}";
    }

    /// <summary>Tries to take the mutex within <paramref name="timeout"/>; null when another caller holds it. Release from the same thread.</summary>
    public static Entry? TryEnter(string name, TimeSpan timeout)
    {
        var mutex = new Mutex(false, name);
        try
        {
            return new Entry(mutex, mutex.WaitOne(timeout) ? false : throw new TimeoutException());
        }
        catch (AbandonedMutexException)
        {
            return new Entry(mutex, abandoned: true);
        }
        catch (TimeoutException)
        {
            mutex.Dispose();
            return null;
        }
    }

    [GeneratedRegex("^[A-Za-z0-9_.-]+$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PluginIdPattern();

    /// <summary>A held mutex; dispose releases it.</summary>
    public sealed class Entry : IDisposable
    {
        private readonly Mutex _mutex;

        internal Entry(Mutex mutex, bool abandoned)
        {
            _mutex = mutex;
            Abandoned = abandoned;
        }

        /// <summary>True when the previous holder ended without releasing, so the shared instance must be restarted.</summary>
        public bool Abandoned { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }
}
