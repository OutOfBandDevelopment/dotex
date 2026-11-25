namespace OoBDev.System.IO.UsbTmc;

/// <summary>Creates LibUsbDotNet-backed devices.</summary>
public sealed class SystemUsbTmcDeviceFactory : IUsbTmcDeviceFactory
{
    /// <inheritdoc />
    public IUsbTmcDevice Create(UsbTmcDeviceAdapterOptions options) => new SystemUsbTmcDevice(options);
}
