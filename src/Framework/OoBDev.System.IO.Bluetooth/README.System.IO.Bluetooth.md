# OoBDev.System.IO.Bluetooth

Bluetooth Low Energy (GATT) serial links as an `IDeviceAdapter`, behind a per-OS backend seam.

## Description

`BleDeviceAdapter` (type `Ble`, path = `DeviceId`) is options-driven by `BleDeviceAdapterOptions`: `DeviceId` (opaque, as produced by the backend's `IBleDeviceDiscovery`), `ServiceUuid`, `WriteCharacteristicUuid` and `NotifyCharacteristicUuid` (default: the Nordic UART Service, `NordicUartService`), `ConnectTimeoutMs`, `WriteTimeoutMs` and `MaxWriteChunkSize` (default 20, one ATT packet; `BleWriteChunker` splits larger writes).

Notifications arrive on an event and are written into `Input` under a gate, so concurrent notifications and `Close` never race the pipe writer. The pipe exists before the connect so a greeting sent the instant notifications are enabled is not lost.

The OS lives behind `IBleConnection` and `IBleConnectionFactory`, plus `IBleDeviceDiscovery` and `IBleGattProfileExplorer` (`BleDeviceDescriptor`, `BleGattServiceDescriptor`, `BleGattCharacteristicDescriptor`). `AddBleDeviceAdapter()` registers defaults that fail clearly at connect time (`UnsupportedPlatform*`; discovery returns an empty list); a backend registers its own after that call.

```csharp
services.AddBleDeviceAdapter();
services.AddWindowsBleConnection(); // from OoBDev.System.IO.Bluetooth.Windows
services.Configure<BleDeviceAdapterOptions>(o => o.DeviceId = "BluetoothLE#...");
```

`OoBDev.System.IO.Bluetooth.Windows` (target `net10.0-windows10.0.19041.0`) is the WinRT `Windows.Devices.Bluetooth` backend. A plain net10.0 application cannot reference it and loads it by reflection.

## Verified

19 Unit tests cover the adapter against a mocked connection, the write chunker and the unsupported-platform defaults. The Windows backend builds but has not been run against a peripheral.
