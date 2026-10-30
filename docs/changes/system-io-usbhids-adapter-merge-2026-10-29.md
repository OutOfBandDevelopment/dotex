# One USB HID adapter (2026-10-29)

`UsbHidDeviceAdapter` (thin, HidSharp stream) is removed. `UsbHidFactory` (attribute-driven discovery used by the hardware tests' `DevicePreflight`) now returns `UsbHidEndpointDeviceAdapter` per matched device, with the vendor id, product id and device path of the enumerated device as options, so both paths share the better-covered adapter (`IUsbHidDevice` seam, read timeout stream). `UsbHidFactoryTests` (4 Unit tests) cover attribute detection and the no-match results.

Verified: UsbHids.Tests 15 passed; `OoBDev.Devices.TestUtilities` and `OoBDev.DevTerm.Core` build. Not run: the device libraries' hardware tests against a real HID device.
