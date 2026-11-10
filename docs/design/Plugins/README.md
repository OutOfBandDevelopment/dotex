# Plugins

Plugin contracts, discovery, isolation and what plugins can contribute. Moved from the DevTerm design folder on 2026-11-04. Application: [DevTerm](../../applications/devterm/README.md).

Two designs cover plugins and they belong together: this folder is the plugin model (what a plugin is and what it contributes); [PluginSandbox](../PluginSandbox/README.md) is how an untrusted plugin is isolated, granted rights and limited.

- [plugin-model.md](plugin-model.md) - plugin contracts, packaging and discovery, isolation and versioning, built-in versus plugin
- [plugin-contributed-panels.md](plugin-contributed-panels.md) - plugins that contribute control panels and menu entries
- [out-of-process-plugins.md](out-of-process-plugins.md) - plugins in any language hosted in a child process; the isolation itself is the [sandboxed plugin host](../PluginSandbox/architecture.md)

## How the two fit

| Question | Where |
|----------|-------|
| What can a plugin contribute (presenters, transports, panels)? | plugin-model, plugin-contributed-panels |
| How does a plugin run outside the host process? | out-of-process-plugins, then [PluginSandbox architecture](../PluginSandbox/architecture.md) |
| Which wire protocol and manifest does an out-of-process plugin use? | [protocol-and-manifest](../PluginSandbox/protocol-and-manifest.md) |
| What may a plugin touch, and who approves it? | [requirements](../PluginSandbox/requirements.md), [threat model](../PluginSandbox/threat-model.md) |
| What exists today? | [spike result](../PluginSandbox/spike-result.md), [migration plan](../PluginSandbox/migration-plan.md) |
