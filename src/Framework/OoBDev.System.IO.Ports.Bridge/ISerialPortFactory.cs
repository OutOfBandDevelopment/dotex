namespace OoBDev.System.IO.Ports.Bridge;

public interface ISerialPortFactory
{
    public ISerialLink Open(BridgeOptions options);
}
