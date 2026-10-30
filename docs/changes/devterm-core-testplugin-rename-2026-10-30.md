# DevTerm: Sample plugin renamed to a test fixture (2026-10-30)

`OoBDev.DevTerm.Plugins.Sample` is now `OoBDev.DevTerm.Core.TestPlugin` (folder, project, namespace, readme, `plugin.json` assembly name, slnx, loader-test references). It stays a separate assembly because the loader tests load it from `plugins/sample` like a real plugin. Owner decision 2026-10-30 (Q1, option A).

Verification: `OoBDev.DevTerm.Core.Tests` Unit run, 1063 passed, 5 skipped, 0 failed. Earlier change documents keep the old name as history.
