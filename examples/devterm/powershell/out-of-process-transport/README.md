# PowerShell transport plugin (echo)

A transport plugin as a child process: what the host writes comes back as input. It shows the transport protocol
(`open`, `write`, `close` from the host; `opened`, `data`, `error`, `closed` from the plugin) described in
[out-of-process plugins](../../../../docs/design/Plugins/out-of-process-plugins.md). Copy the folder into the plugins
directory, approve it, and select it with `--transport ps-echo`. Needs `pwsh` on the PATH.
