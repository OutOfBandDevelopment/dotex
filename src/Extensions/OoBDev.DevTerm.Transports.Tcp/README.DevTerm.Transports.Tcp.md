# OoBDev.DevTerm.Transports.Tcp

`ITransport` over a TCP socket (raw instruments, serial-to-Ethernet bridges), with optional XON/XOFF flow control and LXI device discovery.

Register with `AddTcpTransport()`; options are validated (`TcpTransportOptionsValidator`).
