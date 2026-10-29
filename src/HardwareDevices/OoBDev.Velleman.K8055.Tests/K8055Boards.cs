using OoBDev.Devices.TestUtilities;
using System.IO;

namespace OoBDev.Velleman.K8055.Tests;

internal static class K8055Boards
{
    /// <summary>Opens the board at <paramref name="address"/> (0 to 3, product 0x5500 to 0x5503), or ends the test Inconclusive.</summary>
    public static Stream Require(int address) => DevicePreflight.RequireUsbHid(address switch
    {
        0 => new K8055Device(),
        1 => new K8055Address1Device(),
        2 => new K8055Address2Device(),
        3 => new K8055Address3Device(),
        _ => throw new global::System.ArgumentOutOfRangeException(nameof(address), address, "A K8055 address is 0 to 3."),
    });
}
