# Aeotec Z-Stick S2 (Z-Wave USB controller): known configuration

| | |
|---|---|
| Kind | Z-Wave static controller (Serial API), USB with a virtual COM port |
| Transport | Serial, COM13 on the bench |
| Framing | 115200 8N1, DTR/RTS on, no terminator (binary frames) |
| Presenter | `zwave` (`src/DevTerm.Devices.ZWave`) |
| Identity | Z-Wave 2.78 static controller, Serial API 3.7, manufacturer `0x0086` (Aeotec), product `0x0002/0x0001`, home ID `016A1C28`, node 1, SUC |

Frames are `01 | length | 00 request or 01 response | function | payload | checksum`, with single-byte `06` ACK, `15` NAK and `18` CAN. The host ACKs every data frame; a frame left unACKed is retransmitted and the next request is answered with CAN. Verified in the 2026-10-26 bench report (dev-term `docs/test/2026-10-26-15-43-09.md`).
