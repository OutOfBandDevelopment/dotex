# OoBDev.DevTerm.Configuration

The DevTerm composition root: CLI/environment/JSON options (`CliOptions`, `CliOptionsValidator`), `.devterm` project files, connection profiles, user data paths (under `DEVTERM_HOME`), themes and preferences, converter tools, capture export and stream monitoring, routing, network discovery (mDNS, SSDP, USR bridge) and the `ServiceCollectionExtensions` that build a session (`DevTermSessionBuilder`, `SessionTab`).

`AddDevTermFrontEnd` currently wires the loopback transport; other transports throw `NotSupportedException` until they are ported as pipe transports.

Not yet ported: the connection editor view model, the BLE/HID/USBTMC/LXI option classes and scanners, and `RoutingLinkFactory`.
