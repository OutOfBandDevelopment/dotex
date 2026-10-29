namespace OoBDev.ZWave;

/// <summary>
/// What is known about one node.
/// </summary>
public sealed class ZWaveNodeRecord
{
    /// <summary>Gets the node id.</summary>
    public int Node { get; init; }

    /// <summary>Gets or sets the protocol summary (listening, routing, device class).</summary>
    public string? Protocol { get; set; }

    /// <summary>Gets or sets <c>FAILED</c> or <c>ok</c> once the controller was asked.</summary>
    public string? Failed { get; set; }

    /// <summary>Gets or sets the supported command classes from the node information frame.</summary>
    public string? Classes { get; set; }

    /// <summary>Gets or sets the last report the node sent.</summary>
    public string? Last { get; set; }

    /// <summary>Gets or sets <c>manufacturer:type:id</c> in hex from a Manufacturer Specific report.</summary>
    public string? Ids { get; set; }
}
