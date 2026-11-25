# OoBDev.System.Sandbox.Windows

The Windows launcher for `ISandboxLauncher` (see [OoBDev.System.Sandbox](../OoBDev.System.Sandbox/README.System.Sandbox.md)). It sandboxes with AppContainer inside a job object (needs Windows).

```csharp
services.AddWindowsSandbox(pluginAssemblyPath);
```

## Arbitrary commands

`SandboxedCommand` (`StartCommand`) runs any program under the same isolation (no network, only the granted folders, the job limits) and returns its plain stdio streams as an `ISandboxedProcess`; DevTerm process plugins with a `permissions` block use it ([design](../../../docs/design/Plugins/sandboxed-launch.md)).

## Survive

`new SandboxedPlugin(..., killOnClose: false)` starts the plugin in a job that does not kill it when the host's handle closes; `Detach()` stops the host watching and releases the handles while the plugin keeps running. An AppContainer cannot create a pipe in the global namespace, so a Survive plugin creates `LOCAL<name>` (with an ACL naming the user) and the host connects with `SurviveConnection(AppContainerLauncher.PluginPipeName(name), ...)`.
