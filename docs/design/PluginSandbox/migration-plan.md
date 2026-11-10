# Plugin sandbox: migration plan (proposal, awaiting owner confirmation)

Status: proposed 2026-11-10. Rule `layering-and-migration-plans`: nothing moves until the owner confirms this table.

## Question and answer

Does the sandbox need one assembly per platform? No: the spike runs on Windows and Linux from one assembly because the platform code is managed code plus P/Invoke, chosen at run time with `OperatingSystem.Is*`. Separate assemblies are still recommended, because the repository already splits platform-bound code that way (`OoBDev.System.IO.Bluetooth` and `OoBDev.System.IO.Bluetooth.Windows`), the Windows launcher needs unsafe native code that Linux consumers should not load, and each platform package can then be added or left out on its own.

## Proposed projects

| Project | Layer folder | Namespace | Contents | Unit |
|---------|--------------|-----------|----------|------|
| `OoBDev.System.Abstractions` (existing) | Common | `OoBDev.System.Sandbox` | `ISandboxedPlugin`, `ISandboxLauncher`, `JobLimits`, `LimitHit`, `PluginLifetime`, `FrameType`, `FrameFlags`, `ReceivedFrame` | package |
| `OoBDev.System.Sandbox` (new) | Framework | `OoBDev.System.Sandbox` | frame protocol (`FrameChannel`), `PluginHost`, `BrokeredPlugin`, grants, broker, link resolver, context broker, signed packages, pool, shared-instance lock, reconnect registry, `CommandLine`, `PlainPlugin`, `TryAdd` registration | package |
| `OoBDev.System.Sandbox.Windows` (new) | Framework | `OoBDev.System.Sandbox.Windows` | `SandboxedPlugin` (AppContainer + job object), `AppContainerLauncher`, `JobObject`, `NativeProcess` | package, Windows only |
| `OoBDev.System.Sandbox.Linux` (new) | Framework | `OoBDev.System.Sandbox.Linux` | `BubblewrapPlugin` | package, Linux only |
| `OoBDev.System.Sandbox.MacOS` (new) | Framework | `OoBDev.System.Sandbox.MacOS` | `SeatbeltPlugin` (unverified on a Mac) | package, macOS only |
| `OoBDev.System.Sandbox.Tests` and one test project per platform package | Framework | same + `.Tests` | tests move with their code; the plugin entry point `Program` stays in a small `OoBDev.System.Sandbox.Plugin` console project that the platform packages launch | tests |

## References that change

- Each platform package references `OoBDev.System.Abstractions` and the core `OoBDev.System.Sandbox` (for the frame protocol); the core never references a platform package.
- The platform package registers its `ISandboxLauncher` with `TryAddEnumerable`; `SandboxLauncher.Start` becomes an `ISandboxLauncher` resolved from DI, so an application references only the platform packages it ships on.
- `Program.cs` (the plugin side, with the probe, hog and brokered test modes) is split: the production entry point moves to the plugin project, the test modes stay in the test projects.

## Not decided

- Whether Linux and macOS share one `Sandbox.Unix` package (both are `Process` plus a wrapper command). Recommendation: keep them separate, they share no code beyond `CommandLine`.
- The spikes stay in `src/OoBDev.Spikes.slnx` until this plan is confirmed and executed (owner, 2026-11-10).
