namespace OoBDev.System.IO.Ports.Bridge;

public sealed class Ser2NetConfig
{
    public List<BridgeOptions> Bridges { get; set; } = [];

    public MdnsOptions Mdns { get; set; } = new();
}
