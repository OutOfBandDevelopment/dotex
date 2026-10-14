using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.System.Security.Claims;

/// <summary>
/// Extensions to manage <seealso cref="Claim"/> on <seealso cref="ClaimsPrincipal"/>
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Iterate all <seealso cref="Claim"/> for provided <seealso cref="ClaimsPrincipal"/>
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static IEnumerable<Claim> GetAllClaims(this ClaimsPrincipal principal) =>
       principal?.Identities?.SelectMany(c => c.Claims) ?? [];

    /// <summary>
    /// Get for matched <seealso cref="Claim"/> on <seealso cref="ClaimsPrincipal"/> 
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    public static IEnumerable<(string claim, string value)> GetClaimValues(this ClaimsPrincipal principal, params string[] claims) =>
        claims
            .Join(principal.GetAllClaims(), t => t, c => c.Type, (t, c) => (claim: t, value: c.Value), StringComparer.Ordinal)
            .Where(i => !string.IsNullOrWhiteSpace(i.value));

    /// <summary>
    /// Get first matched <seealso cref="Claim"/> on <seealso cref="ClaimsPrincipal"/> 
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    public static (string claim, string value)? GetClaimValue(this ClaimsPrincipal principal, params string[] claims) =>
        principal.GetClaimValues(claims).FirstOrDefault();
}
