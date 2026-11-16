using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Addresses that must not be sent to again, for example after a hard bounce or a spam complaint.</summary>
public interface ISuppressionList
{
    /// <summary>Checks whether an address is suppressed. Comparison ignores case.</summary>
    /// <param name="address">The recipient address.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns><c>true</c> when sending to it must be skipped.</returns>
    Task<bool> IsSuppressedAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>Adds an address; adding one that is already listed keeps the first reason.</summary>
    /// <param name="address">The recipient address.</param>
    /// <param name="reason">Why, for example <c>bounce</c> or <c>complaint</c>.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task AddAsync(string address, string reason, CancellationToken cancellationToken = default);

    /// <summary>Removes an address, for example after the owner confirmed it works again.</summary>
    /// <param name="address">The recipient address.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task RemoveAsync(string address, CancellationToken cancellationToken = default);
}
