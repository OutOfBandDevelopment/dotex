# OoBDev.System.Sandbox

Throwaway proof of concept for the [sandboxed plugin host](../../../docs/design/PluginSandbox/README.md): the framed JSON protocol of [protocol-and-manifest.md](../../../docs/design/PluginSandbox/protocol-and-manifest.md) over a child process's stdin and stdout, the host's reaction to a bad frame (kill), and a Windows job object that caps process memory. Results: [spike-result.md](../../../docs/design/PluginSandbox/spike-result.md). Windows only; not packaged, not in the solution. Tests: `OoBDev.System.Sandbox.Tests` (Integration tier, they start real processes).
