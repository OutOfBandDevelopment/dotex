using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.JwtAuthentication.OpenApi;
using OoBDev.TestUtilities;
using System.Linq;

namespace OoBDev.AspNetCore.Tests.OpenApi;

[TestClass]
public class ConfigureOAuthOpenApiOptionsTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Get_NamedDocument_HasTheOAuthTransformer()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddOptions<OpenApiOptions>("all");
        services.AddSingleton<IConfigureOptions<OpenApiOptions>, ConfigureOAuthOpenApiOptions>();
        using var provider = services.BuildServiceProvider();

        // Test
        var options = provider.GetRequiredService<IOptionsMonitor<OpenApiOptions>>().Get("all");

        // Assert
        var member = typeof(OpenApiOptions).GetMembers(global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Public).Single(m => m.Name == "DocumentTransformers" || m.Name == "_documentTransformers");
        var transformers = (global::System.Collections.IEnumerable)(member is global::System.Reflection.PropertyInfo pi ? pi.GetValue(options)! : ((global::System.Reflection.FieldInfo)member).GetValue(options)!);
        Assert.IsTrue(transformers.Cast<object>().Any(), "the OAuth document transformer is added to a named document");
    }
}
