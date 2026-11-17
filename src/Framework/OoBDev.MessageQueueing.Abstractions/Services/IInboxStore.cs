using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>Remembers which messages a consumer has handled, so a redelivered message is not handled twice.</summary>
public interface IInboxStore
{
    /// <summary>Atomically records that a consumer is handling a message.</summary>
    /// <param name="consumer">The consumer name, so two consumers of one message do not block each other.</param>
    /// <param name="messageId">The broker message id.</param>
    /// <returns><c>true</c> when this call recorded it first; <c>false</c> when it was already recorded.</returns>
    Task<bool> TryBeginAsync(string consumer, string messageId);

    /// <summary>Forgets a message after a failed attempt, so the broker redelivery is handled again.</summary>
    /// <param name="consumer">The consumer name.</param>
    /// <param name="messageId">The broker message id.</param>
    Task ReleaseAsync(string consumer, string messageId);
}
