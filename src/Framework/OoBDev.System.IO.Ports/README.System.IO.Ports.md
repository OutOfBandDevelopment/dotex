# OoBDev.System.IO.Ports

Serial ports as `IDeviceAdapter`s (see [System.IO.Pipelines](../OoBDev.System.IO.Pipelines/README.System.IO.Pipelines.md)), so a COM port plugs into `StreamDevice<T>` like a TCP socket or a USB HID device. There are two routes: an options-driven adapter for applications, and an attribute-driven factory for typed device definitions.

## Use

```csharp
services.AddSerialDeviceAdapter();   // IDeviceAdapter (SerialDeviceAdapter), ISerialPortFactory, ISerialPortDiscovery (validated options with data annotations)
services.Configure<SerialDeviceAdapterOptions>(o => { o.PortName = "COM3"; o.BaudRate = 115200; });
```

**Table 1 — `SerialDeviceAdapterOptions`**

| Option | Default | Meaning |
|--------|---------|---------|
| `PortName` | empty | The port to open (`COM3`, `/dev/ttyUSB0`) |
| `BaudRate`, `DataBits`, `Parity`, `StopBits`, `Handshake` | 9600, 8, None, One, None | Line settings |
| `WriteTimeoutMs` | 5000 | A blocked write (flow control without CTS) fails with `TimeoutException`; -1 waits forever |
| `ReadTimeoutMs` | 1000 | Finite on purpose: read timeouts are treated as "no data yet" so cancellation is noticed promptly |
| `DtrEnable`, `RtsEnable` | true, true | `SerialPort` itself defaults to false; many instruments stay silent without DTR. RTS is left to the handshake when RTS flow control is on |
| `WriteByteDelayMs` | -1 | Pace each written byte by this many milliseconds (`WriteDelayStream`); -1 writes unpaced |

## Components

**Table 2 — Types**

| Type | Role |
|------|------|
| `SerialDeviceAdapter` | The options-driven `IDeviceAdapter` over an `ISerialPort`, including `IComPortControl` (baud, parity, stop bits, DTR/RTS) |
| `ISerialPortFactory`, `SystemSerialPortFactory`, `SystemSerialPort` | Creates the port the adapter opens; tests substitute a fake |
| `ISerialPortDiscovery`, `SystemSerialPortDiscovery` | `GetPortNames()` and best-effort `GetPortDescriptions()` for a picker (Windows registry, Linux sysfs, macOS; `SerialPortDescriptionFormat` shapes the text) |
| `SerialPortReadStream` | Read side that notices an unplugged port instead of hanging and honours cancellation, which `SerialPort.BaseStream.ReadAsync` does not |
| `SerialPortWriteStream` | Write side over `ISerialPort` |
| `SerialPortFactory`, `SerialPortDeviceAdapter` | The attribute route: a definition class marked `[SerialPort(BaudRate = 9600, ...)]` is opened with `GetDevice("COM3", definition)` (null when the port is absent) and `GetDeviceNames()` lists the ports. `SerialPortDeviceAdapter` also reports `BytesToRead` (`IBufferedDeviceAdapter`) |
| `BridgeExtensions` | Converts the abstraction `Parity` and `StopBits` to the `System.IO.Ports` enums |

## Tests

`OoBDev.System.IO.Ports.Tests` covers the adapter (`SerialDeviceAdapterTests`) and discovery (`SerialPortDiscoveryTests`) with fake ports, and `OoBDev.System.Tests/IO/SerialPortDeviceAdapterTests` the attribute route; the bridge tests in `OoBDev.System.IO.Ports.Bridge.Tests` reuse its fake factory.
