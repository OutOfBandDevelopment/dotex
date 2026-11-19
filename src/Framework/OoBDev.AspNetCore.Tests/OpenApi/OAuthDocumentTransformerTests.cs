using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.JwtAuthentication.OpenApi;
using OoBDev.TestUtilities;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.OpenApi;

[TestClass]
public class OAuthDocumentTransformerTests
{
    private const string Configuration = """{"authorization_endpoint":"https://sts.test/auth","token_endpoint":"https://sts.test/token"}""";

    public TestContext TestContext { get; set; } = null!;

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        public string? LastUrl { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            LastUrl = request.RequestUri?.ToString();
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        }
    }

    private static OpenIdConfigurationDiscovery Discovery(StubHandler handler) =>
        new(new HttpClient(handler), NullLogger<OpenIdConfigurationDiscovery>.Instance);

    private static async Task<OpenApiDocument> TransformAsync(OAuth2OpenApiOptions options, OpenIdConfigurationDiscovery discovery)
    {
        var document = new OpenApiDocument();
        var context = new OpenApiDocumentTransformerContext { DocumentName = "all", DescriptionGroups = [], ApplicationServices = null! };
        await new OAuthDocumentTransformer(Options.Create(options), discovery, NullLogger<OAuthDocumentTransformer>.Instance)
            .TransformAsync(document, context, CancellationToken.None);
        return document;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Transform_AuthorityWithoutUrls_ReadsThemFromTheDiscoveryDocumentAsync()
    {
        // Stage
        var handler = new StubHandler(HttpStatusCode.OK, Configuration);
        var options = new OAuth2OpenApiOptions { UserReadApiClaim = "api.read", Authority = "https://sts.test/realm/" };
        TestContext.WriteLine("Authority {0}, no URLs configured", options.Authority);

        // Test
        var document = await TransformAsync(options, Discovery(handler));

        // Assert
        TestContext.WriteLine("Requested {0}", handler.LastUrl);
        Assert.AreEqual("https://sts.test/realm/.well-known/openid-configuration", handler.LastUrl);
        var flow = document.Components?.SecuritySchemes?[OAuthDocumentTransformer.SecuritySchemeName].Flows?.AuthorizationCode;
        Assert.AreEqual(new Uri("https://sts.test/auth"), flow?.AuthorizationUrl);
        Assert.AreEqual(new Uri("https://sts.test/token"), flow?.TokenUrl);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Transform_ConfiguredUrls_WinOverTheAuthorityAsync()
    {
        // Stage
        var handler = new StubHandler(HttpStatusCode.OK, Configuration);
        var options = new OAuth2OpenApiOptions
        {
            UserReadApiClaim = "api.read",
            Authority = "https://sts.test",
            AuthorizationUrl = "https://own.test/auth",
            TokenUrl = "https://own.test/token",
        };

        // Test
        var document = await TransformAsync(options, Discovery(handler));

        // Assert
        TestContext.WriteLine("Discovery calls: {0}", handler.Calls);
        Assert.AreEqual(0, handler.Calls, "nothing to discover when both URLs are set");
        Assert.AreEqual(new Uri("https://own.test/auth"), document.Components?.SecuritySchemes?["oauth2"].Flows?.AuthorizationCode?.AuthorizationUrl);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Transform_DiscoveryFails_LeavesTheSchemeOutAndRetriesNextTimeAsync()
    {
        // Stage
        var handler = new StubHandler(HttpStatusCode.InternalServerError, "");
        var discovery = Discovery(handler);
        var options = new OAuth2OpenApiOptions { UserReadApiClaim = "api.read", Authority = "https://sts.test" };

        // Test
        var first = await TransformAsync(options, discovery);
        var second = await TransformAsync(options, discovery);

        // Assert
        TestContext.WriteLine("Discovery calls after two documents: {0}", handler.Calls);
        Assert.IsNull(first.Components?.SecuritySchemes);
        Assert.IsNull(second.Components?.SecuritySchemes);
        Assert.AreEqual(2, handler.Calls, "a failure is not cached");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Get_SuccessfulDiscovery_IsCachedAsync()
    {
        // Stage
        var handler = new StubHandler(HttpStatusCode.OK, Configuration);
        var discovery = Discovery(handler);

        // Test
        var first = await discovery.GetAsync("https://sts.test", CancellationToken.None);
        var second = await discovery.GetAsync("https://sts.test", CancellationToken.None);

        // Assert
        TestContext.WriteLine("Discovery calls: {0}", handler.Calls);
        Assert.AreEqual(1, handler.Calls);
        Assert.AreEqual(first, second);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Get_DocumentWithoutEndpoints_ReturnsNullAsync()
    {
        // Stage
        var discovery = Discovery(new StubHandler(HttpStatusCode.OK, """{"issuer":"https://sts.test"}"""));

        // Test
        var result = await discovery.GetAsync("https://sts.test", CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }
}
