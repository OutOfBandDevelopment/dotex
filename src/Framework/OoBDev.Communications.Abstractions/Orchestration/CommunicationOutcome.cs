namespace OoBDev.Communications.Abstractions;

/// <summary>What happened to a request.</summary>
public enum CommunicationOutcome
{
    /// <summary>Sent through at least one channel.</summary>
    Sent,

    /// <summary>Held for later: a quiet window or an explicit deferral.</summary>
    Deferred,

    /// <summary>Dropped: unknown target, no channels, or below the minimum priority.</summary>
    Skipped,
}
