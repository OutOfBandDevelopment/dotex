# ser2net-win testing strategy

| Project | Tier | Covers |
| --- | --- | --- |
| `OoBDev.System.IO.Ports.Bridge.Tests` | `Unit` | configuration store and validator, RFC 2217 session, mDNS plan |
| `OoBDev.System.IO.Ports.Bridge.Tests` | `Simulate` | bridge over loopback TCP with fake serial devices: one-client policy, reconnect, RFC 2217 settings |
| `OoBDev.System.IO.Ports.Bridge.Tests` | `Hardware` | a real COM port (`HardwareConnectionTests`); ends inconclusive when the port is missing, busy or has no TX-RX loopback |
| `OoBDev.Ser2NetWin.Cli.Tests` | `Unit` | command-line parsing |

Run: `dotnet test src/Framework/OoBDev.System.IO.Ports.Bridge.Tests` and `dotnet test src/Applications/Ser2NetWin/OoBDev.Ser2NetWin.Cli.Tests`. Result at migration: 39 passed, 5 hardware tests inconclusive (no TX-RX loopback wired), CLI 5 passed.
