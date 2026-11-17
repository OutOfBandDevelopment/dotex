using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.JwtAuthentication;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;

namespace OoBDev.AspNetCore.Tests.Authorization;

[TestClass]
public class JwtBearerKeyRefreshTests
{
    private static JwtBearerOptions Options(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddJwtBearerAuthentication(configuration, JwtBearerDefaults.AuthenticationScheme, nameof(JwtBearerOptions));
        return services.BuildServiceProvider().GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Options_WithAuthority_KeepsSigningKeyRefreshOnUnknownKid()
    {
        var options = Options(new() { ["JwtBearerOptions:Authority"] = "https://sts.example/realms/app" });

        Assert.AreEqual("https://sts.example/realms/app", options.Authority);
        Assert.IsTrue(options.RefreshOnIssuerKeyNotFound, "A rotated signing key must trigger a metadata and JWKS reload.");
        Assert.IsTrue(options.RequireHttpsMetadata);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Options_RefreshIntervalsBind()
    {
        var options = Options(new()
        {
            ["JwtBearerOptions:Authority"] = "https://sts.example/realms/app",
            ["JwtBearerOptions:AutomaticRefreshInterval"] = "00:30:00",
            ["JwtBearerOptions:RefreshInterval"] = "00:02:00",
        });

        Assert.AreEqual(TimeSpan.FromMinutes(30), options.AutomaticRefreshInterval);
        Assert.AreEqual(TimeSpan.FromMinutes(2), options.RefreshInterval);
    }
}
