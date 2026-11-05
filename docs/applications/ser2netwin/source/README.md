# ser2net-win

Bridges Windows COM ports to TCP sockets (raw or RFC 2217, optionally advertised over mDNS) - a Windows-native stand-in for Linux `ser2net`.
Runs as a console app or as an installable Windows Service, with a terminal UI for editing port mappings.

Motivating case: Docker Desktop can't pass USB serial devices into containers, so a Z-Wave stick on `COM13`
is exposed as `tcp://host.docker.internal:4000` for `zwave-js-ui` (115200 8N1, raw mode).

Design notes: `_notes/shared/projects/ser2net-win/README.md`.

## Layout

| Project | Purpose |
| --- | --- |
| `src/OoBDev.Ser2NetWin.Core` | Config model/store/validator, `BridgeWorker` (serial <-> TCP pump), hosted service |
| `src/OoBDev.Ser2NetWin.Cli` | `ser2net-win.exe`: commands, service installer (`sc.exe`), Spectre.Console TUI |
| `tests/OoBDev.Ser2NetWin.Tests` | MSTest; fake serial devices live in `TestKit/` |

## Usage

```
ser2net-win [command] [--config <path>]

  run         Run the bridges in this console (default; Ctrl+C to stop)
  config      Interactive editor for port mappings
  list        Show configured mappings
  ports       Show COM ports currently present
  install     Register the Windows Service (admin; --start to also start it)
  uninstall   Stop and remove the service (admin)
  start|stop  Control the installed service (admin)
  status      Show service state
```

## Configuring

### Where the config lives

`appsettings.json` next to the exe by default. Use `--config <path>` on any command to use a different file
(the service install records the path, see below). `ser2net-win list` prints the mappings it found and from which file.

### Settings

Each entry in `Bridges` is one serial device bound to one TCP port:

```json
{
  "Bridges": [
    { "ComPort": "COM13", "BaudRate": 115200, "DataBits": 8, "Parity": "None",
      "StopBits": "One", "TcpPort": 4000, "BindAddress": "0.0.0.0" }
  ]
}
```

| Setting | Default | Notes |
| --- | --- | --- |
| `ComPort` | - | Required. Name as shown by `ser2net-win ports`, e.g. `COM13`. |
| `BaudRate` | `115200` | Any positive rate the device supports. |
| `DataBits` | `8` | 5-8. |
| `Parity` | `None` | `None`, `Odd`, `Even`, `Mark`, `Space`. |
| `StopBits` | `One` | `One`, `OnePointFive`, `Two`. |
| `TcpPort` | - | Required, 1-65535, unique across bridges. |
| `BindAddress` | `0.0.0.0` | Local IP the listener binds to. `0.0.0.0` = all interfaces. |
| `Protocol` | `Raw` | `Raw` = plain TCP. `Rfc2217` = telnet COM-PORT-OPTION, so clients can change baud rate, data/parity/stop bits, DTR/RTS and break. |
| `Name` | COM port name | Instance name used when advertising over mDNS. |
| `MdnsServices` | none | Extra mDNS announcements for this bridge (see below). |
| `ReplaceExistingClient` | `false` | `true` = a new connection disconnects the current client (newest wins) instead of being refused. |

Optional top-level section to announce bridges on the LAN (see mDNS below):

```json
"Mdns": { "Enabled": false, "ServiceType": "_ser2net._tcp" }
```

Add one entry per serial device; each needs its own COM port and its own TCP port. Other sections of the file
(such as `Logging`) are standard .NET logging configuration and are left alone when mappings are saved.

`BindAddress` defaults to `0.0.0.0` so containers can reach the bridge via `host.docker.internal`. The bridge has no
authentication: use `127.0.0.1` if only local programs need it, or restrict the port with a firewall rule.

### Using the interactive editor

```powershell
ser2net-win config                    # edits appsettings.json next to the exe
ser2net-win config --config C:\s2n\ser2net.json
```

The editor shows the current mappings in a table with a menu underneath (arrow keys to move, Enter to select):

