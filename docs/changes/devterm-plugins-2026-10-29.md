# DevTerm plugins and presenter catalog

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- `OoBDev.DevTerm.Core` gains `PresenterCatalog`, the plugin loader (in-process assemblies and approval-gated out-of-process plugins), `PluginTrust` (hash-based approvals), `LivePlugins`, `ExternalProcessPresenter` and `AddDevTermCore`.
- New `OoBDev.DevTerm.Plugins.Sample` (the loader test fixture and template) and `examples/devterm` (Python, Java and Go out-of-process plugins).
- `OoBDev.DevTerm.Core.Tests`: 195 passing, 5 inconclusive (Java and Go are not installed here, so those host tests and similar ones skip).
- Not done: the KeyValue plugin, control surfaces and servers.
