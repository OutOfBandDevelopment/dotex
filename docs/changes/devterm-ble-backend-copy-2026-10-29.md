# Windows BLE backend copied next to the Tui and Web apps (2026-10-29)

`OoBDev.DevTerm.Tui` and `OoBDev.DevTerm.Web` are plain `net10.0`, so they cannot reference `OoBDev.System.IO.Bluetooth.Windows` (Windows TFM). Both now carry the `CopyWindowsBleBackend` target from dev-term: on Windows it publishes the backend as a side build and copies its DLLs (with the WinRT closure) into the app output, where `BlePlatformAdapterLoader` finds `OoBDev.System.IO.Bluetooth.Windows.dll`.

Verified: both projects build and the DLL is in each output folder. Not run: a real BLE connection (DevLocal, no device attached).
