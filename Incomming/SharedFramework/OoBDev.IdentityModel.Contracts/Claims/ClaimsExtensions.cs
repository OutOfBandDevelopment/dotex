using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.IdentityModel.Contracts.Claims
{
    public static class ClaimsExtensions
    {
        public static IEnumerable<Claim> GetAllClaims(this ClaimsPrincipal principal) => 
            principal?.Identities?.SelectMany(c => c.Claims) ?? Enumerable.Empty<Claim>();

        public static IEnumerable<string> GetClaimValues(this ClaimsPrincipal principal, params string[] claims) =>
            from t in claims
            join c in principal.GetAllClaims() on t equals c.Type
            where !string.IsNullOrWhiteSpace(c.Value)
            select c.Value;

        public static string GetClaimValue(this ClaimsPrincipal principal, params string[] claims) =>
            principal.GetClaimValues(claims).FirstOrDefault();

        public static Guid? GetClaimId(this ClaimsPrincipal principal, params string[] claims) =>
           (from v in principal.GetClaimValues(claims)
            let id = Guid.TryParse(v, out var g) ? (Guid?)g : null
            where id.HasValue
            select id).FirstOrDefault();
    }
}
