# OoBDev.System.Sandbox.Linux

The Linux launcher for `ISandboxLauncher` (see [OoBDev.System.Sandbox](../OoBDev.System.Sandbox/README.System.Sandbox.md)). It sandboxes with bubblewrap (needs /usr/bin/bwrap).

```csharp
services.AddLinuxSandbox(pluginAssemblyPath);
```

## Survive

`new BubblewrapPlugin(..., surviveCaller: true)` (or `ISandboxLauncher.Start(..., surviveCaller: true)`) leaves out `--die-with-parent`, so the sandbox keeps running after `Detach()`. The plugin listens on a unix socket path inside a folder granted read-write, and the next caller connects with `SurviveConnection(path, key, ...)`. Verified by `LinuxSurviveTests` in the bubblewrap container.
