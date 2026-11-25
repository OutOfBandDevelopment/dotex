namespace OoBDev.System.IO.Ports.Bridge;

public interface IServiceAdvertiser
{
    /// <summary>Announces a listening bridge on the network. Dispose the result to withdraw it.</summary>
    public IDisposable Advertise(BridgeOptions bridge, int port);
}
