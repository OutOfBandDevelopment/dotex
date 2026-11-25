namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>
/// One recognizable response shape for a text protocol: a regex tested against every complete
/// inbound line (a correlated reply or an unsolicited one alike). On a match, the line publishes
/// live values a panel's indicators and charts read (<c>IStructuredPresenter.ValuesChanged</c>): the
/// pattern's <see cref="Name"/> gets the first capture group (the whole match when there's none),
/// and every named group (<c>(?&lt;chA&gt;...)</c>) gets its own value under its group name.
/// </summary>
public sealed class ResponsePattern
{
    public required string Name { get; set; }

    public required string Match { get; set; }

    /// <summary>
    /// An example reply line this pattern matches (<c>"MEAS 12.5 V"</c>), optional. The sample-data generator reads each
    /// published value out of it, so a preview or the picker shows realistic values instead of a generated walk.
    /// </summary>
    public string? Example { get; set; }
}
