# OoBDev.System.Sandbox

Throwaway proof of concept for the [sandboxed plugin host](../../../docs/design/PluginSandbox/README.md): the framed JSON protocol of [protocol-and-manifest.md](../../../docs/design/PluginSandbox/protocol-and-manifest.md) over a child process's stdin and stdout, the host's reaction to a bad frame (kill), and a Windows job object that caps process memory. Results: [spike-result.md](../../../docs/design/PluginSandbox/spike-result.md). Windows only; not packaged, not in the solution. Tests: `OoBDev.System.Sandbox.Tests` (Integration tier, they start real processes).

## Message rate

`MessageRateLimiter` drops frames over a per-second limit and flags a sustained excess; `PluginHost` sets it on `BrokeredPlugin.RateLimiter` from the manifest `limits.msgPerSec` (default 200, ceiling 1000).

## Heartbeat

`PluginHeartbeat.Check(plugin, timeout)` sends a heartbeat and disposes a plugin that does not answer in time.

## Persisted grants

`new GrantStore(TimeProvider.System, path)` loads the approved grants from `path` and rewrites the file atomically on every approval or revoke; the audit trail is appended as JSON lines to `path + ".audit"`. Pass the same store to `GrantBroker` and `PluginHost`, so a plugin approved in one run keeps its grants in the next and a revoke stays revoked after a restart. Without a path the store lives in memory only. The files are not yet encrypted or access-protected.

## Idle heartbeat sweep

`PluginHost.Sweep(TimeSpan heartbeatTimeout)` ends idle instances past the idle timeout and sends a heartbeat to every other idle shared instance. One that does not answer in time is ended and removed from the pool, so the next `Start` launches a fresh process (the restart policy). A leased instance is never probed, because the stdio stream belongs to the caller; `PluginPool<T>.Sweep(Func<T, bool>)` is the generic form. Call it from the host's timer. The pool is locked while it asks, so keep the timeout short.
