using System.Threading.Tasks;

namespace OoBDev.WebApi.Provider;

/// <summary>
/// Interface for providing asynchronous operations related to example messages.
/// </summary>
public interface IExampleMessageProvider
{
    /// <summary>
    /// Posts an example message asynchronously.
    /// </summary>
    /// <param name="message">The message object to be posted.</param>
    /// <param name="correlationId">Optional correlation ID for tracking purposes.</param>
    /// <returns>A task representing the asynchronous operation and containing a string result.</returns>
    Task<string> PostAsync(object message, string? correlationId);
}
