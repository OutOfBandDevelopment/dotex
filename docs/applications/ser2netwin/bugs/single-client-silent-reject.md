# A second client is accepted and then dropped without a reason

Status: fixed in 0.0.4 | Reported: 2026-11-04 (Z-Stick hardware tests in the dotex repo) | Code: `src/OoBDev.Ser2NetWin.Core/BridgeWorker.cs`

## Symptom

A client connecting while another one held the COM port saw a successful connect, then a reset on its first write
("An established connection was aborted by the software in your host machine"). To the client this looked the same as a
dead serial device: no reply, no explanation. Back-to-back test runs in one process failed the same way.

## Causes

1. **Silent reject.** `Accept` completed the TCP handshake, then called `Dispose()` on the extra client without
   writing anything. The reason went only to the service log.
2. **Dead clients kept the port.** The read loop ended only on EOF or an exception. Accepted sockets had no keep-alive,
   so a client that vanished without a FIN (crashed program, dropped network) held the port until the OS gave up,
   which can take many minutes.
3. **Close-then-reconnect race.** The slot was freed only after the old client's read returned 0 and its cleanup ran.
   A new connection arriving in that window was rejected even though the old client had closed properly.
4. **A stalled client blocked the serial reader.** Serial data was written to the client from the serial read loop with
   no write timeout. A peer whose receive window filled up stopped all serial reading until it died.
5. **(Found while fixing) RFC 2217 reset ordering.** The old client reset the serial line settings after releasing the
   slot, so a replacement client's settings could be overwritten by the old client's reset.

## Fix (0.0.4)

| Cause | Change |
| --- | --- |
| 1 | Raw bridges write `ser2net: port busy, held by <address> for <n>s` and close. RFC 2217 clients just see the close (no standard refusal). |
| 2 | Keep-alive on accepted sockets: first probe after 30 s, then every 10 s, 3 retries (about 60 s to notice a dead peer). |
| 3 | A new connection waits up to `ClientReleaseGrace` (1 s) for the previous client to finish releasing before it is refused. |
| 4 | `ClientWriteTimeout` (10 s): a write that makes no progress drops that client; the serial loop keeps running. |
| 5 | The serial line reset now runs before the slot is released. |
| option | `ReplaceExistingClient` (per bridge, default `false`): a new connection disconnects the current client instead of being refused. Also in the config editor. |

## Not done

- The holder's address and age are in the rejection message and the service log only. They are not in the TUI, `status`
  or the mDNS TXT record (a live `busy` flag would mean re-announcing on every connect and disconnect).
- No idle timeout for a client that is connected but silent; keep-alive only catches peers that are gone.

## Tests

`tests/OoBDev.Ser2NetWin.Tests/ClientConnectionTests.cs` (fake serial link, two clients): rejection text and close,
twenty immediate close-and-reconnect rounds, `ReplaceExistingClient`, stalled client dropped while serial reading
continues, keep-alive socket options. `BridgeWorkerTests` was updated because a rejected client now receives the message
before the close.

`HardwareConnectionTests.cs` (`[TestCategory("Hardware")]`) runs the same scenarios against a real port: busy message
with the port staying open, 25 close-and-reconnect rounds, `ReplaceExistingClient`, RFC 2217 baud changes read back from
the real driver and reset on disconnect. It uses COM3 by default (`SER2NET_TEST_PORT` overrides) and is inconclusive when
the port is missing or in use. The loopback echo test is inconclusive unless TX is wired to RX. Run only the hardware
tests with `dotnet test --filter TestCategory=Hardware`. Result with the Prolific adapter on COM3: all pass, no new bugs found.
