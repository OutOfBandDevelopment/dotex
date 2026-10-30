# Z-Wave controller panel

**Purpose.** The **Device > Z-Wave controller...** panel (TUI `ControlPanelMode`, WPF `ControlPanelWindow`, web `Web:Panel=zwave`) shows what a Z-Wave USB controller (such as the Aeotec Z-Stick S2) says about itself and its network, enumerates and queries nodes, switches a node, and manages the network (add, remove, reset, learn). Network-changing buttons change real state; the erasing ones ask first. It is contributed by the `zwave` plugin (`src/DevTerm.Devices.ZWave`) and is available on the `serial` transport only. The same plugin's `zwave` presenter decodes the Serial API frames in the terminal output.

The panel is a generic render of `ZWaveUiDefinition`, driven by `ZWaveControlSurface` (an `IControlSurface` and `ICommandPreview`) on the live session.

## Fields

| Section | Control | Kind | Notes |
| --- | --- | --- | --- |
| Controller | Library, Serial API, Manufacturer, Product, Functions, Home ID, Controller node, Role, Chip, SUC node | indicators | Filled from the decoder's `ValuesChanged` after Refresh. |
| Network | Node count, Nodes | indicators | Nodes shown as ranges (`1-12,14-21,...`), from the init-data node bitmap. |
| Query a node | Node | numeric 1-232, default 2 | Target of the two buttons below. Out of range is rejected with an error (`ArgumentException`). |
| Query a node | Last send, Last report | indicators | The last `SendData` callback result and the last command a node reported. |
| Enumerate | Nodes | indicator | An aligned table, one row per node: **Node**, **Device** (listening/sleeping, routing, device class, failed state), **Product** (manufacturer and product from the id registry, e.g. `GE (Jasco Products) 45603 Plugin Appliance Module`), **Version**, **Classes** (a count; the latest report is in Last report). |
| Manufacturer parameters | Configuration parameters | indicator | Filled when a node's Manufacturer Specific report arrives (Describe node, Enumerate): the node, its product, then one line per known manufacturer configuration parameter - number, label, size in bytes, range, default and named values - or a note that the product has none (a plain relay such as the GE 45603 has none). Data is the embedded registry keyed by manufacturer id, product type and product id. |
| Control a node | Configuration | text | `parameter` to read, or `parameter value [size]` to write (size 1, 2 or 4 bytes; default is the smallest that fits). Non-numeric input is rejected without sending. |

## Actions

| Button | Sends | Notes |
| --- | --- | --- |
| Refresh | GetVersion (0x15), MemoryGetId (0x20), GetCapabilities (0x07), GetControllerCapabilities (0x05), GetInitData (0x02), GetSucNodeId (0x56), 150 ms apart | Each request is `06` (ACK) + the framed request. |
| Node info | GetNodeProtocolInfo (0x41) for the Node value | Answered by the stick from its own table; works for sleeping nodes. |
| Describe node | RequestNodeInfo (0x60), then SendData Manufacturer Specific Get (72 04), Version Get (86 11) and, for classes the node lists, Z-Wave Plus Info, Association Groupings, Multi Channel End Point and Security Commands Supported Gets | Fills Last report, the Nodes table and Manufacturer parameters. Times out with a message when the node answers nothing. |
| Configuration get / set | SendData Configuration Get (70 05 param) / Set (70 04 param size value, big-endian) to the Node value | Set changes the device's stored configuration. The reply shows as Report parameter N = V. |
| Basic Get | SendData (0x13): Basic Get (class 0x20, command 0x02), transmit options 0x25, next callback id (1-255) | Radio traffic; a sleeping or unreachable node gives `no acknowledgement from the node`. |

### Enumerate, control and network management (added 2026-10-26)

| Section | Control | Sends | Notes |
| --- | --- | --- | --- |
| Enumerate | Enumerate nodes | MemoryGetId, GetInitData, then per node GetNodeProtocolInfo (0x41), IsFailedNode (0x62), RequestNodeInfo (0x60), SendData Manufacturer Specific Get (72 04) and Version Get (86 11) | Fills the **Nodes** indicator, one block per node: device class, listening/routing, failed state, command classes, manufacturer, versions, last report. A sleeping battery node only answers when it wakes. Needs the zwave presenter on the session. |
| Control a node | Switch on / Switch off / Switch get | SendData Binary Switch Set `25 01 FF` / `25 01 00` / Get `25 02` to the **Node** value | Actuates a real device. |
| Control a node | Command class payload (hex) + Send to node | SendData with the typed bytes | Anything else, e.g. `26 01 32` for Multilevel Set 50%. Non-hex input is rejected without sending. |
| Network management | Add device / Stop adding | AddNodeToNetwork (0x4A) mode `0xC1` (any node, high power, network wide) / `0x05` stop | Put the device in inclusion mode; progress shows in **Last network event** and the terminal. |
| Network management | Remove device / Stop removing | RemoveNodeFromNetwork (0x4B) mode `0xC1` / `0x05` | Same for exclusion. |
| Network management | Join another network (secondary) / Stop learn mode | SetLearnMode (0x50) mode 1 / 0 | Asks first. Makes the stick a secondary controller of the network that includes it. |
| Network management | Heal node, Request node info, Is node failed?, Remove failed node | RequestNodeNeighborUpdate (0x48), RequestNodeInfo (0x60), IsFailedNode (0x62), RemoveFailedNode (0x61) | Remove failed asks first and only removes a node the controller marks failed. |
| Network management | Reset network (new home ID) | SetDefault (0x42) | Asks first. **Erases every pairing** and creates a new home ID. |

**Status.** Verified on the real stick (2026-10-26): Refresh, Add device and Remove device (include and exclude both worked), and reading node 2 (a GE 45603 plug-in appliance module: node info, Manufacturer Specific and Version reports). Switching node 2 off and on is verified too (2026-10-28, `RealHardwareZStickTests.Switch_AgainstRealNode_TogglesAndRestores`: reads the state, flips it, confirms the report, restores it). Covered by byte-exact unit tests but not yet run live: Reset network, learn mode, Heal node, Remove failed node, Describe node end to end, and Configuration get/set.

## States

- Disabled (greyed menu item) unless connected over `serial`.
- While the panel is open a `ZWaveAckResponder` ACKs every data frame the stick sends, off the read loop; without it the stick retransmits unACKed callbacks and answers the next request with CAN. It stops when the panel closes.
- Command preview (`ICommandPreview`) shows the exact bytes for Refresh, Node info and Basic Get without sending.

## Per-front-end notes

None: all three front ends render the definition generically.

## Open items

- Security (S0/S2) keys are not handled, and some network-management commands are still unverified on hardware (see Status); see [the proposal](../design/proposals/z-wave-support.md).
- A Basic Get through the panel to a reachable node has not been exercised on the bench (the paired nodes were asleep or out of range, and nothing is switched without asking).

## No-reply handling

Refresh watches `ZWaveAckResponder.BytesReceived` after its first request. If nothing arrives within 2.5 s it throws a `TimeoutException` ("The Z-Stick did not reply. It talks at 115200 baud, 8N1: ...") instead of sending the rest silently. The usual cause is a connection left at dev-term's 9600 baud default.
