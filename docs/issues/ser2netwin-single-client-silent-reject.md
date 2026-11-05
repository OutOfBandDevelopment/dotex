# Ser2NetWin: a second client is accepted and then dropped without a reason

Status: open (reported 2026-11-04 from the Z-Stick Hardware tests) | Code: `C:\repo\mine\proving-grounds\tools\Ser2NetWin\src\OoBDev.Ser2NetWin.Core\BridgeWorker.cs` | Backlog: [BACKLOG.md](../../BACKLOG.md) section 2 (Devices) (migration item in section 6)

## What happened

The Aeotec Z-Stick is exposed by Ser2NetWin as a raw TCP bridge at `overwatch.local:4000` (192.168.0.22). The `OoBDev.ZWave.Tests` Hardware tests connect to it with `TcpDeviceAdapter`.

- First run: `GetVersion`, the relay switch and node enumeration all got no reply.
- A raw probe (connect, send the GetVersion frame `01 03 00 15 E9`, read) failed twice with "An established connection was aborted by the software in your host machine".
- An idle connection stayed open, and a single `0x06` (ACK) byte written to it was accepted. Only a frame with data made the bridge abort.
- After the owner stopped the other program that was using the stick, a retry passed: GetVersion `Z-Wave 2.78`, node enumeration and the relay toggle.
- The three tests passed one by one but failed when run back to back in one process. The cause on our side: `ZWaveStick.Dispose` never closed the TCP adapter, so the first test's socket stayed open until the process exited and the bridge kept rejecting the next connection. Fixed in the tests (commit `6c52ecd2`).

## Cause in the bridge

`BridgeWorker.Accept` allows one client per COM port. For a second connection it logs a warning and calls `client.Dispose()` immediately:

```csharp
if (_client is not null)
{
    _logger.LogWarning("{Bridge}: rejected extra client {Remote}; one client at a time", ...);
    client.Dispose();
    return;
}
```

The listener has already completed the TCP handshake, so the second client sees a successful connect and an idle line. Its first write then fails with a reset. Nothing tells the client why: a SCPI or Z-Wave program sees "no reply" or "connection aborted", the same as a dead serial device. The warning is only in the service log.

A held connection is also not released until the read loop sees `read == 0` or an error, so a client that disappears without a FIN (a crashed program, a dropped network) keeps the port reserved indefinitely.

## Suggested fixes

1. **Say why on the wire.** Before disposing the extra client, write a short text line (for example `ser2net: port busy, held by 192.168.0.5:51234\r\n`) for raw bridges and an RFC 2217 refusal where the protocol has one, then close. A person using `telnet` or a test sees the reason at once.
2. **Optionally stop accepting.** Close the listener while a client is attached, so a second connect is refused at the TCP level (a clear `ConnectionRefused`). Re-open it when the client leaves. This is the simplest honest signal for programs that do not read text.
3. **Release dead clients.** Enable TCP keep-alive on accepted sockets (`SocketOptionName.KeepAlive`, with short time and interval) and expose an idle timeout, so a vanished client frees the port.
4. **Show the holder.** Report the current client's address and age in the TUI, the service status and the mDNS TXT record (`busy=1`), so an operator can see who has the stick.
5. **Optional: take over.** A config flag `ReplaceExistingClient` that disconnects the old client when a new one arrives, for bench use where the newest program should win.

## How to reproduce

1. Start the bridge for the Z-Stick on port 4000.
2. Connect client A and keep it open.
3. Connect client B, send `01 03 00 15 E9`: B gets a reset and no data. The bridge log says "rejected extra client".
4. Close A and repeat B: B gets the `Z-Wave` version reply.

## Done when

Each fix chosen above has a unit test in `Ser2NetWin` (a fake serial link, two clients) and the behaviour is in the project readme.
