namespace OoBDev.System.IO.UsbTmc;

/// <summary>
/// Creates the <see cref="IUsbTmcDevice"/> a <see cref="UsbTmcDeviceAdapter"/> opens. Indirection
/// point that lets tests substitute a fake factory/device instead of touching real hardware.
/// </summary>
public interface IUsbTmcDeviceFactory
{
    /// <summary>Creates a device for the given options.</summary>
    IUsbTmcDevice Create(UsbTmcDeviceAdapterOptions options);
}
