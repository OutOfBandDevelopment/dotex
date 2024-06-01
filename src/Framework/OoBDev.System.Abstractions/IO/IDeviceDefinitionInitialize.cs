using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO;

public interface IDeviceDefinitionInitialize
{
    Task InitializeAsync(IDeviceAdapter device, CancellationToken token);
}
