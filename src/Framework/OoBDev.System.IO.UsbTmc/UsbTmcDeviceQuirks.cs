using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.System.IO.UsbTmc;

/// <summary>
/// Per-device deviations from USBTMC 1.0/USB488 that <see cref="UsbTmcDeviceAdapter"/> has to work
/// around, keyed by VID:PID. Explicit <see cref="UsbTmcDeviceAdapterOptions"/> values always override
/// what's here.
/// </summary>
public sealed record UsbTmcDeviceQuirks(int RequestDelayMs, bool SupportsRemoteControl)
{
    private const int _rigolVendorId = 0x1AB1;

    // Shared by the Rigol DS1000E/D oscilloscopes (DS1102E: "DS1000 SERIES") and the DG1000
    // function generators (DG1022: "DG3000 SERIES") - so every quirk here applies to both.
    private const int _rigolDs1000DgProductId = 0x0588;

    public static UsbTmcDeviceQuirks None { get; } = new(RequestDelayMs: 0, SupportsRemoteControl: true);

    public static UsbTmcDeviceQuirks For(int vendorId, int productId) =>
        (vendorId, productId) switch
        {
            // RequestDelayMs: confirmed against a real DG1022: a
            // REQUEST_DEV_DEP_MSG_IN sent back-to-back with the query's DEV_DEP_MSG_OUT is never
            // answered - every query times out, reproducibly - while even a ~1 ms gap makes every
            // query answer normally. 20 ms is margin, not a measured minimum. The DS1102E
            // shares this VID:PID but doesn't need it (bench-checked with a 0 ms gap, same
            // report) - it just pays the 20 ms, since the two can't be told apart by VID:PID.
            //
            // SupportsRemoteControl: libsigrok blacklists RL1 for this VID:PID ("publishes RL1
            // support, but doesn't support it"); a real DG1022 timed out GO_TO_LOCAL too.
            (_rigolVendorId, _rigolDs1000DgProductId) => new(RequestDelayMs: 20, SupportsRemoteControl: false),
            _ => None,
        };
}
