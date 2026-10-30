# DevTerm BLE platform loader test (2026-10-29)

`BlePlatformAdapterLoader`, `BleDeviceScanner` and the BLE option classes were already ported into `OoBDev.DevTerm.Core` Configuration; the backlog line was stale and is removed. Added `BlePlatformAdapterLoaderTests` (1 Unit test): without the Windows backend assembly beside the app, nothing is registered. The success path (loading the real backend) needs the Windows BLE assembly and stays a manual `DevLocal` check.
