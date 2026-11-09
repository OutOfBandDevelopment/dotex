using System.Collections.Generic;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>Loose key/value defaults for pre-filling connection setup (e.g. Type="serial", Options=[{Key:"Baud",Value:"9600"}]) — a hint, not a hard requirement.</summary>
public sealed class TransportHint
{
    public required string Type { get; set; }

    // A List<TransportOption>, not a Dictionary<string,string>: XmlSerializer has no native
    // support for IDictionary (throws NotSupportedException at type-reflection time), while a
    // plain list of a simple Key/Value class works natively for both JSON and XML with no
    // special-casing either serializer.
    public List<TransportOption> Options { get; set; } = [];
}
