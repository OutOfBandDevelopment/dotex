namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>Opens the system serial port. A bridge selected by USB id is resolved on every open, so an unplug and replug on another COM number reconnects.</summary>
public sealed class SystemSerialPortFactory(IUsbPortLister? lister = null) : ISerialPortFactory
{
    private readonly IUsbPortLister _lister = lister ?? new RegistryUsbPortLister();

    public ISerialLink Open(BridgeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var comPort = options.SelectsByUsbId ? UsbPortResolver.Resolve(options, _lister.List()) : options.ComPort;
        return new SystemSerialLink(options with { ComPort = comPort });
    }
}
