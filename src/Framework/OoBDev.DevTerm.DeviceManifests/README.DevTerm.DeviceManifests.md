# OoBDev.DevTerm.DeviceManifests

Declarative device description for DevTerm: a YAML/JSON manifest (transport hints, outbound commands with parameters, inbound response patterns and frame layouts) is loaded, validated and turned into a control surface, a reply presenter, a frame presenter and a generated `UiDefinition`.

| Area | Types |
|------|-------|
| Model and I/O | `DeviceManifest`, `DeviceManifestLoader`, `DeviceManifestSerializer`, `DeviceManifestWriter`, `DeviceManifestValidator`, `ManifestCatalog` |
| Runtime | `ManifestControlSurface`, `ManifestReplyPresenter`, `ManifestFramePresenter`, `ManifestUiBuilder`, `ManifestPanel` |
| Frames | `FrameSchema`, `FrameField`, `FrameLayout`, `FrameDecoder`, `KsyImporter` (Kaitai `.ksy` subset) |
| Preview data | `ValuePathCatalog`, `SampleDataGenerator`, `RecordedSamples` |

Pre-packaged manifests under `Manifests/` are copied to `manifests\{device}\` next to the app. The manifest editor (`Editing`: `ManifestEditorViewModel`, the form classes, `ExpressionPickerViewModel`) is in; WPF and web forms rendering is separate.

Tests: `OoBDev.DevTerm.DeviceManifests.Tests`.
