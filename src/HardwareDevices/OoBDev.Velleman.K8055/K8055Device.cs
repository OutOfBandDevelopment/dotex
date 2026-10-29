using OoBDev.System.IO.UsbHids;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// A Velleman K8055 USB experiment board at address 0 (vendor 0x10CF, product 0x5500; both jumpers open). It streams its inputs
/// without being asked and takes <see cref="K8055Command"/> writes for its outputs and counters.
/// </summary>
[UsbHid(0x10CF, 0x5500)]
public sealed class K8055Device : K8055DeviceBase;
