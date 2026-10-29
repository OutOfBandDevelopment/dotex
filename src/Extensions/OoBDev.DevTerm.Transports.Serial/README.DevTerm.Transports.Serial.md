# OoBDev.DevTerm.Transports.Serial

`ITransport` over a COM port, including `IComPortControl` (baud, parity, stop bits, DTR/RTS) and per-platform port descriptions (Windows registry, Linux sysfs, macOS).

Register with `AddSerialTransport()`. The read stream notices an unplugged port instead of hanging.
