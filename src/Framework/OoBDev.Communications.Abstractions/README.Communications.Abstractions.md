# OoBDev.Communications.Abstractions

## Summary

The OoBDev Communications libraries are an abstraction designed to asynchronously send 
messages over a dedicated channel such as Email and SMS

### Example

The `ISendService` sends an `IMessage` through a named channel (`OoBDev:Communications:Channels`); the channel's
provider (for example `OoBDev.MailKit` for `email`) does the delivery. Details are in `README.Communications.md`.

```csharp
public class ExampleClass(ISendService send)
{
    public Task<SendResult> SendAsync(EmailMessage message) =>
        send.SendAsync("email", message);
}
```
