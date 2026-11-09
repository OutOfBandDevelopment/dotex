using System.Collections.Generic;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>The inbound half of the declarative schema: either a reference to a binary Kaitai Struct layout, or a list of text response patterns.</summary>
public sealed class InboundProtocol
{
    /// <summary>Path to a Kaitai Struct (.ksy) file, relative to the manifest's own location. Referenced only — not parsed by dev-term itself (see docs/design/UiDefinitions/device-manifests.md).</summary>
    public string? KaitaiFile { get; set; }

    public List<ResponsePattern> Patterns { get; set; } = [];

    /// <summary>A fixed-layout binary frame the device sends, decoded into live values (<see cref="ManifestFramePresenter"/>): the binary counterpart to <see cref="Patterns"/>.</summary>
    public FrameSchema? Frame { get; set; }

    /// <summary>
    /// Whether replies end in CR, LF, or CRLF (true, the default). False for a device whose replies
    /// have no terminator at all: whatever arrives in one read then counts as one complete reply
    /// (see <c>LineReplyPresenter.ConfigureTerminator</c>).
    /// </summary>
    public bool LineTerminated { get; set; } = true;
}
