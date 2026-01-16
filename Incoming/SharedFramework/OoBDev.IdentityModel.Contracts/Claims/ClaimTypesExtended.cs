using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.IdentityModel.Contracts.Claims
{
    public static class ClaimTypesExtended
    {
        public static string? GetClaimValueByType(this IEnumerable<Claim>? claims, string claimType)
        {
            return claims?.FirstOrDefault(c => string.Equals(c.Type, claimType, StringComparison.InvariantCultureIgnoreCase))?.Value;
        }
        public static string[] GetClaimsValueByType(this IEnumerable<Claim>? claims, string claimType)
        {
            return claims?.Where(c => string.Equals(c.Type, claimType, StringComparison.InvariantCultureIgnoreCase))?.Select(c => c.Value).ToArray() ?? Array.Empty<string>();
        }
    }
}