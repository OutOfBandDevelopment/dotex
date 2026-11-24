using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>Decides one request: known target, accepted inbound credential, outbound bearer token. Independent of YARP.</summary>
public sealed partial class AuthGatewayAuthorizer(
    IOptions<AuthGatewayOptions> options,
    IEnumerable<IInboundAuthenticator> authenticators,
    IOutboundTokenProvider tokens,
    ILogger<AuthGatewayAuthorizer> logger)
{
    public async Task<GatewayDecision> AuthorizeAsync(HttpContext context, string? targetName)
    {
        if (string.IsNullOrEmpty(targetName) || !options.Value.Targets.TryGetValue(targetName, out var target))
        {
            LogRefused(targetName ?? "(none)", "no configured target");
            return new GatewayDecision(StatusCodes.Status403Forbidden, "The route has no configured gateway target.");
        }

        GatewayCaller? caller = null;
        foreach (var scheme in target.Inbound)
        {
            caller = authenticators.FirstOrDefault(a => a.Scheme == scheme)?.Authenticate(context);
            if (caller is not null)
            {
                break;
            }
        }

        if (caller is null)
        {
            LogRefused(targetName, "no accepted credential");
            return new GatewayDecision(StatusCodes.Status401Unauthorized, "A valid credential is required.");
        }

        try
        {
            var token = await tokens.GetTokenAsync(targetName, target.Outbound, caller, BearerOf(context), context.RequestAborted).ConfigureAwait(false);
            LogAllowed(targetName, caller.Scheme, caller.Subject);
            return new GatewayDecision(StatusCodes.Status200OK, "Allowed", token, caller);
        }
        catch (TokenRequestException ex)
        {
            LogTokenFailed(targetName, caller.Subject, ex.Message);
            return new GatewayDecision(StatusCodes.Status502BadGateway, "The gateway could not obtain a token for the target.");
        }
    }

    private static string? BearerOf(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header["Bearer ".Length..].Trim() : null;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Gateway allowed target {Target} for {Subject} via {Scheme}")]
    private partial void LogAllowed(string target, InboundScheme scheme, string subject);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Gateway refused target {Target}: {Reason}")]
    private partial void LogRefused(string target, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Gateway token request for target {Target} and {Subject} failed: {Reason}")]
    private partial void LogTokenFailed(string target, string subject, string reason);
}
