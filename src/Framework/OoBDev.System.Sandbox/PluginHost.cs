using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Sandbox;

/// <summary>
/// The path from a package on disk to a running, brokered plugin: verify the signed package, require the owner's approved grant, take the
/// cross-process mutex for a shared instance, launch in the platform sandbox with only the granted folders reachable, and hand out leases from
/// the pool. Nothing is started unless every step passes.
/// </summary>
public sealed class PluginHost : IDisposable
{
    private readonly PackageVerifier _verifier;
    private readonly GrantStore _grants;
    private readonly GrantBroker _broker;
    private readonly ContextBroker _contexts;
    private readonly Func<string, IReadOnlyCollection<string>, IReadOnlyCollection<string>, JobLimits, ISandboxedPlugin> _launch;
    private readonly PluginPool<BrokeredPlugin> _pool;

    /// <summary>Creates a host; <paramref name="launch"/> starts a sandboxed plugin from a command line and the read and write folders (normally <see cref="ISandboxLauncher.Start"/>).</summary>
    public PluginHost(PackageVerifier verifier, GrantStore grants, GrantBroker broker, ContextBroker contexts, TimeProvider time, TimeSpan idleTimeout, Func<string, IReadOnlyCollection<string>, IReadOnlyCollection<string>, JobLimits, ISandboxedPlugin> launch)
    {
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
        _grants = grants ?? throw new ArgumentNullException(nameof(grants));
        _broker = broker ?? throw new ArgumentNullException(nameof(broker));
        _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));
        _launch = launch ?? throw new ArgumentNullException(nameof(launch));
        _pool = new PluginPool<BrokeredPlugin>(time, idleTimeout);
    }

    /// <summary>Number of shared plugin processes running.</summary>
    public int SharedCount => _pool.SharedCount;

    /// <summary>Ends shared instances idle past the timeout; call on a timer.</summary>
    public int Sweep() => _pool.Sweep();

    /// <summary>Verifies and starts the package, or says why not. <paramref name="commandArguments"/> is the plugin's entry command line.</summary>
    public HostStart Start(string packageDirectory, string commandArguments, PluginLifetime lifetime)
    {
        var verification = _verifier.Verify(packageDirectory);
        if (!verification.Accepted || verification.Manifest is not { } manifest)
        {
            return new HostStart(HostRefusal.PackageRejected, verification.Refusal.ToString(), null);
        }

        var grant = _grants.Find(manifest.Id);
        if (grant is null)
        {
            return new HostStart(HostRefusal.NoGrant, manifest.Id, null);
        }

        if (!Within(grant, manifest))
        {
            return new HostStart(HostRefusal.GrantExceedsManifest, manifest.Id, null);
        }

        var read = grant.Files.Select(f => f.Path).ToList();
        var write = grant.Files.Where(f => f.Access == "rw").Select(f => f.Path).ToList();
        try
        {
            var lease = _pool.Acquire(manifest.Id, grant.Hash, lifetime, () => Create(manifest.Id, grant, commandArguments, read, write, lifetime, manifest.Limits));
            return new HostStart(HostRefusal.None, string.Empty, lease);
        }
        catch (TimeoutException)
        {
            return new HostStart(HostRefusal.InUseElsewhere, manifest.Id, null);
        }
    }

    /// <summary>Ends every shared plugin.</summary>
    public void Dispose() => _pool.Dispose();

    private static bool Within(Grant grant, PluginManifest manifest) =>
        grant.Files.All(f => manifest.Permissions.Files?.Any(r => r.Path == f.Path && (r.Access == f.Access || (r.Access == "rw" && f.Access == "ro"))) ?? false)
        && grant.Network.All(n => manifest.Permissions.Network?.Contains(n) ?? false);

    private BrokeredPlugin Create(string pluginId, Grant grant, string commandArguments, List<string> read, List<string> write, PluginLifetime lifetime, PluginLimits? limits)
    {
        InstanceHold? hold = null;
        if (lifetime == PluginLifetime.Shared)
        {
            hold = InstanceHold.TryAcquire(SharedInstanceLock.Name(pluginId, grant.Hash), TimeSpan.FromMilliseconds(200)) ?? throw new TimeoutException();
        }

        try
        {
            var plugin = new BrokeredPlugin(pluginId, _launch(commandArguments, read, write, (limits ?? new PluginLimits()).ToJobLimits()), _broker, _contexts, hold);
            plugin.RateLimiter = new MessageRateLimiter((limits ?? new PluginLimits()).EffectiveMsgPerSec);
            plugin.WaitReady();
            return plugin;
        }
        catch
        {
            hold?.Dispose();
            throw;
        }
    }
}
