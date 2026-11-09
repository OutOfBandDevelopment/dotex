using System.Collections.Generic;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>
/// A complete, no-code description of a device: identity, a transport hint, the declarative
/// command/response schema from docs/design/UiDefinitions/device-control-modules.md, and a control-panel
/// <see cref="UiDefinition"/> — either inline or loaded from <see cref="UiFile"/> by
/// <see cref="DeviceManifestLoader"/>. See docs/design/UiDefinitions/device-manifests.md.
/// </summary>
public sealed class DeviceManifest
{
    public required string Name { get; set; }

    public string? Vendor { get; set; }

    public string? Version { get; set; }

    public string? Description { get; set; }

    public TransportHint? Transport { get; set; }

    public InboundProtocol? Inbound { get; set; }

    public List<OutboundCommand> OutboundCommands { get; set; } = [];

    /// <summary>
    /// Appended to every outbound command's formatted template before it's sent (e.g. <c>"\n"</c>);
    /// empty by default, for a manifest whose templates already carry their own terminator.
    /// </summary>
    public string Terminator { get; set; } = string.Empty;

    /// <summary>Inline UI definition (single-file mode). If null, <see cref="DeviceManifestLoader"/> loads it from <see cref="UiFile"/> instead.</summary>
    public UiDefinition? Ui { get; set; }

    /// <summary>Path to an external UI definition file (.json or .xml), relative to the manifest's own location (package mode).</summary>
    public string? UiFile { get; set; }
}
