# OoBDev.System.Sandbox

Throwaway proof of concept for the [sandboxed plugin host](../../../docs/design/PluginSandbox/README.md): the framed JSON protocol of [protocol-and-manifest.md](../../../docs/design/PluginSandbox/protocol-and-manifest.md) over a child process's stdin and stdout, the host's reaction to a bad frame (kill), and a Windows job object that caps process memory. Results: [spike-result.md](../../../docs/design/PluginSandbox/spike-result.md). Windows only; not packaged, not in the solution. Tests: `OoBDev.System.Sandbox.Tests` (Integration tier, they start real processes).

## Message rate

`MessageRateLimiter` drops frames over a per-second limit and flags a sustained excess; `PluginHost` sets it on `BrokeredPlugin.RateLimiter` from the manifest `limits.msgPerSec` (default 200, ceiling 1000).

## Heartbeat

`PluginHeartbeat.Check(plugin, timeout)` sends a heartbeat and disposes a plugin that does not answer in time.
