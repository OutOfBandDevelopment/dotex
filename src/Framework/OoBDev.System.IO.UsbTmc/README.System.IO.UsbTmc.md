# OoBDev.System.IO.UsbTmc

USBTMC (USB Test and Measurement Class, instruments such as oscilloscopes and multimeters) as an `IDeviceAdapter`, over LibUsbDotNet and libusb.

## Description

`UsbTmcDeviceAdapter` (type `UsbTmc`) is options-driven by `UsbTmcDeviceAdapterOptions`: `VendorId` and `ProductId` (decimal), optional `SerialNumber` or `DevicePath` (`UsbTmcDeviceLocation`, for example `usb:1-4.2`), read and write timeouts, `MaxTransferSize`, `MaxResponseSize`, `ClearOnOpen`, `RemoteOnOpen`, `ClearHaltOnOpen` and `RequestDelayMs`.

USBTMC has no unsolicited data. A write whose SCPI mnemonic ends with `?` (also after a parameter, or in a `;` chain) is a query: the adapter writes the command, requests the reply (`REQUEST_DEV_DEP_MSG_IN`) and pumps it into `Input`. Replies follow USBTMC 1.0 section 3.3: multi-transfer messages, transfers that span several bulk-IN reads, stale replies skipped by `bTag`, and abort (`INITIATE_ABORT_BULK_IN/OUT`, then `INITIATE_CLEAR`) after a timeout or protocol error. `UsbTmcDeviceQuirks` holds per-vendor behavior (Rigol request delay, remote control support).

```csharp
services.AddUsbTmcDeviceAdapter();
services.Configure<UsbTmcDeviceAdapterOptions>(o => { o.VendorId = 0x1AB1; o.ProductId = 0x04CE; });
```

`IUsbTmcDeviceDiscovery` lists connected devices. `IUsbTmcDevice` and `IUsbTmcDeviceFactory` are the test seams. On Windows the device needs a WinUSB driver (rebind it with Zadig), and the build copies `libusb-1.0.dll` from the `libusb-dynamic` package.

## Verified

43 Unit tests cover the codec, the device location parser and the adapter against a fake device (queries, multi-transfer replies, stale replies, timeouts and aborts). The hardware behavior notes came from a Rigol DS1102E in dev-term and were not re-run here.
