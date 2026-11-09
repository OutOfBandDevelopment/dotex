namespace OoBDev.System.IO.Ports;

public sealed class SystemSerialPortFactory : ISerialPortFactory
{
    public ISerialPort Create(SerialDeviceAdapterOptions options) => new SystemSerialPort(options);
}
