namespace OoBDev.System.IO.Ports.Bridge;

public sealed class SystemSerialPortFactory : ISerialPortFactory
{
    public ISerialLink Open(BridgeOptions options) => new SystemSerialLink(options);
}
