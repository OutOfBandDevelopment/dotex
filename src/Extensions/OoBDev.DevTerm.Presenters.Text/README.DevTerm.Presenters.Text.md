# OoBDev.DevTerm.Presenters.Text

Presenters that turn received bytes into viewable text, ported from dev-term `DevTerm.Presenters.Text`.

| Name | Behaviour |
|------|-----------|
| `ascii` | Line-buffered: emits a line on CR, LF or CRLF (one terminator) or at `MaxLineLength` (default 4096, `0` = unbounded). Stateful; implements `IResettablePresenter`. |
| `utf8` | Stateful decoder, so a multi-byte character split across reads decodes once complete. |
| `hex` | Upper-case hex, unspaced. `Parse` accepts spaces and `0x` prefixes. |
| `decimal`, `octal`, `binary` | Space-separated bytes, padded to 1, 3 and 8 digits. |

All implement `IPresenterInput` to turn typed text back into bytes.

```csharp
services.TryAddTextPresenters();
services.Configure<AsciiPresenterOptions>(o => o.MaxLineLength = 1024);
```

Presenters are transient: resolve one per session, since `ascii` and `utf8` hold partial input.
