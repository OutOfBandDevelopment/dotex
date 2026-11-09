namespace OoBDev.Communications.MessageQueueing;

/// <summary>Settings for <see cref="EmailMessageHandler"/> (section <c>OoBDev:Communications:EmailQueue</c>).</summary>
public sealed class EmailMessageHandlerOptions
{
    /// <summary>Gets or sets the channel (see <c>OoBDev:Communications:Channels</c>) that queued emails are sent through.</summary>
    public string ChannelName { get; set; } = "email";
}
