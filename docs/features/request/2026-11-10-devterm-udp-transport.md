# Feature request: DevTerm UDP transport (target and listener) and listener sessions

- **Status:** triaged
- **Requested by:** owner (retire dev-term, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm`

## Need

DevTerm has TCP, serial, BLE, USBTMC, VXI-11, MQTT and broker transports but no UDP. Real target hardware speaks UDP (the EByte E810-DTU broadcast and config protocol on port 1901).

## Proposed behavior

- A UDP target mode (send to host and port) and a UDP listener mode (bind and receive).
- Listener decision of 2026-10-13: one session, each datagram shown with its sender.
- A TCP listener opens one tab per accepted peer (decided the same day); build it with this request so listeners share one model. The generic services request ([listeners](../completed/2026-11-10-devterm-generic-service-listeners.md)) is the first user.

## Source (read-only, to be retired)

`C:\repo\oobdev\dev-term`: `docs/design/transports.md` (UDP and listener decisions), `docs/design/proposals/ebyte-e810-dtu-config-protocol.md`; dotex copies in [transports](../../design/Transports/transports.md) and [the EByte protocol](../../protocols/ebyte-e810-dtu-config-protocol.md).

## Acceptance criteria

- Given a UDP listener, when two senders send datagrams, then one session shows each datagram with its sender address.
- Given a TCP listener, when two peers connect, then each gets its own tab and closing one leaves the other.
- Given the loopback transport, then tests run without hardware.

## Existing code checked

Searched `src` for `UdpClient`, `UdpTransport`, `UdpListener`, per-peer tabs: nothing in DevTerm. `Rfc2217ServerBridge` is the nearest listener.

## Security and operations

Listeners bind loopback by default; binding other addresses is explicit. Datagrams are untrusted input: bound sizes.

## Priority

- **Impact:** needed for EByte and similar network devices.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

The EByte byte-count discrepancy (`FD00`/`FD01`) needs a fresh capture of the real unit before the config part; the transport itself does not.

## Triage notes

Origin: dev-term BACKLOG (Transports; UX decisions 2026-10-13).

2026-11-11: triaged. The note above is out of date: the UDP adapter (`UdpEndpointDeviceAdapter`), the listener libraries and the UDP session tab are built (2026-11-09, Core `SessionHost` decision for tabs per peer). The EByte E810-DTU config protocol still needs a fresh capture. Design: [Transports](../../design/Transports/README.md), [MultiSessionUi](../../design/MultiSessionUi/README.md). Backlog: BACKLOG.md, DevTerm.
