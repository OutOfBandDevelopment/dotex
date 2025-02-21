# OoBDev.Communications.Abstractions

## Summary

The OoBDev Communications libraries are an abstraction designed to asynchronously send 
messages over a dedicated channel such as Email and SMS

### Example

The `ICommunicationSender<>` interface is provided as a means to interact with various 
channel providers.  Example channels are hosted for `EmailMessageModel` and `SmsMessageModel`

```csharp

//Include ICommunicationSender<EmailMessageModel> in your service class

public class ExampleClass (
    ICommunicationSender<EmailMessageModel> email
    )
{
    public async Task<string> SendAsync(EmailMessageModel message) =>
        await email.SendAsync(message);
}

```