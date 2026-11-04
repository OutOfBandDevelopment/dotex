using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OoBDev.DevTerm.Web.Tests;
namespace OoBDev.DevTerm.Web.Tests;

[TestClass]
[TestCategory("Unit")]
public class AccessPolicyTests
{
    [TestMethod]
    public void Default_IsLoopbackOnlyAndValid() => Assert.IsNull(AccessPolicy.Validate(new WebOptions()));

    [TestMethod]
    public void NonLoopback_WithoutAllowRemote_IsRejected() =>
        Assert.IsNotNull(AccessPolicy.Validate(new WebOptions { Urls = "https://0.0.0.0:5443", Token = "t", CertificatePath = "c.pfx" }));

    [TestMethod]
    public void NonLoopback_OverPlainHttp_IsRejected() =>
        Assert.IsNotNull(AccessPolicy.Validate(new WebOptions { Urls = "http://0.0.0.0:5080", AllowRemote = true, Token = "t", CertificatePath = "c.pfx" }));

    [TestMethod]
    public void NonLoopback_WithoutToken_IsRejected() =>
        Assert.IsNotNull(AccessPolicy.Validate(new WebOptions { Urls = "https://0.0.0.0:5443", AllowRemote = true, CertificatePath = "c.pfx" }));

    [TestMethod]
    public void NonLoopback_WithoutCertificate_IsRejected() =>
        Assert.IsNotNull(AccessPolicy.Validate(new WebOptions { Urls = "https://0.0.0.0:5443", AllowRemote = true, Token = "t" }));

    [TestMethod]
    public void NonLoopback_WithEverything_IsAllowed() =>
        Assert.IsNull(AccessPolicy.Validate(new WebOptions { Urls = "https://0.0.0.0:5443", AllowRemote = true, Token = "t", CertificatePath = "c.pfx" }));

    [TestMethod]
    public void TokenMatches_OnlyTheExactToken()
    {
        Assert.IsTrue(AccessPolicy.TokenMatches("abc", "abc"));
        Assert.IsFalse(AccessPolicy.TokenMatches("abc", "abd"));
        Assert.IsFalse(AccessPolicy.TokenMatches("abc", null));
    }

    [TestMethod]
    public void GeneratedTokens_AreLongAndDistinct()
    {
        var a = AccessPolicy.GenerateToken();
        Assert.IsGreaterThanOrEqualTo(40, a.Length);
        Assert.AreNotEqual(a, AccessPolicy.GenerateToken());
    }

    [TestMethod]
    public void Origin_MustMatchHostWhenPresent()
    {
        Assert.IsTrue(AccessPolicy.OriginAllowed(string.Empty, "127.0.0.1:5080"));
        Assert.IsTrue(AccessPolicy.OriginAllowed("http://127.0.0.1:5080", "127.0.0.1:5080"));
        Assert.IsFalse(AccessPolicy.OriginAllowed("http://evil.example", "127.0.0.1:5080"));
    }
}
