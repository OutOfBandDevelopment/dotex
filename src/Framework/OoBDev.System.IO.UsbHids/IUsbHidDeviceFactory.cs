namespace OoBDev.System.IO.UsbHids;

/// <summary>
/// Creates the <see cref="IUsbHidDevice"/> a <see cref="UsbHidEndpointDeviceAdapter"/> opens. Indirection point
/// that lets tests substitute a fake factory/device instead of touching real hardware.
/// </summary>
public interface IUsbHidDeviceFactory
{
    /// <summary>Creates a device for the given options; it is not opened yet.</summary>
    /// <param name="options">The vendor, product and optional disambiguators.</param>
    /// <returns>A closed device.</returns>
    IUsbHidDevice Create(UsbHidEndpointOptions options);
}
