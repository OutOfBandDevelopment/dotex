using OoBDev.System.IO.UsbHids;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// A Velleman K8055 USB experiment board at address 3 (vendor 0x10CF, product 0x5503).
/// </summary>
[UsbHid(0x10CF, 0x5503)]
public sealed class K8055Address3Device : K8055DeviceBase;
