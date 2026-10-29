# Sandboxed Plugin Host — API design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

**Status:** Draft (intent only; fixed after the threat model) · **Date:** 2026-10-29

## Sketch

```csharp
public interface IPluginHost
{
    Task<PluginInstance> StartAsync(PluginPackage package, PluginGrant grant, CancellationToken ct);
}
public interface IPluginBroker { Task<BrokerResult> InvokeAsync(string pluginId, BrokerRequest request, CancellationToken ct); }
public interface IPluginPackageVerifier { VerificationResult Verify(Stream package); }
public sealed record PluginGrant(IReadOnlySet<string> Hosts, IReadOnlySet<string> Paths, PluginLimits Limits);
public sealed record PluginLimits(TimeSpan WallTime, long MemoryBytes, int MaxProcesses, double CpuShare);
```

Manifest: JSON with `id`, `version`, `entry`, `permissions[]` (kind, target, reason). Protocol: length-prefixed JSON messages over a pipe or Unix socket, with `protocol` version in the first frame. Signature verification uses `OoBDev.Identity.Jws` ideas only as reference; package signing algorithm is decided in phase 1.

Providers follow the repo pattern: `IPluginLauncher` per platform behind a factory, `TryAdd*` registration.
