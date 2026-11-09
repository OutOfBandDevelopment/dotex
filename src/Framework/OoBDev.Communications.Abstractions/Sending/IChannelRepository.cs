using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Finds channels by name.</summary>
public interface IChannelRepository
{
    /// <summary>Gets a channel by its unique name, or <c>null</c>.</summary>
    Task<IChannel?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
