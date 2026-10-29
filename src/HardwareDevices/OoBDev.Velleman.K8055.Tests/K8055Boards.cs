using OoBDev.Devices.TestUtilities;
using System.IO;

namespace OoBDev.Velleman.K8055.Tests;

internal static class K8055Boards
{
    /// <summary>Opens the board at whichever address (0 to 3) is plugged in, or ends the test Inconclusive.</summary>
    public static Stream Require()
    {
        object[] definitions = [new K8055Device(), new K8055Address1Device(), new K8055Address2Device(), new K8055Address3Device()];
        foreach (var definition in definitions)
        {
            if (new OoBDev.System.IO.UsbHids.UsbHidFactory().GetDevices(definition).GetEnumerator().MoveNext())
                return DevicePreflight.RequireUsbHid(definition);
        }

        return DevicePreflight.RequireUsbHid(definitions[0]);
    }
}
