using OoBDev.System.Security.Claims;
using OoBDev.System.Security;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Telemetry;

internal static class UserClaims
{
    public static IEnumerable<(string Key, string Value)> Read(ICurrentUserAccessor accessor)
    {
        var user = accessor.User;
        if (user is null) yield break;

        var objectId = user.Claims.FirstOrDefault(c => c.Type == CommonClaims.ObjectId)?.Value;
        if (!string.IsNullOrWhiteSpace(objectId))
            yield return ($"Claim-{CommonClaims.ObjectId}", objectId);

        var userId = user.Claims.FirstOrDefault(c => c.Type == CommonClaims.UserId)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
            yield return ($"Claim-{CommonClaims.UserId}", userId);
    }
}
