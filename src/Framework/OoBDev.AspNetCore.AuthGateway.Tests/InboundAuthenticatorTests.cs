using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OoBDev.AspNetCore.AuthGateway.Tests;

[TestClass]
public class InboundAuthenticatorTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ApiKey_MalformedConfiguredHash_DoesNotMatchAndDoesNotThrow()
    {
        var host = new GatewayTestHost();
        host.Options.ApiKeys.Add(new ApiKeyEntry { Id = "bad", Sha512 = "zz", Subject = "x" });
        var authenticator = new ApiKeyAuthenticator(Options.Create(host.Options));

        Assert.IsNull(authenticator.Authenticate(GatewayTestHost.Request("other")));
        Assert.AreEqual("caller-one", authenticator.Authenticate(GatewayTestHost.Request(GatewayTestHost.ApiKey))!.Subject);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ClientCertificate_ThumbprintOnAllowList_Authenticates()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=client", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(global::System.DateTimeOffset.UtcNow.AddDays(-1), global::System.DateTimeOffset.UtcNow.AddDays(1));
        var options = new AuthGatewayOptions();
        options.ClientCertificates.Add(new ClientCertificateEntry { Thumbprint = certificate.Thumbprint.ToLowerInvariant(), Subject = "cert-client" });
        var authenticator = new ClientCertificateAuthenticator(Options.Create(options));

        var allowed = GatewayTestHost.Request();
        allowed.Connection.ClientCertificate = certificate;
        var none = GatewayTestHost.Request();

        Assert.AreEqual("cert-client", authenticator.Authenticate(allowed)!.Subject);
        Assert.IsNull(authenticator.Authenticate(none));

        using var other = request.CreateSelfSigned(global::System.DateTimeOffset.UtcNow.AddDays(-1), global::System.DateTimeOffset.UtcNow.AddDays(1));
        var unknown = GatewayTestHost.Request();
        unknown.Connection.ClientCertificate = other;
        Assert.IsNull(authenticator.Authenticate(unknown));
    }
}
