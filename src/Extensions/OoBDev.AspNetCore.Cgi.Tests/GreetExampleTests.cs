using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Cgi.Tests;

// The Go example program of OoBDev.Example.WebApi (cgi-src/greet), served through the CGI middleware with the example's settings.
[TestClass]
public class GreetExampleTests
{
    private static string? FindCgiBin()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "Examples", "OoBDev.Example.WebApi", "cgi-bin");
            if (Directory.Exists(candidate))
                return candidate;
        }
        return null;
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Post_NameWithMarkup_ReturnsEscapedGreetingAsync()
    {
        // Stage
        var root = FindCgiBin();
        var program = OperatingSystem.IsWindows() ? "greet.exe" : "greet.cgi";
        if (root == null || !File.Exists(Path.Combine(root, program)))
            Assert.Inconclusive($"{program} is not built; run src/Examples/OoBDev.Example.WebApi/cgi-src/build.ps1.");

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OoBDev:Cgi:ScriptRoot"] = root,
            ["OoBDev:Cgi:AllowAnonymous"] = "true",
            ["OoBDev:Cgi:Handlers:.exe"] = "",
            ["OoBDev:Cgi:Handlers:.cgi"] = "",
        });
        builder.Services.TryAddCgiServices(builder.Configuration, "OoBDev:Cgi");
        await using var app = builder.Build();
        app.UseCgi();
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        using var client = new HttpClient { BaseAddress = new Uri(address) };

        // Test
        using var form = new FormUrlEncodedContent(new Dictionary<string, string> { ["name"] = "Ada <b>" });
        using var response = await client.PostAsync($"/cgi-bin/{program}", form);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(body, "Hello, Ada &lt;b&gt;!", StringComparison.Ordinal);
    }
}
