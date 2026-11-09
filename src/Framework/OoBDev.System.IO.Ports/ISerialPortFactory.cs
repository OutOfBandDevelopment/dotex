namespace OoBDev.System.IO.Ports;

/// <summary>
/// Creates the <see cref="ISerialPort"/> a <see cref="SerialDeviceAdapter"/> opens. Indirection
/// point that lets tests substitute a fake factory/port instead of touching real hardware.
/// </summary>
public interface ISerialPortFactory
{
    ISerialPort Create(SerialDeviceAdapterOptions options);
}
