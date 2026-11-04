# Z-Wave support

Sourced from `BACKLOG.md`'s "Proposed Ideas" section (added 2026-09-30): "Z-Wave support — ZStick,
ZWave RPi hat."

## Transport: already covered

Both named target devices are serial-attached — a Z-Stick (e.g. Aeotec's) is a USB-serial adapter
exposing the Z-Wave Serial API over a virtual COM port, and Z-Wave Raspberry Pi HATs typically expose
the same Serial API over UART. This needs **no new transport**: `DevTerm.Transports.Serial` as-is
covers it, the same way (per the [LXI proposal](../features/lxi-support.md)) raw-socket LXI
instruments need no new transport either — the new work here is entirely the protocol/decoder/
control-module layer above the byte stream, not how the bytes arrive.

## Protocol: a real Kaitai Struct candidate

The Z-Wave Serial API is a binary, frame-based protocol (start-of-frame byte, length, frame type,
command class, checksum, plus ACK/NAK/CAN handshake bytes) — squarely the shape
[device-control-modules.md](../../../../design/UiDefinitions/device-control-modules.md) already flags Kaitai Struct (`.ksy`) as the
right tool for ("binary response layouts — byte-level fields, conditionals, repeats, bit widths"),
rather than the lightweight text send-template/response-pattern schema `DevTerm.Devices.Scpi` uses for
ASCII query/response gear. [Device manifests](../../../../design/UiDefinitions/device-manifests.md) already reference `.ksy` as part
of its design ("a folder/zip of one, when a binary Kaitai `.ksy` reference is needed") but that
reference is still unimplemented — building Z-Wave support would be the first real forcing function to
actually implement `.ksy` decoding rather than leave it purely speculative in the design docs.

## Scope

Z-Wave is a mesh network protocol: one controller (the stick/HAT) manages potentially many paired
nodes, each exposing its own device class and command classes (Binary Switch, Multilevel Sensor,
Multilevel Switch, and dozens more in the full Z-Wave specification). A complete controller
implementation — inclusion/exclusion, mesh routing, OTA firmware updates — is a large undertaking, well
beyond what any device module built so far has attempted.

Recommended initial scope, matching how every other device module in this project started with one
device's core commands rather than everything at once (K8055's first pass covered digital/analog I/O
and counter reset, leaving several open questions for later; Busylight shipped color/blink/sound and
dropped a confirmed-no-op batch-program format rather than keep chasing it):

- Controller frame encode/decode (SOF/ACK/NAK/CAN handshake, the basic request/response frame shape)
  via a `.ksy` definition.
- Basic control of a single already-paired node's common command classes — Binary Switch, Basic
  Get/Set, Multilevel Switch — as a `UiDefinition`/`IControlSurface` control panel, the same pattern
  K8055/Busylight already use.
- Explicitly **out of scope for a first pass**: node inclusion/exclusion (pairing), mesh
  routing/repair, security (S0/S2) key exchange, OTA firmware. These are real, substantial pieces of
  the Z-Wave spec on their own and shouldn't block shipping basic node control.

## Decisions made building the first pass (2026-10-26)

- **Node capabilities are queried, not declared:** the panel asks the controller (`GetNodeProtocolInfo`) rather than carrying a manifest of each node's command classes. Reading a node's full command-class list (Node Info Frame / `ApplicationUpdate`) is still to do.
- **No `.ksy` runtime:** the frame is simple enough (SOF, length, type, function, payload, XOR checksum) that a hand-written `ZWaveFramer` was less work than wiring a Kaitai-generated parser, so this plugin did not become the `.ksy` forcing function the section above imagined. The `.ksy` question stays open for other binary devices.
- **ACK handling:** typed lines carry a leading ACK (`06`), and the panel's `ZWaveAckResponder` ACKs each data frame while it is open, because the stick retransmits unACKed callbacks and answers the next request with CAN.
- **Registry data is ported, code is not (2026-10-26):** manufacturer/product/command-class/device-class names and the manufacturers' configuration parameters (CC 0x70) are embedded JSON under `src/DevTerm.Devices.ZWave/Data/`, extracted from OpenZWave (`config/`) and PyZwaver (`zwave.py`), both GPLv3. Only identifier facts were taken; no code. The protocol is standard, so per-product configuration parameters are the manufacturer-specific part.

## Completion checklist

What is needed before this proposal can be closed. Tick items as they land, in the same change.

- [x] Acquire a Z-Wave controller (Aeotec Z-Stick S2 on COM13)
- [x] Capture real frames to ground the protocol work ([bench report](../../test/2026-10-26-15-43-09.md))
- [x] Transport and codec with unit tests (no new transport; `ZWaveFramer`, `ZWaveInterpreter`, `ZWaveDecoder`)
- [x] Both front ends, plus `docs/specs/` and `docs/user-guide/` entries ([spec](../../specs/zwave-panel.md), [guide](../../user-guide/zwave-controller.md))
- [x] Real-hardware pass (`docs/test/2026-10-26-15-43-09.md`)
- [x] Build switch (Binary Switch Get/Set, raw send), enumerate, include/exclude/learn/reset, heal, failed-node and event decoding (2026-10-26; byte-exact unit tests)
- [ ] Verify the rest on the real stick and a reachable node: learn, reset, heal, remove failed (network-changing ones need your go-ahead). Switch, enumerate, add and remove are verified.
- [x] Read a node command-class list (RequestNodeInfo, shown by Enumerate)
- [ ] The Z-Wave Raspberry Pi HAT (same Serial API over UART; untested)

## Status

**First pass built and verified on real hardware (2026-10-26).** The `zwave` plugin (`src/DevTerm.Devices.ZWave`) frames and decodes the Serial API, ACKs frames, offers a read-only control panel (Refresh, Node info, Basic Get) and was exercised on an Aeotec Z-Stick S2 (Z-Wave 2.78, home ID `016A1C28`, 26 nodes). Switching node 2 (a plug-in relay) off and on was verified on 2026-10-28 by `RealHardwareZStickTests.Switch_AgainstRealNode_TogglesAndRestores`. Not done: learn/reset/heal/remove-failed on the bench, S0/S2 security, OTA, and the RPi HAT.

**Update, 2026-10-26.** The panel now also enumerates nodes, switches a node, sends raw command-class bytes, and manages the network (include, exclude, learn mode, reset, heal, is-failed, remove-failed), with callbacks decoded into a "last network event". The network-management payloads were written from the Serial API as remembered and are covered only by byte-exact unit tests; nothing here has been run against the stick, and S0/S2 security keys are still not handled.
