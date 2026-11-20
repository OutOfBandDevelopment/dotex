namespace OoBDev.ComplexEvents.EntityFrameworkCore;

/// <summary>A persisted schedule item. Times are UTC ticks so every provider compares them the same way.</summary>
public sealed class ScheduleRow
{
    /// <summary>Idempotency key.</summary>
    public System.Guid Id { get; set; }

    /// <summary>Caller key, unique.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Due time as UTC ticks.</summary>
    public long DueAtTicks { get; set; }

    /// <summary>Event type name.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Serialized event.</summary>
    public byte[] Payload { get; set; } = [];

    /// <summary>Times leased.</summary>
    public int Attempts { get; set; }

    /// <summary>True after the attempt limit.</summary>
    public bool Failed { get; set; }

    /// <summary>Lease expiry as UTC ticks; 0 when not leased.</summary>
    public long LeasedUntilTicks { get; set; }
}
