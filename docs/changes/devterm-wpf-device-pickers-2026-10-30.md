# WPF connection editor device pickers (2026-10-30)

## What changed

- `ConnectionEditorWindow` shows the fields of the chosen transport in scrollable sections: serial (detected ports, port, baud, data bits, parity, stop bits, flow control), network (Scan network, detected devices, host, TCP port), HID and USBTMC (detected devices, vendor and product id, serial number, device path), BLE (Scan Bluetooth, detected devices, device id, Explore characteristics, write and notify pickers, UUIDs) and MQTT (user name).
- Pickers use an explicit item template with ellipsis trimming and a tooltip (rule `devterm-constraints`, ComboBox clip).
- `ConnectionDeviceScans` holds the three scan commands (`ScanLxi`, `ScanBle`, `ExploreBle`) as real `ICommand` types over the editor view model; scanners are injectable and the window exposes them as `Scans`.
- `ConnectionDeviceScansTests` (4 Unit tests); the Wpf test project passes (134 Unit tests).

## Not done

- The editor window was not viewed running, and the scans were not run against real devices.
- Profile-button refresh remains in the backlog.
