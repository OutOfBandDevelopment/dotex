using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>Obtains the bearer token the gateway sends to a target.</summary>
public interface IOutboundTokenProvider
{
    /// <param name="targetName">The configured target.</param>
    /// <param name="outbound">Its outbound settings.</param>
    /// <param name="caller">The authenticated caller; part of the cache key.</param>
    /// <param name="subjectToken">The inbound bearer token, for a token exchange.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <exception cref="TokenRequestException">The token endpoint failed.</exception>
    public Task<string> GetTokenAsync(string targetName, OutboundOptions outbound, GatewayCaller caller, string? subjectToken, CancellationToken cancellationToken = default);
}
