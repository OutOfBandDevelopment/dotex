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

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
