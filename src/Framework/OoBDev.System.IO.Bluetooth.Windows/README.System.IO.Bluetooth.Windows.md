# OoBDev.System.IO.Bluetooth.Windows

The Windows backend for the BLE serial adapter in [OoBDev.System.IO.Bluetooth](../OoBDev.System.IO.Bluetooth/README.System.IO.Bluetooth.md), built on the WinRT `Windows.Devices.Bluetooth` GATT client.

## Description

| Type | Role |
|------|------|
| `WindowsBleConnection` | `IBleConnection`: connects by device id, finds the configured service and characteristics, subscribes to notifications, writes in the chunks the adapter hands over, and turns the subscription off before releasing the handles (a peripheral that accepts one central otherwise stays "connected" and refuses the next link) |
| `WindowsBleConnectionFactory` | `IBleConnectionFactory`, creates a connection per `BleDeviceAdapterOptions` |
| `WindowsBleDeviceDiscovery` | `IBleDeviceDiscovery`: a live advertisement scan (`BluetoothLEAdvertisementWatcher`), not the paired-devices list, because cheap UART modules (HC-08 family) often show as paired in Settings without a completed LE bond and would stay invisible |
| `WindowsBleGattProfileExplorer` | `IBleGattProfileExplorer`: every service and characteristic of a peripheral, on demand and uncached (the GATT table can change between visits) |
| `ServiceCollectionExtensions.AddWindowsBleConnection()` | Replaces the default "unsupported platform" registrations with the three types above |

Connecting needs no pairing: `BluetoothLEDevice.FromIdAsync` and the GATT calls work against an unpaired peripheral.

## Why it is loaded by reflection

The project targets `net10.0-windows10.0.19041.0`, the only kind of target framework that projects the WinRT Bluetooth types. A plain `net10.0` application cannot reference it, so the application looks up `AddWindowsBleConnection` by name after `AddBleDeviceAdapter()` and invokes it. Windows-targeted applications (DevTerm WPF) may reference it directly.

```csharp
services.AddBleDeviceAdapter();
services.AddWindowsBleConnection();
services.Configure<BleDeviceAdapterOptions>(o => o.DeviceId = "BluetoothLE#...");
```

## Verified

`OoBDev.System.IO.Bluetooth.Windows.Tests` holds a `Hardware` round trip through an HC-08 module on a serial port (`HC08_SERIAL_PORT`): it ends `Assert.Inconclusive` when the module is absent, busy or not advertising. The round trip passed against the HC-08 on 2026-11-05. A DE-5000 meter with a BLE radio has not been tested (radio not fitted).