1. **Add bridge** / **Edit bridge** walks through COM port (a list of the ports currently present, or type a name for
   a device that is unplugged), baud rate, data bits, parity, stop bits, TCP port and bind address. When editing, the
   current value is listed first and marked `(current)`.
2. **Remove bridge** asks for confirmation.
3. **Save and exit** validates everything (no duplicate COM or TCP ports, values in range) and writes the file.
   **Exit without saving** discards changes, asking first if there are any.

If the Windows Service is installed and running, saving offers to restart it so the new mappings take effect.
The editor needs an interactive terminal; otherwise edit the JSON by hand.

### Applying changes

Config is read once at startup. After editing the file by hand, restart the service
(`ser2net-win stop` then `ser2net-win start`, elevated) or re-run the console command.

## Behaviour

- One independent listener + serial pump per entry; one TCP client at a time per port. An extra connection gets
  `ser2net: port busy, held by <address> for <n>s` (raw bridges) and is closed, after waiting up to 1 s for a client that
  is just reconnecting to release the port. With `ReplaceExistingClient` the new connection wins instead.
- Accepted sockets use TCP keep-alive (about 60 s to notice a vanished peer), and a client that stops accepting data for
  10 s is dropped so it cannot stall the serial reader.
- The serial port stays open when the client disconnects; serial data with no client is discarded.
- USB unplug / missing device: logged, retried every 5 s, client connection is kept. A bridge that can't bind its TCP
  port is stopped without affecting the others.
- Logs go to the console, plus the Windows Event Log when running as a service.
- RFC 2217: with `"Protocol": "Rfc2217"` the bridge negotiates telnet BINARY/SGA and the COM-PORT-OPTION. Client changes to
  line settings apply to the real port and are reset to the configured values when the client disconnects. Modem-state
  (CTS/DSR/CD) changes are reported to clients that ask. Point RFC 2217 clients (pySerial `rfc2217://host:port`,
  ser2net) at that port. Clients such as zwave-js use `tcp://` and need a `Raw` bridge.

## mDNS advertisement

Set `Mdns.Enabled` to `true` (or use "Enable mDNS advertisement" in `ser2net-win config`) to announce each bridge as a
DNS-SD service `_ser2net._tcp` named after the bridge `Name` (default: the COM port). The TXT record carries
`protocol` (`raw` or `rfc2217`), `port` (COM name) and `baud`. It is **off by default**: the bridge has no
authentication, so only enable it on a network you trust. mDNS failures are logged and never stop a bridge. Browse with
`dns-sd -B _ser2net._tcp` or Avahi/Bonjour tools. Services are withdrawn on shutdown.

### Secondary entries

