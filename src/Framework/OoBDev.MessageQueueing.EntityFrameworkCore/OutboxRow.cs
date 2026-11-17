namespace OoBDev.MessageQueueing.EntityFrameworkCore;

/// <summary>A message waiting to be sent. <see cref="Position"/> keeps insert order, which is also send order.</summary>
public sealed class OutboxRow
{
    /// <summary>Identity column giving the order.</summary>
    public long Position { get; set; }

    /// <summary>The message id.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>The registered name of the message type.</summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>The message as JSON.</summary>
    public string Json { get; set; } = string.Empty;

    /// <summary>UTC ticks when the entry was created (ticks, because SQLite cannot order a DateTimeOffset).</summary>
    public long CreatedAtTicks { get; set; }
}
