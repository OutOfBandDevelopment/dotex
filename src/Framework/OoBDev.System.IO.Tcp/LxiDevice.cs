namespace OoBDev.System.IO.Tcp;

/// <summary>One LAN instrument found by <see cref="LxiDiscovery"/>.</summary>
/// <param name="Host">Dotted IPv4 address.</param>
/// <param name="Vxi11Port">The port its VXI-11 core channel listens on (from the portmapper reply).</param>
/// <param name="ScpiPort">A raw-SCPI socket port that answered <c>*IDN?</c>, or 0 when none of the probed ports did.</param>
/// <param name="Identity">The <c>*IDN?</c> reply, or empty when no raw port answered.</param>
public sealed record LxiDevice(string Host, int Vxi11Port, int ScpiPort, string Identity)
{
    public string Display => Identity.Length > 0 ? $"{Identity}  ({Host}:{ScpiPort})" : $"{Host}  (VXI-11 only, no raw SCPI port found)";
}
