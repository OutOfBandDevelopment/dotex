namespace OoBDev.System.IO.Ports.Bridge;

public sealed class MdnsOptions
{
    /// <summary>Off by default: the bridge has no authentication, so announcing it on the LAN is opt-in.</summary>
    public bool Enabled { get; set; }

    public string ServiceType { get; set; } = "_ser2net._tcp";
}