A bridge can announce extra DNS-SD entries besides `_ser2net._tcp`. Add them in the editor (it asks "Add secondary mDNS
entries?" after the name) or in JSON:

```json
"MdnsServices": [
  { "ServiceType": "_zwave._tcp" },
  { "ServiceType": "_other._tcp", "Name": "alt", "Port": 5000, "Txt": { "key": "value" } }
]
```

`ServiceType` is required (`_name._tcp` or `_name._udp`). `Name`, `Port` and `Txt` default to the bridge's name, TCP
port and the standard protocol/port/baud entries. Entries only announce when `Mdns.Enabled` is true.

### Z-Wave JS discovery

Z-Wave JS can find remote controllers by browsing mDNS for `_zwave._tcp` (it needs a `Raw` bridge at 115200 8N1, and
the Z-Wave JS host must be able to receive multicast; in Docker that means host networking). Add a secondary entry; the
instance name is what shows up in Z-Wave JS, like the alias in a ser2net config:

```json
{ "ComPort": "COM13", "TcpPort": 4000, "Name": "my-zwave",
  "MdnsServices": [ { "ServiceType": "_zwave._tcp" } ] }
```

with `"Mdns": { "Enabled": true }`. Source: Z-Wave JS docs, "Remote serial port over TCP".

## Build and test

```powershell
dotnet test
./scripts/publish.ps1            # single-file self-contained exe in ./artifacts/win-x64
```

## Installing as a Windows Service

The service is registered by the exe itself (`ser2net-win install`, which calls `sc.exe`); there is no separate
installer. It needs the published single-file exe, not `dotnet run` and not the .NET tool install.

1. **Publish** with `./scripts/publish.ps1`, then copy the contents of `artifacts\win-x64` to where it should live,
   for example `C:\Program Files\ser2net-win\`. The service points at the exe's location at install time, so don't
   move it afterwards without reinstalling.
2. **Configure** the mappings. From the install folder:
   ```powershell
   .\ser2net-win.exe ports      # confirm the COM port is present
   .\ser2net-win.exe config     # add/edit mappings
   ```
   Writing under `Program Files` needs an elevated terminal. Alternatively keep the config elsewhere and pass
   `--config C:\ProgramData\ser2net-win\appsettings.json` to `config` and to `install`; the path is baked into the
   service so it uses the same file.
3. **Optionally test in the console first**: `.\ser2net-win.exe run`, check the log shows
   `serial port open` and your client can connect, then Ctrl+C.
4. **Install and start**, from an **elevated** terminal (Run as administrator):
   ```powershell
   .\ser2net-win.exe install --start
   ```
   This creates a service named `ser2net-win` that starts automatically at boot, runs as `LocalSystem`, and restarts
   itself after a crash (5 s, 5 s, then 30 s). Add `--config <path>` if you used a non-default config file.
5. **Check it**:
   ```powershell
   .\ser2net-win.exe status         # running / stopped / not installed
   Get-NetTCPConnection -LocalPort 4000 -State Listen
   ```
   Service logs go to the Windows Event Log (Application log, source `ser2net-win`); use Event Viewer or
   `Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='ser2net-win'} -MaxEvents 20`.

### Day-to-day

| Task | Command (elevated) |
| --- | --- |
| Start / stop | `ser2net-win start` / `ser2net-win stop` |
| Change mappings | `ser2net-win config` (offers to restart the service), or edit the JSON and restart |
| Upgrade | `ser2net-win stop`, replace the exe, `ser2net-win start` (no reinstall needed if the path is unchanged) |
| Remove | `ser2net-win uninstall` (stops and deletes the service; config file is left in place) |

### Firewall

If the client is on another machine, or a container can't reach the host, allow the TCP port inbound:

```powershell
New-NetFirewallRule -DisplayName "ser2net-win 4000" -Direction Inbound -Protocol TCP -LocalPort 4000 -Action Allow
```

Scope it to the clients that need it; the bridge itself has no authentication.

### Troubleshooting

- **"must be run from an elevated terminal"**: open PowerShell with Run as administrator for `install`, `uninstall`,
  `start` and `stop`.
- **"needs the published ser2net-win.exe"**: you ran `install` via `dotnet run` or the .NET tool; use the published exe.
- **Service starts but nothing listens**: the log shows `Invalid configuration` (fix duplicate/invalid entries) or
  `bridge failed` for a TCP port already in use (check with `Get-NetTCPConnection -LocalPort <port>`).
- **`serial port unavailable ... retrying`**: the COM port is missing or held by another program (only one process can
  open a COM port; close any terminal or Z-Wave tool using it). The bridge keeps retrying every 5 s.
- **Second client gets "port busy" and is disconnected**: only one TCP client per port is served at a time. Close the
  other client, or set `ReplaceExistingClient`. See `docs/bugs/`.

## Installing as a .NET tool (NuGet)

`dotnet pack src/OoBDev.Ser2NetWin.Cli -c Release -o G:\nuget` produces `OoBDev.Ser2NetWin.Cli.<version>.nupkg`
(a .NET global tool, command `ser2net-win`). Install it with:

```powershell
dotnet tool install -g OoBDev.Ser2NetWin.Cli --version 0.0.4 --add-source G:\nuget
```

Limitations of the tool install:

- `install` (Windows Service registration) refuses to run, because a tool runs through `dotnet` rather than as its own
  exe. Use the single-file exe from `scripts/publish.ps1` for the service; `run`, `config`, `list` and `ports` all work
  from the tool.
- The default `appsettings.json` lives inside the tool's store folder. Pass `--config <path>` to keep your mappings
  somewhere stable.
