# TCP device transport (2026-10-29)

New `OoBDev.System.IO.Tcp` (Framework) with `TcpDeviceAdapter`, an `IDeviceAdapter` over a TCP socket (first transport of phase 9 of the device decoding plan). `TryOpen` connects within a timeout (default 5 s) and returns false on refusal or timeout; disposing the adapter closes the connection. It plugs into `StreamDevice<T>` like the serial and USB HID adapters, so a SCPI instrument on port 5025 or a serial-to-Ethernet bridge needs no bespoke code.

Tests (Unit, 4 in `OoBDev.System.IO.Tcp.Tests`): bytes exchanged with an in-process loopback listener, refused connection, stream before open, blank host. Not verified against a real instrument. Left in phase 9: USBTMC, VXI-11, RFC 2217, BLE and brokers. Readme: `src/Framework/OoBDev.System.IO.Tcp/README.System.IO.Tcp.md`.
