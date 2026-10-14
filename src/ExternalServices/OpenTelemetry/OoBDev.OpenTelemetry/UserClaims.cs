using OoBDev.System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OoBDev.OpenTelemetry;

internal static class UserClaims
{
    public static IEnumerable<(string Key, string Value)> Read(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        if (user is null) yield break;

        var objectId = user.Claims.FirstOrDefault(c => c.Type == CommonClaims.ObjectId)?.Value;
        if (!string.IsNullOrWhiteSpace(objectId))
            yield return ($"Claim-{CommonClaims.ObjectId}", objectId);

        var userId = user.Claims.FirstOrDefault(c => c.Type == CommonClaims.UserId)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
            yield return ($"Claim-{CommonClaims.UserId}", userId);
    }
}
