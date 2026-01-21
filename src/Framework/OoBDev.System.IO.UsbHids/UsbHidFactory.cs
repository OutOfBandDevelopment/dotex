using HidSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.System.IO.UsbHids;

/// <summary>
/// Factory for creating USB HID device adapters based on <see cref="UsbHidAttribute"/> configuration.
/// </summary>
[DeviceTarget(typeof(UsbHidAttribute))]
public class UsbHidFactory : IImplictDeviceFactory
{
    /// <inheritdoc/>
    public bool CanGetDevice(object? definition) => definition?.GetType()?.GetCustomAttributes<UsbHidAttribute>()?.Any() ?? false;

    /// <inheritdoc/>
    public IDeviceAdapter? GetDevice(string devicePath, object? definition) =>
        GetDevices(definition).FirstOrDefault(d => string.Equals(d.Path, devicePath, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc/>
    public IDeviceAdapter? GetDevice(object? definition) => GetDevices(definition).FirstOrDefault();

    /// <inheritdoc/>
    public IEnumerable<IDeviceAdapter> GetDevices(object? definition)
    {
        if (definition == null) yield break;
        var def = definition.GetType();
        var config = def.GetCustomAttribute<UsbHidAttribute>();
        if (config == null) yield break;

        var devices = from device in DeviceList.Local.GetAllDevices().OfType<HidDevice>()
                      where device.VendorID == config.VendorId
                      where (device.ProductID & config.ProductMask) == (config.ProductId & config.ProductMask)
                      group device by device.DevicePath into hids
                      select hids.First();

        foreach (var device in devices)
            yield return new UsbHidDeviceAdapter(device);
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetDeviceNames() =>
        DeviceList.Local
                  .GetAllDevices()
                  .OfType<HidDevice>()
                  .Select(s => s.DevicePath)
                  .Distinct()
                  .OrderBy(s => s);
}
