namespace OoBDev.System.IO.Bluetooth.Windows;

public sealed class WindowsBleConnectionFactory : IBleConnectionFactory
{
    public IBleConnection Create(BleDeviceAdapterOptions options) => new WindowsBleConnection(options);
}
