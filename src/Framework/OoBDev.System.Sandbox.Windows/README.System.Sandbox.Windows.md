# OoBDev.System.Sandbox.Windows

The Windows launcher for `ISandboxLauncher` (see [OoBDev.System.Sandbox](../OoBDev.System.Sandbox/README.System.Sandbox.md)). It sandboxes with AppContainer inside a job object (needs Windows).

```csharp
services.AddWindowsSandbox(pluginAssemblyPath);
```

## Survive

`new SandboxedPlugin(..., killOnClose: false)` starts the plugin in a job that does not kill it when the host's handle closes; `Detach()` stops the host watching and releases the handles while the plugin keeps running. An AppContainer cannot create a pipe in the global namespace, so a Survive plugin creates `LOCAL<name>` (with an ACL naming the user) and the host connects with `SurviveConnection(AppContainerLauncher.PluginPipeName(name), ...)`.
