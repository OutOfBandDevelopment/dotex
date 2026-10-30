namespace OoBDev.System.IO.UsbHids;

/// <summary>Creates HidSharp-backed devices.</summary>
public sealed class SystemUsbHidDeviceFactory : IUsbHidDeviceFactory
{
    /// <inheritdoc />
    public IUsbHidDevice Create(UsbHidEndpointOptions options) => new SystemUsbHidDevice(options);
}
