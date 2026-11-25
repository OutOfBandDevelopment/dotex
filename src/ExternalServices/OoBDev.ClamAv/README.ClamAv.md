# OoBDev.ClamAv

`IContentScanner` (from `OoBDev.Documents.Abstractions`) over a [ClamAV](https://www.clamav.net/) `clamd` daemon, using the `zINSTREAM` TCP protocol. Plug it into `ScanningBlobContainer` to refuse infected uploads.

## Use

```csharp
services.TryAddClamAvScanner(configuration);                 // ClamAv:*
var scanner = sp.GetRequiredService<IContentScanner>();      // or keyed "clamav"
var result = await scanner.ScanAsync(content, ct);           // result.IsClean, result.Detail = signature name
```

## Rules

- One connection per scan: `zINSTREAM\0`, then chunks of a 4-byte big-endian length plus data, then a zero length. The reply `stream: OK` is clean, `stream: <signature> FOUND` is infected with the signature as `Detail`.
- Anything else (`ERROR`, such as `INSTREAM size limit exceeded`, an empty reply, a refused connection or a timeout) throws, so a caller such as `ScanningBlobContainer` treats the file as not scanned and refuses it. clamd's `StreamMaxLength` bounds the size it accepts.

## Options (`ClamAv`)

| Key | Default | Meaning |
|-----|---------|---------|
| `Host` | `localhost` | clamd host |
| `Port` | 3310 | clamd TCP port |
| `TimeoutSeconds` | 60 | Connect, send and reply time |
| `ChunkSize` | 65536 | Bytes per INSTREAM chunk |

## Tests

`Simulate` tests use a loopback fake clamd; `Integration` tests need the `clamav` container (`containers/testing`, `CLAMAV_HOST`, `CLAMAV_PORT`; first start downloads signatures) and send the EICAR test string.
