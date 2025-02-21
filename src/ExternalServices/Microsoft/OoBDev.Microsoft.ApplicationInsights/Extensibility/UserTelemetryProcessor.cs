using OoBDev.System.Security.Claims;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace OoBDev.Microsoft.ApplicationInsights.Extensibility;

/// <summary>
/// Telemetry processor that extracts user information from the HTTP context and adds it to telemetry items.
/// </summary>
public class UserTelemetryProcessor : ITelemetryProcessor
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTelemetryProcessor"/> class.
    /// </summary>
    /// <param name="accessor">The accessor for accessing the current HTTP context.</param>
    public UserTelemetryProcessor(
        IHttpContextAccessor accessor
        ) => _accessor = accessor;

    /// <summary>
    /// Processes telemetry items by extracting user information from the HTTP context and adding it to the global properties.
    /// </summary>
    /// <param name="item">The telemetry item to process.</param>
    public void Process(ITelemetry item)
    {
        var user = _accessor.HttpContext?.User;
        if (user != null)
        {
            var objectId = user.Claims.FirstOrDefault(c => c.Type == CommonClaims.ObjectId)?.Value;
            if (!string.IsNullOrWhiteSpace(objectId))
                item.Context.GlobalProperties[$"Claim-{CommonClaims.ObjectId}"] = objectId;

            var userId = user.Claims.FirstOrDefault(c => c.Type == CommonClaims.UserId)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
                item.Context.GlobalProperties[$"Claim-{CommonClaims.UserId}"] = userId;
        }
    }
}
