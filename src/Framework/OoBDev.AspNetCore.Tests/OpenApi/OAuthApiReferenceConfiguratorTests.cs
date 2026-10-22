using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.JwtAuthentication.OpenApi;
using OoBDev.TestUtilities;
using Scalar.AspNetCore;
using System.Linq;

namespace OoBDev.AspNetCore.Tests.OpenApi;

[TestClass]
public class OAuthApiReferenceConfiguratorTests
{
    private static OAuthApiReferenceConfigurator Create(string? audience, string? scope = "api.read") =>
        new(
            Options.Create(new JwtBearerOptions { Audience = audience }),
            Options.Create(new OAuth2OpenApiOptions { UserReadApiClaim = scope!, AuthorizationUrl = "https://sts.test/auth", TokenUrl = "https://sts.test/token" }),
            NullLogger<OAuthApiReferenceConfigurator>.Instance);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Configure_AudienceSet_UsesAuthorizationCodeWithPkceAndTheAudienceAsClientId()
    {
        // Stage
        var options = new ScalarOptions();

        // Test
        Create("my-api").Configure(options);

        // Assert
        var scheme = options.Authentication?.SecuritySchemes?[OAuthDocumentTransformer.SecuritySchemeName] as ScalarOAuth2SecurityScheme;
        var flow = scheme?.Flows?.AuthorizationCode;
        Assert.IsNotNull(flow);
        Assert.AreEqual("my-api", flow.ClientId);
        Assert.AreEqual(Pkce.Sha256, flow.Pkce);
        CollectionAssert.AreEqual(new[] { "api.read" }, flow.SelectedScopes?.ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Configure_NoAudience_LeavesSignInUnconfigured()
    {
        // Stage
        var options = new ScalarOptions();

        // Test
        Create(null).Configure(options);

        // Assert
        Assert.IsFalse(options.Authentication?.SecuritySchemes?.ContainsKey(OAuthDocumentTransformer.SecuritySchemeName) ?? false);
    }
}
