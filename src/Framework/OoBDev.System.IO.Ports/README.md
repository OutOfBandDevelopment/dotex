# OoBDev.System.IO.Ports

Serial port device abstraction and management library.

## Description

This package provides a factory-based abstraction layer for serial port communication. It simplifies working with serial ports by offering attribute-based configuration and a consistent device interface pattern.

## Key Features

- Serial port device discovery and enumeration
- Attribute-based serial port configuration (baud rate, parity, data bits, stop bits)
- Device factory pattern for consistent access
- Timeout configuration support
- Integration with OoBDev device abstraction framework

## Installation

```xml
<PackageReference Include="OoBDev.System.IO.Ports" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.System.IO.Ports;

// Define device with serial port configuration
[SerialPort(BaudRate = 9600, DataBits = 8, Parity = Parity.None, StopBits = StopBits.One)]
public class MySerialDevice { }

// Get device using factory
var factory = new SerialPortFactory();
var device = factory.GetDevice("COM3", new MySerialDevice());

// List available serial ports
var portNames = factory.GetDeviceNames();
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
