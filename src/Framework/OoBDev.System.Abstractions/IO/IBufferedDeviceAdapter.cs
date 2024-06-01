namespace OoBDev.System.IO;

public interface IBufferedDeviceAdapter : IDeviceAdapter
{
    int BytesToRead { get; }
}
