using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO;

/// <summary>
/// Represents an interface for initializing a device definition asynchronously.
/// </summary>
public interface IDeviceDefinitionInitialize
{
    /// <summary>
    /// Initializes the device definition asynchronously.
    /// </summary>
    /// <param name="device">The device adapter used for initialization.</param>
    /// <param name="token">A cancellation token that can be used to cancel the initialization operation.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    Task InitializeAsync(IDeviceAdapter device, CancellationToken token);
}
