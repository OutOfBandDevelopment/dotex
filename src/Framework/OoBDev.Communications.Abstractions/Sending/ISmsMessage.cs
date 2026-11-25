namespace OoBDev.Communications.Abstractions;

/// <summary>A text message (SMS).</summary>
public interface ISmsMessage : IMessage
{
    /// <summary>Gets the recipient phone number in E.164 form, for example <c>+15551234567</c>.</summary>
    string To { get; }

    /// <summary>Gets the sender number or sender id, or <c>null</c> to use the provider default.</summary>
    string? From { get; }

    /// <summary>Gets the text.</summary>
    string Body { get; }
}
