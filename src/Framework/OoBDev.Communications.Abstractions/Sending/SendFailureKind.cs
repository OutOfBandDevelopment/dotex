namespace OoBDev.Communications.Abstractions;

/// <summary>Why a send failed; only <see cref="Transient"/> and <see cref="RateLimited"/> are retried.</summary>
public enum SendFailureKind
{
    /// <summary>A temporary error (network, 5xx).</summary>
    Transient,

    /// <summary>The request can never succeed (4xx, bad address).</summary>
    Permanent,

    /// <summary>The vendor asked the caller to slow down.</summary>
    RateLimited,

    /// <summary>The provider declined the message before sending.</summary>
    Rejected,
}
