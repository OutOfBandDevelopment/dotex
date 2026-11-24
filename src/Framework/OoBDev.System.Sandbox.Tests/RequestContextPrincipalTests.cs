using System;
using System.Security.Claims;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>REQ-012: a request context built from the principal the host's token validator produced.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class RequestContextPrincipalTests
{
    public required TestContext TestContext { get; set; }

    private static ClaimsPrincipal Principal(params Claim[] claims) => new(new ClaimsIdentity(claims, "Bearer"));

    private static readonly DateTimeOffset _start = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void FromPrincipal_ValidatedPrincipal_KeepsOnlyDeclaredClaimsAndBoundsLifetimeByExp()
    {
        // Stage
        var time = new FakeTimeProvider(_start);
        var exp = _start.AddSeconds(60).ToUnixTimeSeconds().ToString(global::System.Globalization.CultureInfo.InvariantCulture);
        var principal = Principal(new Claim("sub", "ann"), new Claim("email", "ann@example.test"), new Claim("secret", "x"), new Claim("exp", exp));

        // Test
        var context = RequestContext.FromPrincipal(principal, "t1", "plugin-a", ["email"], ["docs.read"], time, TimeSpan.FromHours(1), "trace");

        // Assert
        TestContext.WriteLine(context.Canonical);
        Assert.AreEqual("ann", context.Subject);
        CollectionAssert.AreEqual(new[] { "email" }, new global::System.Collections.Generic.List<string>(context.Claims.Keys));
        Assert.AreEqual(_start.AddSeconds(60).ToUnixTimeSeconds(), context.ExpiresAt);
    }

    [TestMethod]
    public void FromPrincipal_NoSubjectExpiredOrAnonymous_IsRefused()
    {
        // Stage
        var time = new FakeTimeProvider(_start);
        var expired = _start.AddSeconds(-1).ToUnixTimeSeconds().ToString(global::System.Globalization.CultureInfo.InvariantCulture);

        // Test and Assert
        Assert.ThrowsExactly<ArgumentException>(() => RequestContext.FromPrincipal(Principal(new Claim("email", "a")), "t", "p", [], [], time, TimeSpan.FromMinutes(1), "x"));
        Assert.ThrowsExactly<ArgumentException>(() => RequestContext.FromPrincipal(Principal(new Claim("sub", "ann"), new Claim("exp", expired)), "t", "p", [], [], time, TimeSpan.FromMinutes(1), "x"));
        Assert.ThrowsExactly<ArgumentException>(() => RequestContext.FromPrincipal(new ClaimsPrincipal(new ClaimsIdentity()), "t", "p", [], [], time, TimeSpan.FromMinutes(1), "x"));
    }
}
