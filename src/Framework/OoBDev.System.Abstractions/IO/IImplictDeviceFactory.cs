using System.Collections.Generic;

namespace OoBDev.System.IO;

public interface IImplictDeviceFactory : IDeviceFactory
{
    IDeviceAdapter? GetDevice(object? definition);
    IEnumerable<IDeviceAdapter> GetDevices(object? definition);
}
