# Device binary formats

Generated from the `.ksy` files next to each device library by `KsyGuideTests`; do not edit by hand. Sizes and offsets come from the DevTerm importer ([ksy importer](../../../design/UiDefinitions/ksy-importer.md)).

## Formats

### busylight_command

File: `busylight-command.ksy`

Kuando Busylight single-step command report

The 9-byte HID report "apply" sends, per src/DevTerm.Devices.Busylight/BusylightControlSurface.cs and docs/design/features/kuando-busylight-protocol.md. The 64-byte program/batch form is not described here (it had no visible effect on the real device).

**Table 1 — Fields of busylight_command (9 bytes, little-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `report_id` | skip | 1 | expects 00 |
| `next_step` | u1 | 1 | Always sent as 0 |
| `repeat` | u1 | 1 | Always sent as 0 |
| `red` | u1 | 1 |  |
| `green` | u1 | 1 |  |
| `blue` | u1 | 1 |  |
| `on_time` | u1 | 1 | 0x01 solid; 0x50/0x50 and 0x10/0x10 are the two blink rates |
| `off_time` | u1 | 1 |  |
| `audio` | u1 | 1 | bit7 play, bits 3-6 track index, bits 0-2 volume. The importer reads this as one byte |

### de5000

File: `de5000.ksy`

DER EE DE-5000 LCR meter measurement packet (Cyrustek ES51919)

One fixed 17-byte packet the meter streams. Field meaning follows src/DevTerm.Devices.De5000/De5000Framer.cs and docs/design/proposals/de5000-lcr-meter-protocol.md. Sub-byte fields are described in each attribute's doc, since dev-term's importer does not read bit fields yet.

**Table 2 — Fields of de5000 (17 bytes, big-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `header` | skip | 2 | expects 000D |
| `flags` | u1 | 1 | bit0 Hold, bit1 Reference shown, bit2 Delta, bit3 Calibration, bit4 Sorting, bit5 LCR auto, bit6 Auto range, bit7 Parallel (selects Lp/Cp/Rp over Ls/Cs/Rs) |
| `frequency` | u1 | 1 | bits 5-7: 0 100 Hz, 1 120 Hz, 2 1 kHz, 3 10 kHz, 4 100 kHz, 5 DC |
| `tolerance` | u1 | 1 | Sorting-mode tolerance code: 3 +-0.25%, 4 +-0.5%, 5 +-1%, 6 +-2%, 7 +-5%, 8 +-10%, 9 +-20%, 10 -20%/+80% |
| `primary_quantity` | u1 | 1 | 1 L, 2 C, 3 R, 4 DCR (series or parallel per the Parallel flag) |
| `primary_raw` | u2 | 2 | Unsigned count; value = raw * 10^-(primary_unit & 7) |
| `primary_unit` | u1 | 1 | bits 3-7 unit (1 Ohm, 2 kOhm, 3 MOhm, 5 uH, 6 mH, 7 H, 8 kH, 9 pF, 10 nF, 11 uF, 12 mF, 13 %, 14 deg); bits 0-2 decimal places |
| `primary_status` | u1 | 1 | low 4 bits: 0 normal, 1 blank, 2 ----, 3 OL, 7 PASS, 8 FAIL, 9 OPEn, 10 Srt |
| `secondary_quantity` | u1 | 1 | 1 D, 2 Q, 3 ESR, 4 Theta |
| `secondary_raw` | u2 | 2 | Signed (two's complement) only when the unit is % or deg; value = raw * 10^-(secondary_unit & 7) |
| `secondary_unit` | u1 | 1 | Same unit and decimal-place coding as primary_unit |
| `secondary_status` | u1 | 1 | low 3 bits, same table as primary_status |
| `footer` | skip | 2 | expects 0D0A |

### k8055_input_report

File: `k8055-input-report.ksy`

Velleman K8055 unprompted 9-byte input report

Streamed continuously by the board over HID, per src/DevTerm.Devices.K8055/K8055Decoder.cs and docs/design/features/velleman-k8055-protocol.md. The per-bit mapping of digital_in to the five physical input pins is not yet confirmed on hardware.

**Table 3 — Fields of k8055_input_report (9 bytes, little-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `report_id` | skip | 1 | The HID report id, never device data; expects 00 |
| `digital_in` | u1 | 1 | Digital input bitmask (five inputs) |
| `status` | u1 | 1 | Observed constant 0x03; meaning undocumented |
| `analog_in_1` | u1 | 1 |  |
| `analog_in_2` | u1 | 1 |  |
| `counter_1` | u2 | 2 |  |
| `counter_2` | u2 | 2 |  |

### k8055_output_report

File: `k8055-output-report.ksy`

Velleman K8055 Set Analog/Digital Outputs (0x05) report

The 9-byte HID report dev-term sends to drive the outputs, per src/DevTerm.Devices.K8055/K8055ControlSurface.cs and docs/design/features/velleman-k8055-protocol.md. The reset-counter commands (0x03 counter 1, 0x04 counter 2) use the same 9 bytes with command set and everything after it zero. The duration/debounce bytes are always sent as zero (unconfirmed).

**Table 4 — Fields of k8055_output_report (9 bytes, little-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `report_id` | skip | 1 | expects 00 |
| `command` | u1 | 1 | 0x05 set outputs, 0x03 reset counter 1, 0x04 reset counter 2 |
| `digital_out` | u1 | 1 | Digital output bitmask |
| `analog_out_1` | u1 | 1 |  |
| `analog_out_2` | u1 | 1 |  |
| `reserved` | bytes | 4 |  |

### radexone_query_request

File: `radexone-query-request.ksy`

Radex One query request (Read Data, Read Serial/Version, Read Settings, Reset Accumulated)

The outbound packet for the four commands that share a 6-byte extension, per src/DevTerm.Devices.RadexOne/RadexOneFramer.cs (BuildRequest) and RadexOneExtensionCodec.BuildQuery. Both checksums are a word-sum (little-endian 16-bit words, summed, then 0xFFFF - (sum % 0xFFFF)); the envelope's covers the first 10 bytes, the extension's its first 4. Kaitai cannot express that, so a consumer has to compute them.

**Table 5 — Fields of radexone_query_request (18 bytes, little-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `prefix` | skip | 2 | expects 7BFF |
| `type_marker` | u2 | 2 | Constant 0x0020 on every outbound packet |
| `extension_length` | u2 | 2 | 6 for these commands |
| `packet_number` | u2 | 2 |  |
| `reserved` | bytes | 2 |  |
| `envelope_checksum` | u2 | 2 |  |
| `extension.command_code` | u2 | 2 | 0x0800 Read Data, 0x0001 Read Serial/Version, 0x0801 Read Settings, 0x0803 Reset Accumulated |
| `extension.word` | u2 | 2 | 0x000C for the three reads, 0x0001 for Reset Accumulated |
| `extension.checksum` | u2 | 2 |  |

### radexone_read_data_reply

File: `radexone-read-data-reply.ksy`

Radex One Read Data (0x0800) reply

The inbound packet for the Read Data command, per src/DevTerm.Devices.RadexOne/RadexOneFramer.cs and RadexOneExtensionCodec.cs. Both checksums are a word-sum (little-endian 16-bit words, summed, then 0xFFFF - (sum % 0xFFFF)): the envelope's covers the first 10 bytes, the extension's covers the extension's first 20 bytes. Kaitai cannot express that, so neither is validated here.

**Table 6 — Fields of radexone_read_data_reply (34 bytes, little-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `prefix` | skip | 2 | expects 7AFF |
| `type_marker` | u2 | 2 | Constant 0x8020 on every inbound packet |
| `extension_length` | u2 | 2 | 22 for this reply |
| `packet_number` | u2 | 2 |  |
| `reserved` | bytes | 2 |  |
| `envelope_checksum` | u2 | 2 |  |
| `extension.command_code` | u2 | 2 | 0x0800 Read Data (a reply echoes its request's code) |
| `extension.reserved_a` | bytes | 2 |  |
| `extension.constant_0c` | u2 | 2 | 0x000C |
| `extension.reserved_b` | bytes | 2 |  |
| `extension.ambient` | u2 | 2 | Ambient dose rate; units not specified by the source doc |
| `extension.reserved_c` | bytes | 2 |  |
| `extension.accumulated` | u2 | 2 | Accumulated dose |
| `extension.reserved_d` | bytes | 2 |  |
| `extension.cpm` | u2 | 2 | Counts per minute |
| `extension.reserved_e` | bytes | 2 |  |
| `extension.checksum` | u2 | 2 |  |

### radexone_serial_version_reply

File: `radexone-serial-version-reply.ksy`

Radex One Read Serial/Version (0x0001) reply

The inbound packet for Read Serial/Version. The payload layout is inferred, not documented: a real unit's reply (2026-10-02, COM8, docs/test/2026-10-02-14-42-26.md) decodes to exactly the source doc's example string "SN: 180620-0840-008344 v1.8" with the fields below (day 20, month 06, year 18 -> "180620"; batch 0x0348 = 840 -> "0840"; serial 0x2098 = 8344 -> "008344"; version 1.8). The three `unknown_*` fields are not understood. This reply is 42 bytes; a unit with a longer payload would need the variable-length support the importer doesn't have yet. The extension checksum is not the word-sum over the first 28 bytes the other extensions use (it did not reconcile against the source trace), so nothing here validates it.

**Table 7 — Fields of radexone_serial_version_reply (42 bytes, little-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `prefix` | skip | 2 | expects 7AFF |
| `type_marker` | u2 | 2 | Constant 0x8020 |
| `extension_length` | u2 | 2 | 30 for this reply |
| `packet_number` | u2 | 2 |  |
| `reserved` | bytes | 2 |  |
| `envelope_checksum` | u2 | 2 |  |
| `extension.command_code` | u2 | 2 | 0x0001 (a reply echoes its request's code) |
| `extension.reserved_a` | bytes | 2 |  |
| `extension.unknown_a` | u4 | 4 | 0x14 on the real unit; meaning unknown |
| `extension.unknown_b` | u4 | 4 | 0xA411 on the real unit; meaning unknown |
| `extension.serial_number` | u4 | 4 | Last block of the printed serial (008344) |
| `extension.day` | u2 | 2 |  |
| `extension.month` | u1 | 1 |  |
| `extension.year` | u1 | 1 | Two-digit year (18 = 2018) |
| `extension.version_major` | u1 | 1 |  |
| `extension.version_minor` | u1 | 1 |  |
| `extension.batch` | u2 | 2 | Middle block of the printed serial (0840) |
| `extension.unknown_c` | u4 | 4 |  |
| `extension.checksum` | u2 | 2 |  |

### radexone_write_settings_request

File: `radexone-write-settings-request.ksy`

Radex One Write Settings (0x0802) request

The outbound packet that sets the alarm mode and threshold, per src/DevTerm.Devices.RadexOne/RadexOneExtensionCodec.cs (BuildWriteSettings). Checksums are the same word-sum as in radexone-query-request.ksy; the extension's covers its first 14 bytes. The Read Settings reply has the same shape minus the 0x000E word (a zero there) and under an 0x8020 envelope.

**Table 8 — Fields of radexone_write_settings_request (28 bytes, little-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `prefix` | skip | 2 | expects 7BFF |
| `type_marker` | u2 | 2 | Constant 0x0020 |
| `extension_length` | u2 | 2 | 16 |
| `packet_number` | u2 | 2 |  |
| `reserved` | bytes | 2 |  |
| `envelope_checksum` | u2 | 2 |  |
| `extension.command_code` | u2 | 2 | 0x0802 |
| `extension.constant_0e` | u2 | 2 | 0x000E |
| `extension.target_value` | u2 | 2 | 0x0005 |
| `extension.reserved_a` | bytes | 2 |  |
| `extension.alarm_mode` | u1 | 1 |  |
| `extension.threshold` | u2 | 2 |  |
| `extension.reserved_b` | bytes | 3 |  |
| `extension.checksum` | u2 | 2 |  |

### zoom_h4n_command

File: `zoom-h4n-command.ksy`

Zoom H4n RC04/RC2 remote button press

One button press is two writes, per src/DevTerm.Devices.ZoomH4n/ZoomH4nControlSurface.cs: this 2-byte press code, then the shared release code 0x80 0x00 (as a separate write, not part of one 4-byte frame). Before the first press the device needs a wake handshake: send 0x00 repeatedly (about every 30 ms, up to 1024 times) until a reply byte with the high bit set arrives, then send 0xA1 0x80 0x00. Press codes: record 81 00, play 82 00, stop 84 00, ffwd 88 00, rwd 90 00, volUp 80 08, volDown 80 10, recUp 80 20, recDown 80 40, mic 80 01, ch1 80 02, ch2 80 04.

**Table 9 — Fields of zoom_h4n_command (2 bytes, little-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `transport_bits` | u1 | 1 | 0x80 plus 0x01 record, 0x02 play, 0x04 stop, 0x08 ffwd, 0x10 rwd; plain 0x80 for the rest |
| `button_bits` | u1 | 1 | 0x08 volUp, 0x10 volDown, 0x20 recUp, 0x40 recDown, 0x01 mic, 0x02 ch1, 0x04 ch2; 0 for the transport buttons |

### zoom_h4n_status

File: `zoom-h4n-status.ksy`

Zoom H4n RC04/RC2 remote protocol status byte

Each unsolicited byte is a complete, independent status snapshot, per src/DevTerm.Devices.ZoomH4n/ZoomH4nDecoder.cs and docs/design/features/zoom-h4n-remote-protocol.md. Bits are listed most significant first. dev-term imports this as a one-byte frame of eight bit fields (checked against the decoder's documented masks, not yet against a live recorder).

**Table 10 — Fields of zoom_h4n_status (1 bytes, little-endian)**

| Field | Type | Size | Notes |
|-------|------|------|-------|
| `handshake_wake` | b1 | 1 bits | 0x80, the handshake's own wake signal, not a status bit |
| `led2` | b1 | 1 bits | 0x40 |
| `led1` | b1 | 1 bits | 0x20 |
| `mic_led` | b1 | 1 bits | 0x10 |
| `unknown_08` | b1 | 1 bits | 0x08, undocumented |
| `unknown_04` | b1 | 1 bits | 0x04, undocumented |
| `peak` | b1 | 1 bits | 0x02 |
| `record_led` | b1 | 1 bits | 0x01 |

