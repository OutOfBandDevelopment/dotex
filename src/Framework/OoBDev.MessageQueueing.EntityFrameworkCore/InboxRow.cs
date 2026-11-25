namespace OoBDev.MessageQueueing.EntityFrameworkCore;

/// <summary>A message a consumer has begun handling.</summary>
public sealed class InboxRow
{
    /// <summary>The consumer name.</summary>
    public string Consumer { get; set; } = string.Empty;

    /// <summary>The message id.</summary>
    public string MessageId { get; set; } = string.Empty;
}
