# OoBDev.Communications.Web

The HTTP edge of the RFC 8058 one-click unsubscribe from [OoBDev.Communications](../../Framework/OoBDev.Communications/README.Communications.md) (`AddUnsubscribeLinks`).

```csharp
builder.Services.AddUnsubscribeLinks();   // options: OoBDev:Communications:Unsubscribe (EndpointUrl, SigningKey)
app.MapUnsubscribe("/unsubscribe");       // must match EndpointUrl
```

- `POST {pattern}?t={token}` with the form value `List-Unsubscribe=One-Click` suppresses the address in the signed token and answers 200; a bad token or a body that is not the one-click form answers 400 problem details.
- `GET {pattern}?t={token}` shows a button that posts the same form, so a link prefetch never unsubscribes anyone.
- Anonymous by design: the signed token is the credential. Rate-limit it at the host if needed.
