# OoBDev.DevTerm.Plugins.KeyValue

Example in-process DevTerm plugin: the `keyvalue` presenter decodes `name=value [unit]` lines into a `StructuredMessage` and renders one `name = value unit` row per field. Try it against the loopback device's `STATUS?`.

Tests: `OoBDev.DevTerm.Plugins.KeyValue.Tests`.
