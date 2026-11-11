# Device IO bug review: open findings

- **Status:** open, found by code reading on 2026-11-10 (reviewed: `OoBDev.System.IO.Pipelines` pump and streams, `OoBDev.System.IO.Ports` read and write streams, `Rfc2217Session`, ISO 9660 dates, `TempFileHandle`; `BridgeWorker` (no defects found), `SerialDeviceAdapter`; `BrokerDeviceAdapter`, `MqttDeviceAdapter`, `BleDeviceAdapter` and `StompConnection`; `MqttNetConnection` read, no defects; `OoBDev.System.Text.Markdown` and `Templating.Xslt` read; `AmqpConnection` read, no defects; not yet reviewed: `System.Text.Html`, `System.Text.Yaml`, CodeAnalysis projects; `WindowsBleConnection` was read and has no defects, it needs WinRT so it is covered only by hardware)
- **Backlog line:** `BACKLOG.md`, bug review of migrated projects
- **Fixed in the same review (tests added):** `DacPacElementNodeBuilder.AllowNavigate` guard now tracks `(model, property)` pairs and the `ObjectIdentifier` check is no longer inverted, `StructuredLogNavigatorFactory` skipped the children of any tree node type it did not list (and threw for other objects), now every tree node is navigated, YAML and JSON navigators threw on keys that are not XML names (now encoded) and YAML on multi-document files, PlantUML HTML block wrote the diagram source unescaped, `BrokerDeviceAdapter.OpenAsync`, `MqttDeviceAdapter.OpenAsync` and `BleDeviceAdapter.OpenAsync` after a broker-side disconnect leaked the old connection and let its late events reach the new pipe (it also reopened while already open), `SerialDeviceAdapter.OpenAsync` leaked the old port and pump when reopened after a device-side close or unplug, `SerialPortWriteStream` async writes now run on the thread pool and honour a canceled token, `WriteDelayStream` array and synchronous writes (threw `NotSupportedException`), ISO 9660 directory-record day taken from the month byte, wrong sign on the GMT offset, culture and machine time zone leaking into volume dates, `TempFileHandle` finalizer able to throw on Linux

## Open

- `OoBDev.System.Text.Markdown` `PlantUmlRenderer` sends every diagram to the public plantuml.com server (a TODO in the code says to make it configurable), so document text leaves the machine; the repo already runs a `plantuml` container (port 8086, `PLANTUML_URL`). Make the URL an option; the default is parked as Q2 in `OPEN-QUESTIONS.md`.


## Done when

Review the projects not yet reviewed (list in Status), then delete this file. `SerialPortReadStream` no longer wakes on `ErrorReceived` (it takes the concrete `SerialPort`, so it is covered only by the Hardware test, not run here).
