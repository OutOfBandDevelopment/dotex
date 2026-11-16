namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>One suppressed address (stored lower-cased).</summary>
public sealed class SuppressionRow
{
    /// <summary>The lower-cased address; the key.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Why it is suppressed: <c>bounce</c>, <c>complaint</c>, <c>unsubscribe</c> or free text.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>UTC ticks when it was added.</summary>
    public long AddedAtTicks { get; set; }
}
