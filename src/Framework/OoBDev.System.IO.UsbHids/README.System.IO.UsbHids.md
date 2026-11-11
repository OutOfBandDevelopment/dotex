# OoBDev.System.IO.UsbHids

USB HID (Human Interface Device) device abstraction and management library.

## Description

This package provides a factory-based abstraction layer for USB HID devices using the HidSharp library. It enables easy discovery, connection, and communication with USB HID devices through a unified interface.

## Key Features

- USB HID device discovery and enumeration
- Vendor ID and Product ID filtering with mask support
- Device factory pattern for consistent device access
- Integration with OoBDev device abstraction framework
- Attribute-based device configuration

## Installation

```xml
<PackageReference Include="OoBDev.System.IO.UsbHids" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.System.IO.UsbHids;

// Define device with USB HID attribute
[UsbHid(VendorId = 0x1234, ProductId = 0x5678)]
public class MyUsbDevice { }

// Get device using factory
var factory = new UsbHidFactory();
var device = factory.GetDevice(new MyUsbDevice());

// List available devices
var deviceNames = factory.GetDeviceNames();
```

## Configured adapter

`UsbHidEndpointDeviceAdapter` is an options-driven `IDeviceAdapter` (type `UsbHid`) for one HID device chosen by `UsbHidEndpointOptions`: `VendorId` and `ProductId` (decimal), then an optional `SerialNumber`, then an optional `DevicePath` (for devices that report no serial descriptor, such as the Velleman K8055), with `ReadTimeoutMs` and `WriteTimeoutMs`. Reports are read on a dedicated thread into a channel (`UsbHidReadStream`) and pumped into `Input`; `Output` writes one report per flush, and an empty write is ignored.

```csharp
services.AddUsbHidEndpointDeviceAdapter();
services.Configure<UsbHidEndpointOptions>(o => { o.VendorId = 0x1915; o.ProductId = 0xAFDA; });
```

`IUsbHidDiscovery` (`SystemUsbHidDiscovery`) lists connected devices as `UsbHidDeviceDescriptor`. `IUsbHidDevice` and `IUsbHidDeviceFactory` are the test seams; the adapter is unit tested against a pipe-backed fake, and hardware tests are not part of this library.

## License

See repository license file for details.
