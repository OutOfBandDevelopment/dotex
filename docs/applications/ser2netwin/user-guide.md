# ser2net-win

Bridges Windows COM ports to raw TCP or RFC 2217.

## Commands and options

Run the tool with `--help` to list its commands and options.

```console
$ ser2net-win --help
ser2net-win - bridge Windows COM ports to raw TCP sockets

Usage: ser2net-win [command] [--config <path>]

Commands:
  run         Run the bridges in this console (default; Ctrl+C to stop)
  config      Open the interactive editor for port mappings
  list        Show the configured mappings
  ports       Show the COM ports present, with USB vendor, product and serial
  install     Register the Windows Service (admin; add --start to start it)
  uninstall   Stop and remove the Windows Service (admin)
  start       Start the installed service (admin)
  stop        Stop the installed service (admin)
  status      Show the service state

Options:
  -c, --config <path>   JSON config file (default: appsettings.json next to the exe)
```
