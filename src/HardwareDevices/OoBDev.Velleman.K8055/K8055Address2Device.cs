using OoBDev.System.IO.UsbHids;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// A Velleman K8055 USB experiment board at address 2 (vendor 0x10CF, product 0x5502).
/// </summary>
[UsbHid(0x10CF, 0x5502)]
public sealed class K8055Address2Device : K8055DeviceBase;
